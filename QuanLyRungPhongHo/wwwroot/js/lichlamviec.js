// ========================================
// LICH LAM VIEC - AUTO INIT VERSION
// ========================================

console.log('📦 [lichlamviec.js] Loading...');

// Global state
let employees = [];
let shifts = [];
let currentWeek = new Date();
let draggedEmployee = null;
let dragSource = null;
let draggedElement = null;
let dropSuccessful = false;

// Cấu hình ràng buộc
const MAX_EMPLOYEES_PER_SHIFT = 3; // Tối đa 3 nhân viên/ca
let emergencyMode = false; // Chế độ khẩn cấp để bỏ qua giới hạn

// ========================================
// TỰ ĐỘNG KHỞI TẠO KHI DOM READY
// ========================================

(function () {
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeApp);
    } else {
        // DOM đã sẵn sàng, chạy ngay
        initializeApp();
    }
})();

function initializeApp() {
    console.log('🚀 [INIT] Starting app initialization...');

    // Kiểm tra DOM
    const employeesList = document.getElementById('employeesList');
    const scheduleTable = document.querySelector('.schedule-table');

    if (!employeesList || !scheduleTable) {
        console.error('❌ [INIT] Required DOM elements not found!');
        return;
    }

    console.log('✅ [INIT] DOM elements found');

    // 1. Khởi tạo UI cơ bản NGAY LẬP TỨC
    updateWeekInfo();
    updateDayHeaders();
    setupSearchFilter();
    updateStats();

    // 2. Khởi tạo DRAG & DROP NGAY (không đợi API)
    initDragAndDrop();
    console.log('✅ [INIT] Drag & Drop activated!');

    // 3. Load dữ liệu từ server (chạy sau, không block UI)
    loadDataFromServer();
}

// ========================================
// LOAD DATA (Không block UI)
// ========================================

async function loadDataFromServer() {
    console.log('📡 [DATA] Loading data from server...');

    try {
        // Load song song
        await Promise.all([
            loadEmployees(),
            loadShifts(),
            loadScheduleFromServer()
        ]);

        console.log('✅ [DATA] All data loaded successfully');
    } catch (error) {
        console.error('❌ [DATA] Error loading data:', error);
        showNotification('Không thể tải dữ liệu từ server', 'warning');
    }
}

// ========================================
// DRAG AND DROP - SIMPLIFIED
// ========================================

function initDragAndDrop() {
    console.log('🎯 [DRAG] Initializing drag and drop...');

    setupEmployeeCardsDrag();
    setupScheduleCellsDrop();

    console.log('✅ [DRAG] Drag and drop ready!');
}

function setupEmployeeCardsDrag() {
    const employeeCards = document.querySelectorAll('.employee-card');
    console.log(`📋 [DRAG] Setting up ${employeeCards.length} employee cards`);

    employeeCards.forEach(card => {
        card.addEventListener('dragstart', handleEmployeeDragStart);
        card.addEventListener('dragend', handleEmployeeDragEnd);
    });
}

function handleEmployeeDragStart(e) {
    const card = e.currentTarget;
    console.log('🖱️ [DRAG] Dragging employee:', card.dataset.name);

    draggedEmployee = {
        id: parseInt(card.dataset.id),
        name: card.dataset.name,
        role: card.dataset.role || ''
    };
    dragSource = 'sidebar';
    draggedElement = null;
    dropSuccessful = false;

    card.classList.add('dragging');
    e.dataTransfer.effectAllowed = 'copy';
    e.dataTransfer.setData('text/plain', card.dataset.id);
}

function handleEmployeeDragEnd(e) {
    e.currentTarget.classList.remove('dragging');
}

function setupScheduleCellsDrop() {
    const cells = document.querySelectorAll('.schedule-cell');
    console.log(`📅 [DRAG] Setting up ${cells.length} drop zones`);

    cells.forEach(cell => {
        cell.addEventListener('dragover', handleCellDragOver);
        cell.addEventListener('dragleave', handleCellDragLeave);
        cell.addEventListener('drop', handleCellDrop);
    });
}

function handleCellDragOver(e) {
    e.preventDefault();
    e.dataTransfer.dropEffect = dragSource === 'sidebar' ? 'copy' : 'move';
    e.currentTarget.classList.add('drag-over');
}

function handleCellDragLeave(e) {
    if (e.target === e.currentTarget) {
        e.currentTarget.classList.remove('drag-over');
    }
}

