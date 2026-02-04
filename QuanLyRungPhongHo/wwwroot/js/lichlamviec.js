// ========================================
// LICH LAM VIEC - FIXED VERSION
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

// Use global flag instead of local
window.isLichLamViecInitialized = window.isLichLamViecInitialized || false;

// ========================================
// MAIN INITIALIZATION
// ========================================

async function initLichLamViec() {
    console.log('🔵 [INIT] initLichLamViec() called!');

    // Check if already initialized
    if (window.isLichLamViecInitialized) {
        console.log('⚠️ [INIT] Already initialized, skipping...');
        return;
    }

    // Wait for DOM to be ready if not already
    if (document.readyState === 'loading') {
        console.log('⏳ [INIT] DOM still loading, waiting...');
        await new Promise(resolve => {
            document.addEventListener('DOMContentLoaded', resolve, { once: true });
        });
    }

    // Check if required elements exist
    const employeesList = document.getElementById('employeesList');
    const scheduleTable = document.querySelector('.schedule-table');

    console.log('🔍 [INIT] DOM check:', {
        employeesList: !!employeesList,
        scheduleTable: !!scheduleTable
    });

    if (!employeesList || !scheduleTable) {
        console.error('❌ [INIT] Required DOM elements not found!', {
            employeesList: employeesList,
            scheduleTable: scheduleTable
        });
        // Don't mark as initialized so it can be retried
        return;
    }

    console.log('🚀 [INIT] Starting initialization...');
    window.isLichLamViecInitialized = true;

    try {
        // Load employees from API
        // 1. Setup UI & Kéo thả NGAY LẬP TỨC (Không chờ API)
        console.log('🎨 Setting up UI & DragDrop immediately...');
        updateWeekInfo();
        updateDayHeaders();
        setupSearchFilter();

        // Kích hoạt kéo thả ngay vì DOM nhân viên đã có từ Server (ViewBag)
        initDragAndDrop();

        // 2. Sau đó mới tải dữ liệu ngầm (Background)
        console.log('📡 Loading data in background...');

        // Chạy song song các API để tiết kiệm thời gian
        const loadEmpPromise = loadEmployees();
        const loadShiftPromise = loadShifts();
        const loadSchedulePromise = loadScheduleFromServer();

        await Promise.all([loadEmpPromise, loadShiftPromise, loadSchedulePromise]);

        console.log('✅ [INIT] Complete! Data synced.');
    } catch (error) {
        console.error('❌ Init error:', error);
        window.isLichLamViecInitialized = false;
    }
}

// ========================================
// DRAG AND DROP - IMPROVED
// ========================================

function initDragAndDrop() {
    console.log('🎯 [DRAG] Initializing drag and drop...');

    // Setup listeners
    setupEmployeeCardsDrag();
    setupScheduleCellsDrop();
    setupAssignedEmployeesDrag();

    console.log('✅ [DRAG] Drag and drop ready!');
}

function setupEmployeeCardsDrag() {
    const employeeCards = document.querySelectorAll('.employee-card');
    console.log(`📋 [DRAG] Setting up drag for ${employeeCards.length} employee cards`);

    employeeCards.forEach(card => {
        // Skip if already initialized
        if (card.dataset.dragInitialized === 'true') return;
        card.dataset.dragInitialized = 'true';
        card.draggable = true;

        card.addEventListener('dragstart', function (e) {
            console.log('🖱️ [DRAG] Started dragging employee:', this.dataset.name);

            draggedEmployee = {
                id: parseInt(this.dataset.id),
                name: this.dataset.name,
                role: this.dataset.role || ''
            };
            dragSource = 'sidebar';
            draggedElement = null;
            dropSuccessful = false;

            this.classList.add('dragging');
            e.dataTransfer.effectAllowed = 'copy';
            e.dataTransfer.setData('text/plain', this.dataset.id);
        });

        card.addEventListener('dragend', function (e) {
            console.log('🖱️ [DRAG] Ended dragging employee');
            this.classList.remove('dragging');
        });
    });

    console.log(`✅ [DRAG] Employee cards setup complete`);
}