function handleCellDrop(e) {
    e.preventDefault();
    const cell = e.currentTarget;
    cell.classList.remove('drag-over');

    if (!draggedEmployee) {
        console.warn('⚠️ [DRAG] No employee being dragged');
        return;
    }

    const day = cell.dataset.day;
    const shift = cell.dataset.shift;
    console.log(`📍 [DRAG] Dropped ${draggedEmployee.name} to ${day} shift ${shift}`);

    // ✅ RÀNG BUỘC 1: Kiểm tra ngày đã qua
    const cellDate = getCellDate(day);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    
    if (cellDate < today) {
        showNotification('❌ Không thể phân công vào ngày đã qua!', 'error');
        return;
    }

    // ✅ RÀNG BUỘC 2: Kiểm tra trùng lặp trong ca
    const existing = cell.querySelector(`[data-id="${draggedEmployee.id}"]`);
    if (existing) {
        showNotification('Nhân viên đã có trong ca này!', 'warning');
        return;
    }

    // ✅ RÀNG BUỘC 3: Kiểm tra nhân viên đã làm ca khác trong cùng ngày chưa
    const hasOtherShift = checkEmployeeOnSameDay(draggedEmployee.id, day, shift);
    if (hasOtherShift) {
        if (!confirm(`⚠️ ${draggedEmployee.name} đã được phân công ca khác trong ngày này!\n\nBạn có chắc muốn tiếp tục?`)) {
            return;
        }
    }

    // ✅ RÀNG BUỘC 4: Giới hạn số lượng nhân viên trong ca
    const currentCount = cell.querySelectorAll('.assigned-employee').length;
    if (currentCount >= MAX_EMPLOYEES_PER_SHIFT && !emergencyMode) {
        const msg = `⚠️ Ca này đã đủ ${MAX_EMPLOYEES_PER_SHIFT} nhân viên!\n\nBật chế độ khẩn cấp (nút ở góc trên) để vượt giới hạn.`;
        showNotification(msg, 'warning');
        // Highlight nút khẩn cấp
        highlightEmergencyButton();
        return;
    }

    // Thêm nhân viên vào cell
    addEmployeeToCell(cell, draggedEmployee);

    // Nếu di chuyển từ cell khác, xóa khỏi cell cũ
    if (dragSource === 'cell' && draggedElement) {
        draggedElement.remove();
    }

    // Cảnh báo nếu vượt giới hạn (chế độ khẩn cấp)
    if (currentCount + 1 > MAX_EMPLOYEES_PER_SHIFT && emergencyMode) {
        showNotification(`⚠️ Chế độ khẩn cấp: Ca này có ${currentCount + 1}/${MAX_EMPLOYEES_PER_SHIFT} nhân viên`, 'warning');
    }

    updateStats();
    updateCellWarnings();
    console.log('✅ [DRAG] Employee added successfully');
}

function addEmployeeToCell(cell, employee) {
    const employeeEl = document.createElement('div');
    employeeEl.className = 'assigned-employee';
    employeeEl.dataset.id = employee.id;
    employeeEl.dataset.scheduleId = employee.scheduleId || '';
    employeeEl.draggable = true;

    employeeEl.innerHTML = `
        <span class="assigned-employee-name">${employee.name}</span>
        <button onclick="removeEmployee(this)" class="remove-btn" type="button">
            <i class="fas fa-times"></i>
        </button>
    `;

    cell.appendChild(employeeEl);

    // Setup drag cho nhân viên đã phân công
    employeeEl.addEventListener('dragstart', handleAssignedDragStart);
    employeeEl.addEventListener('dragend', handleAssignedDragEnd);
}

function handleAssignedDragStart(e) {
    const empEl = e.currentTarget;

    draggedEmployee = {
        id: parseInt(empEl.dataset.id),
        name: empEl.querySelector('.assigned-employee-name').textContent,
        scheduleId: empEl.dataset.scheduleId
    };
    dragSource = 'cell';
    draggedElement = empEl;
    dropSuccessful = false;

    empEl.classList.add('dragging');
    e.dataTransfer.effectAllowed = 'move';
}

function handleAssignedDragEnd(e) {
    e.currentTarget.classList.remove('dragging');
    draggedElement = null;
}

// ========================================
// API CALLS
// ========================================

async function loadEmployees() {
    try {
        const response = await fetch('/LichLamViec/GetEmployees');
        const data = await response.json();

        if (data.success) {
            employees = data.data;
            console.log(`✅ Loaded ${employees.length} employees`);
            updateStats();
        }
    } catch (error) {
        console.error('❌ Error loading employees:', error);
    }
}

async function loadShifts() {
    try {
        const response = await fetch('/LichLamViec/GetShifts');
        const data = await response.json();

        if (data.success) {
            shifts = data.data;
            console.log(`✅ Loaded ${shifts.length} shifts`);
        }
    } catch (error) {
        console.error('❌ Error loading shifts:', error);
    }
}

async function loadScheduleFromServer() {
    const weekStart = getWeekStart(currentWeek);
    const startDate = weekStart.toISOString().split('T')[0];

    try {
        const response = await fetch(`/LichLamViec/GetSchedule?weekStart=${startDate}`);
        const data = await response.json();

        // Xóa lịch cũ
        document.querySelectorAll('.schedule-cell').forEach(cell => {
            cell.innerHTML = '';
        });

        if (data.success && data.schedule && data.schedule.length > 0) {
            data.schedule.forEach(item => {
                const cell = document.querySelector(
                    `.schedule-cell[data-day="${item.day}"][data-shift="${item.shiftId}"]`
                );

                if (cell && item.employees) {
                    item.employees.forEach(emp => {
                        addEmployeeToCell(cell, {
                            id: emp.id,
                            name: emp.name,
                            scheduleId: emp.scheduleId
                        });
                    });
                }
            });

            console.log('✅ Schedule loaded from server');
        }

        updateStats();
        updateCellWarnings(); // Cập nhật cảnh báo sau khi load
    } catch (error) {
        console.error('❌ Error loading schedule:', error);
    }
}

// ========================================
// WEEK NAVIGATION
// ========================================

async function previousWeek() {
    currentWeek.setDate(currentWeek.getDate() - 7);
    updateWeekInfo();
    updateDayHeaders();
    await loadScheduleFromServer();
    showNotification('Đã chuyển sang tuần trước', 'info');
}

async function nextWeek() {
    currentWeek.setDate(currentWeek.getDate() + 7);
    updateWeekInfo();
    updateDayHeaders();
    await loadScheduleFromServer();
    showNotification('Đã chuyển sang tuần sau', 'info');
}

function updateWeekInfo() {
    const weekStart = getWeekStart(currentWeek);
    const weekEnd = new Date(weekStart);
    weekEnd.setDate(weekEnd.getDate() + 6);

    const weekInfo = document.getElementById('weekInfo');
    if (weekInfo) {
        const weekNumber = getWeekNumber(weekStart);
        weekInfo.textContent = `Tuần ${weekNumber}: ${formatDate(weekStart)} - ${formatDate(weekEnd)}`;
    }
}

function updateDayHeaders() {
    const weekStart = getWeekStart(currentWeek);
    const dayHeaders = document.querySelectorAll('.day-header');
    const dayNames = ['Thứ Hai', 'Thứ Ba', 'Thứ Tư', 'Thứ Năm', 'Thứ Sáu', 'Thứ Bảy', 'Chủ Nhật'];

    dayHeaders.forEach((header, index) => {
        const date = new Date(weekStart);
        date.setDate(date.getDate() + index);

        const dayName = header.querySelector('.day-name');
        const dayDate = header.querySelector('.day-date');

        if (dayName) dayName.textContent = dayNames[index];
        if (dayDate) dayDate.textContent = `${String(date.getDate()).padStart(2, '0')}/${String(date.getMonth() + 1).padStart(2, '0')}`;

        const today = new Date();
        if (date.toDateString() === today.toDateString()) {
            header.classList.add('today');
        } else {
            header.classList.remove('today');
        }
    });
}

// ========================================
// SAVE & CLEAR SCHEDULE
// ========================================

async function saveSchedule() {
    const weekStart = getWeekStart(currentWeek);
    const schedule = [];

    const cells = document.querySelectorAll('.schedule-cell');
    cells.forEach(cell => {
        const day = cell.dataset.day;
        const shiftId = parseInt(cell.dataset.shift);
        const employees = [];

        cell.querySelectorAll('.assigned-employee').forEach(emp => {
            employees.push({
                Id: parseInt(emp.dataset.id),
                ScheduleId: parseInt(emp.dataset.scheduleId) || 0
            });
        });

        if (employees.length > 0) {
            schedule.push({
                Day: day,
                ShiftId: shiftId,
                Employees: employees
            });
        }
    });

    console.log('💾 Saving schedule:', { weekStart, scheduleCount: schedule.length });

    try {
        const response = await fetch('/LichLamViec/SaveSchedule', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                WeekStart: weekStart.toISOString(),
                Schedule: schedule
            })
        });

        const data = await response.json();

        if (data.success) {
            const msg = data.added || data.deleted
                ? `Lưu thành công! Thêm: ${data.added || 0}, Xóa: ${data.deleted || 0}`
                : 'Lưu lịch thành công!';
            showNotification(msg, 'success');
            await loadScheduleFromServer();
        } else {
            showNotification('Lỗi: ' + data.message, 'error');
        }
    } catch (error) {
        console.error('❌ Error saving schedule:', error);
        showNotification('Không thể lưu lịch!', 'error');
    }
}