function setupScheduleCellsDrop() {
    const cells = document.querySelectorAll('.schedule-cell');
    console.log(`📅 [DRAG] Setting up drop zones for ${cells.length} cells`);

    cells.forEach(cell => {
        // Skip if already initialized
        if (cell.dataset.dropInitialized === 'true') return;
        cell.dataset.dropInitialized = 'true';

        cell.addEventListener('dragover', function (e) {
            e.preventDefault();
            e.dataTransfer.dropEffect = dragSource === 'sidebar' ? 'copy' : 'move';
            this.classList.add('drag-over');
        });

        cell.addEventListener('dragleave', function (e) {
            // Only remove if leaving the cell itself, not child elements
            if (e.target === this) {
                this.classList.remove('drag-over');
            }
        });

        cell.addEventListener('drop', function (e) {
            e.preventDefault();
            this.classList.remove('drag-over');

            if (!draggedEmployee) {
                console.warn('⚠️ [DRAG] No employee being dragged');
                return;
            }

            const day = this.dataset.day;
            const shift = this.dataset.shift;
            console.log(`📍 [DRAG] Dropped employee ${draggedEmployee.name} to ${day} shift ${shift}`);

            // Check if already exists
            const existing = this.querySelector(`[data-id="${draggedEmployee.id}"]`);
            if (existing) {
                console.log('⚠️ [DRAG] Employee already in this cell');
                showNotification('Nhân viên đã có trong ca này!', 'warning');
                dropSuccessful = false;
                return;
            }

            // Mark drop as successful if moving from another cell
            if (dragSource === 'cell') {
                dropSuccessful = true;
            }

            // Add employee to cell
            addEmployeeToCell(this, draggedEmployee);
            updateStats();
            console.log('✅ [DRAG] Employee added to cell successfully');
        });
    });

    console.log(`✅ [DRAG] Schedule cells setup complete`);
}

function setupAssignedEmployeesDrag() {
    const assignedEmployees = document.querySelectorAll('.assigned-employee');
    console.log(`👥 [DRAG] Setting up drag for ${assignedEmployees.length} assigned employees`);

    assignedEmployees.forEach(empEl => {
        empEl.draggable = true;

        empEl.addEventListener('dragstart', function (e) {
            console.log('🖱️ [DRAG] Started dragging assigned employee');

            draggedEmployee = {
                id: parseInt(this.dataset.id),
                name: this.querySelector('.assigned-employee-name').textContent,
                scheduleId: this.dataset.scheduleId
            };
            dragSource = 'cell';
            draggedElement = this;
            dropSuccessful = false;

            this.classList.add('dragging');
            e.dataTransfer.effectAllowed = 'move';
            e.dataTransfer.setData('text/plain', this.dataset.id);
        });

        empEl.addEventListener('dragend', function (e) {
            console.log('🖱️ [DRAG] Ended dragging assigned employee');
            this.classList.remove('dragging');

            // Only remove if drop was successful
            if (dropSuccessful && draggedElement) {
                console.log('🗑️ [DRAG] Removing old element after successful move');
                draggedElement.remove();
                updateStats();
            }

            // Reset state
            draggedElement = null;
            dropSuccessful = false;
        });
    });

    console.log(`✅ [DRAG] Assigned employees setup complete`);
}

function addEmployeeToCell(cell, employee) {
    const employeeEl = document.createElement('div');
    employeeEl.className = 'assigned-employee';
    employeeEl.dataset.id = employee.id;
    employeeEl.dataset.scheduleId = employee.scheduleId || '';
    employeeEl.draggable = true;

    employeeEl.innerHTML = `
        <span class="assigned-employee-name">${employee.name}</span>
        <button onclick="removeEmployee(this)" class="remove-btn">
            <i class="fas fa-times"></i>
        </button>
    `;

    cell.appendChild(employeeEl);

    // Setup drag for this new element
    setupDragForSingleElement(employeeEl);
}