async function clearSchedule() {
    if (!confirm('Xác nhận xóa toàn bộ lịch tuần này?')) return;

    const weekStart = getWeekStart(currentWeek);

    try {
        const response = await fetch('/LichLamViec/ClearWeekSchedule', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                WeekStart: weekStart.toISOString()
            })
        });

        const data = await response.json();

        if (data.success) {
            // Xóa giao diện
            const cells = document.querySelectorAll('.schedule-cell');
            cells.forEach(cell => {
                cell.innerHTML = '';
            });

            updateStats();
            showNotification(data.message || 'Đã xóa lịch tuần thành công!', 'success');
        } else {
            showNotification('Lỗi: ' + data.message, 'error');
        }
    } catch (error) {
        console.error('❌ Error clearing schedule:', error);
        showNotification('Không thể xóa lịch!', 'error');
    }
}

// ========================================
// UI HELPERS
// ========================================

function updateStats() {
    const assignedIds = new Set();
    document.querySelectorAll('.assigned-employee').forEach(el => {
        assignedIds.add(el.dataset.id);
    });

    const totalEl = document.getElementById('totalEmployees');
    const assignedEl = document.getElementById('assignedCount');

    if (totalEl && employees.length > 0) {
        totalEl.textContent = employees.length;
    } else if (totalEl) {
        const cards = document.querySelectorAll('.employee-card');
        totalEl.textContent = cards.length;
    }

    if (assignedEl) {
        assignedEl.textContent = assignedIds.size;
    }
}

function setupSearchFilter() {
    const searchInput = document.getElementById('searchEmployee');
    if (!searchInput) return;

    searchInput.addEventListener('input', function (e) {
        const searchTerm = e.target.value.toLowerCase();
        const cards = document.querySelectorAll('.employee-card');

        cards.forEach(card => {
            const name = card.querySelector('.employee-name')?.textContent.toLowerCase() || '';
            const role = card.querySelector('.employee-role')?.textContent.toLowerCase() || '';

            if (name.includes(searchTerm) || role.includes(searchTerm)) {
                card.style.display = 'flex';
            } else {
                card.style.display = 'none';
            }
        });
    });
}

function removeEmployee(btn) {
    const empEl = btn.closest('.assigned-employee');
    const empName = empEl.querySelector('.assigned-employee-name').textContent;
    const scheduleId = empEl.dataset.scheduleId;

    if (!confirm(`Xác nhận xóa "${empName}" khỏi ca làm việc?`)) {
        return;
    }

    // Nếu chưa lưu vào DB (scheduleId rỗng), chỉ xóa giao diện
    if (!scheduleId || scheduleId === '0' || scheduleId === '') {
        empEl.remove();
        updateStats();
        showNotification(`Đã xóa ${empName} khỏi ca`, 'info');
        return;
    }

    // Nếu đã có trong DB, gọi API xóa
    fetch('/LichLamViec/DeleteScheduleItem', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            ScheduleId: parseInt(scheduleId)
        })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            empEl.remove();
            updateStats();
            showNotification(`Đã xóa ${empName} khỏi ca`, 'success');
        } else {
            showNotification('Lỗi: ' + data.message, 'error');
        }
    })
    .catch(error => {
        console.error('❌ Error removing employee:', error);
        showNotification('Không thể xóa nhân viên!', 'error');
    });
}

// ========================================
// UTILITY FUNCTIONS
// ========================================

function getWeekStart(date) {
    const d = new Date(date);
    const day = d.getDay();
    const diff = d.getDate() - day + (day === 0 ? -6 : 1);
    return new Date(d.setDate(diff));
}

function getCellDate(dayString) {
    const weekStart = getWeekStart(currentWeek);
    const dayOffset = {
        'mon': 0, 'tue': 1, 'wed': 2, 'thu': 3,
        'fri': 4, 'sat': 5, 'sun': 6
    }[dayString.toLowerCase()] || 0;
    
    const date = new Date(weekStart);
    date.setDate(date.getDate() + dayOffset);
    date.setHours(0, 0, 0, 0);
    return date;
}

function checkEmployeeOnSameDay(employeeId, targetDay, targetShift) {
    // Lấy tất cả các cell trong cùng ngày
    const sameDayCells = document.querySelectorAll(`.schedule-cell[data-day="${targetDay}"]`);
    
    for (const cell of sameDayCells) {
        // Bỏ qua cell đích
        if (cell.dataset.shift === targetShift) continue;
        
        // Kiểm tra xem nhân viên có trong cell này không
        const empInCell = cell.querySelector(`[data-id="${employeeId}"]`);
        if (empInCell) {
            return true;
        }
    }
    return false;
}

function updateCellWarnings() {
    const cells = document.querySelectorAll('.schedule-cell');
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    
    cells.forEach(cell => {
        const day = cell.dataset.day;
        const cellDate = getCellDate(day);
        const count = cell.querySelectorAll('.assigned-employee').length;
        
        // Xóa các class cảnh báo cũ
        cell.classList.remove('cell-past', 'cell-full', 'cell-overfull');
        
        // Ngày đã qua
        if (cellDate < today) {
            cell.classList.add('cell-past');
        }
        // Đã đầy
        else if (count >= MAX_EMPLOYEES_PER_SHIFT && count < MAX_EMPLOYEES_PER_SHIFT + 2) {
            cell.classList.add('cell-full');
        }
        // Quá tải
        else if (count >= MAX_EMPLOYEES_PER_SHIFT + 2) {
            cell.classList.add('cell-overfull');
        }
    });
}

function toggleEmergencyMode() {
    emergencyMode = !emergencyMode;
    const btn = document.getElementById('emergencyBtn');
    
    if (emergencyMode) {
        btn.classList.add('active');
        btn.innerHTML = '<i class="fas fa-exclamation-triangle"></i> Khẩn cấp: BẬT';
        showNotification('🚨 Đã bật chế độ khẩn cấp - Có thể vượt giới hạn nhân viên', 'warning');
    } else {
        btn.classList.remove('active');
        btn.innerHTML = '<i class="fas fa-shield-alt"></i> Khẩn cấp: TẮT';
        showNotification('Đã tắt chế độ khẩn cấp', 'info');
    }
}

function highlightEmergencyButton() {
    const btn = document.getElementById('emergencyBtn');
    if (btn && !emergencyMode) {
        btn.classList.add('highlight-pulse');
        setTimeout(() => btn.classList.remove('highlight-pulse'), 2000);
    }
}

function getWeekNumber(date) {
    const d = new Date(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate()));
    const dayNum = d.getUTCDay() || 7;
    d.setUTCDate(d.getUTCDate() + 4 - dayNum);
    const yearStart = new Date(Date.UTC(d.getUTCFullYear(), 0, 1));
    return Math.ceil((((d - yearStart) / 86400000) + 1) / 7);
}

function formatDate(date) {
    const d = new Date(date);
    return `${String(d.getDate()).padStart(2, '0')}/${String(d.getMonth() + 1).padStart(2, '0')}/${d.getFullYear()}`;
}

function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;

    const icons = {
        'success': 'fa-check-circle',
        'error': 'fa-times-circle',
        'warning': 'fa-exclamation-triangle',
        'info': 'fa-info-circle'
    };

    notification.innerHTML = `
        <i class="fas ${icons[type] || 'fa-info-circle'}"></i>
        <span>${message}</span>
    `;

    document.body.appendChild(notification);
    setTimeout(() => notification.classList.add('show'), 10);

    setTimeout(() => {
        notification.classList.remove('show');
        setTimeout(() => notification.remove(), 300);
    }, 3000);
}

// ========================================
// EXPORT TO WINDOW
// ========================================

window.previousWeek = previousWeek;
window.nextWeek = nextWeek;
window.clearSchedule = clearSchedule;
window.saveSchedule = saveSchedule;
window.removeEmployee = removeEmployee;
window.toggleEmergencyMode = toggleEmergencyMode;

console.log('✅ [lichlamviec.js] Loaded & Auto-initialized');
console.log(`⚙️ Ràng buộc: Tối đa ${MAX_EMPLOYEES_PER_SHIFT} nhân viên/ca`);

// Cập nhật cảnh báo mỗi khi có thay đổi
setInterval(() => {
    updateCellWarnings();
}, 5000); // Kiểm tra mỗi 5 giây