function setupDragForSingleElement(empEl) {
    empEl.draggable = true;

    empEl.addEventListener('dragstart', function (e) {
        draggedEmployee = {
            id: parseInt(this.dataset.id),
            name: this.querySelector('.assigned-employee-name').textContent,
            scheduleId: this.dataset.scheduleId
        };
        dragSource = 'cell';
        draggedElement = this;
        dropSuccessful = false;

        this.classList.add('dragging');
        e.dataTransfer.effectAllowed = 'move';
    });

    empEl.addEventListener('dragend', function (e) {
        this.classList.remove('dragging');

        if (dropSuccessful && draggedElement) {
            draggedElement.remove();
            updateStats();
        }

        draggedElement = null;
        dropSuccessful = false;
    });
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
            console.log(`✅ Loaded ${employees.length} employees from API`);
            updateStats();
        } else {
            console.error('Failed to load employees:', data);
        }
    } catch (error) {
        console.error('Error loading employees:', error);
        throw error;
    }
}

async function loadShifts() {
    try {
        const response = await fetch('/LichLamViec/GetShifts');
        const data = await response.json();

        if (data.success) {
            shifts = data.data;
            console.log(`✅ Loaded ${shifts.length} shifts`);
        } else {
            console.error('Failed to load shifts:', data);
        }
    } catch (error) {
        console.error('Error loading shifts:', error);
        throw error;
    }
}

async function loadScheduleFromServer() {
    const weekStart = getWeekStart(currentWeek);
    const startDate = weekStart.toISOString().split('T')[0];

    try {
        const response = await fetch(`/LichLamViec/GetSchedule?weekStart=${startDate}`);
        const data = await response.json();

        // Clear old assignments
        document.querySelectorAll('.schedule-cell').forEach(cell => {
            cell.innerHTML = '';
        });

        if (data.success && data.schedule && data.schedule.length > 0) {
            data.schedule.forEach((item) => {
                const cell = document.querySelector(`.schedule-cell[data-day="${item.day}"][data-shift="${item.shiftId}"]`);
                if (cell) {
                    item.employees.forEach(emp => {
                        addEmployeeToCell(cell, {
                            id: emp.id,
                            name: emp.name,
                            scheduleId: emp.scheduleId
                        });
                    });
                }
            });
        }

        // Setup drag for newly added assigned employees
        console.log('🔄 [DRAG] Re-initializing drag for assigned employees after load');
        setupAssignedEmployeesDrag();

        updateStats();
    } catch (error) {
        console.error('Error loading schedule:', error);
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
// SAVE SCHEDULE
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
            headers: {
                'Content-Type': 'application/json',
            },
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

    const cells = document.querySelectorAll('.schedule-cell');
    let count = 0;

    cells.forEach(cell => {
        const assigned = cell.querySelectorAll('.assigned-employee');
        count += assigned.length;
        cell.innerHTML = '';
    });

    updateStats();

    if (count > 0) {
        showNotification(`Đã xóa ${count} ca làm việc!`, 'success');
    } else {
        showNotification('Lịch đã trống!', 'info');
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

    if (assignedEl) assignedEl.textContent = assignedIds.size;
}

function setupSearchFilter() {
    const searchInput = document.getElementById('searchEmployee');
    if (!searchInput) return;

    // Remove old listener
    const newSearch = searchInput.cloneNode(true);
    searchInput.parentNode.replaceChild(newSearch, searchInput);

    newSearch.addEventListener('input', function (e) {
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

    if (confirm(`Xác nhận xóa "${empName}" khỏi ca làm việc?`)) {
        empEl.remove();
        updateStats();
        showNotification(`Đã xóa ${empName} khỏi ca`, 'info');
    }
}

// ========================================
// HELPER FUNCTIONS
// ========================================

function getWeekStart(date) {
    const d = new Date(date);
    const day = d.getDay();
    const diff = d.getDate() - day + (day === 0 ? -6 : 1);
    return new Date(d.setDate(diff));
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

window.initLichLamViec = initLichLamViec;
window.previousWeek = previousWeek;
window.nextWeek = nextWeek;
window.clearSchedule = clearSchedule;
window.saveSchedule = saveSchedule;
window.removeEmployee = removeEmployee;

console.log('✅ [lichlamviec.js] Loaded. window.initLichLamViec is ready.');