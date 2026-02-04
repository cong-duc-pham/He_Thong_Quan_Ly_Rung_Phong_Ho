// ========================================
// PREVENT MULTIPLE INITIALIZATION
// ========================================

// ✅ Kiểm tra nếu đã load rồi thì không load lại
if (window.lichLamViecLoaded) {
    console.warn('⚠️ lichlamviec.js already loaded, skipping...');
    throw new Error('Script already loaded');
}
window.lichLamViecLoaded = true;

// ========================================
// DATA & STATE
// ========================================

let employees = [];
let shifts = [];
let currentWeek = new Date();
let scheduleData = {};
let draggedEmployee = null;
let dragSource = null;
let isInitialized = false;

// ========================================
// INITIALIZATION
// ========================================

async function init() {
    console.log('🔍 init() called');

    // Check if we're on the correct page
    const employeesList = document.getElementById('employeesList');
    if (!employeesList) {
        console.log('❌ Not on LichLamViec page (#employeesList not found), skipping initialization');
        return;
    }

    if (isInitialized) {
        console.log('⚠️ Already initialized, skipping...');
        return;
    }

    console.log('🚀 Initializing LichLamViec page...');

    try {
        // Load data in sequence
        console.log('📡 Step 1: Loading employees...');
        await loadEmployees();

        console.log('📡 Step 2: Loading shifts...');
        await loadShifts();

        // Setup UI elements
        console.log('🎨 Step 3: Setting up UI...');
        updateWeekInfo();
        updateDayHeaders();
        setupSearchFilter();
        initDragAndDrop();

        // Load schedule after everything else is ready
        console.log('📅 Step 4: Loading schedule...');
        await loadScheduleFromServer();

        isInitialized = true;
        console.log('✅ Initialization complete!');
    } catch (error) {
        console.error('❌ Error during initialization:', error);
        showNotification('Lỗi khởi tạo trang!', 'error');
    }
}

// ✅ Export init to window for external access
window.init = init;

// ========================================
// API CALLS
// ========================================

async function loadEmployees() {
    try {
        console.log('Loading employees...');
        const response = await fetch('/LichLamViec/GetEmployees');
        const data = await response.json();

        if (data.success) {
            employees = data.data;
            console.log(`✅ Loaded ${employees.length} employees`);
            renderEmployeeList();
            updateStats();
        } else {
            console.error('Failed to load employees:', data);
            showNotification('Lỗi khi tải danh sách nhân viên', 'error');
        }
    } catch (error) {
        console.error('Error loading employees:', error);
        showNotification('Không thể kết nối server!', 'error');
    }
}

async function loadShifts() {
    try {
        console.log('Loading shifts...');
        const response = await fetch('/LichLamViec/GetShifts');
        const data = await response.json();

        if (data.success) {
            shifts = data.data;
            console.log(`✅ Loaded ${shifts.length} shifts:`, shifts);
        } else {
            console.error('Failed to load shifts:', data);
            showNotification('Lỗi khi tải danh sách ca làm việc', 'error');
        }
    } catch (error) {
        console.error('Error loading shifts:', error);
        showNotification('Không thể tải ca làm việc!', 'error');
    }
}

// ========================================
// LOAD SCHEDULE FROM SERVER - ĐÃ FIX
// ========================================

async function loadScheduleFromServer() {
    const weekStart = getWeekStart(currentWeek);
    const startDate = weekStart.toISOString().split('T')[0];

    console.log(`📡 Loading schedule for week: ${startDate}...`);

    try {
        const url = `/LichLamViec/GetSchedule?weekStart=${startDate}`;
        console.log(`   Calling: ${url}`);

        const response = await fetch(url);

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        console.log("📦 Server returned:", data);

        // Xóa sạch các ô lịch cũ
        document.querySelectorAll('.schedule-cell').forEach(cell => {
            cell.innerHTML = '';
        });

        if (!data.success) {
            console.error('❌ API error:', data.message);
            showNotification('Lỗi khi tải lịch: ' + data.message, 'error');
            return;
        }

        if (!data.schedule || data.schedule.length === 0) {
            console.log("ℹ️ No schedule data for this week");
            return;
        }

        console.log(`📋 Processing ${data.schedule.length} cells...`);

        // Vẽ nhân viên vào các ô
        let totalEmployees = 0;
        data.schedule.forEach((item) => {
            const dayCode = item.day;
            const selector = `.schedule-cell[data-day="${dayCode}"][data-shift="${item.shiftId}"]`;
            const cell = document.querySelector(selector);

            if (cell) {
                item.employees.forEach(emp => {
                    addEmployeeToCell(cell, {
                        id: emp.id,
                        name: emp.name,
                        scheduleId: emp.scheduleId
                    });
                    totalEmployees++;
                });
                console.log(`   ✓ ${dayCode.toUpperCase()}-Ca${item.shiftId}: ${item.employees.length} employees`);
            } else {
                console.warn(`   ⚠️ Cell not found: ${dayCode}-Ca${item.shiftId}`);
            }
        });

        updateStats();
        console.log(`✅ Loaded ${totalEmployees} employee assignments`);

    } catch (error) {
        console.error("❌ Error loading schedule:", error);
        showNotification('Không thể tải lịch làm việc!', 'error');
    }
}

// ========================================
// EMPLOYEE MANAGEMENT
// ========================================

function renderEmployeeList() {
    const list = document.getElementById('employeesList');
    console.log('📋 renderEmployeeList called');
    console.log('  - List element:', list);
    console.log('  - Employees count:', employees.length);

    if (!list) {
        console.error('❌ Element #employeesList not found');
        return;
    }

    if (employees.length === 0) {
        console.warn('⚠️ No employees to render');
        list.innerHTML = '<div style="padding: 20px; text-align: center; color: #9ca3af;">Không có nhân viên</div>';
        return;
    }

    const html = employees.map(emp => `
        <div class="employee-card" draggable="true" data-id="${emp.id}" data-name="${emp.name}" data-role="${emp.role || ''}">
            <div class="employee-avatar">
                ${emp.name.charAt(0)}
            </div>
            <div class="employee-info">
                <div class="employee-name">${emp.name}</div>
                <div class="employee-role">${emp.role || 'Nhân viên'}</div>
            </div>
            <i class="fas fa-grip-vertical employee-drag-icon"></i>
        </div>
    `).join('');

    console.log('  - Generated HTML length:', html.length);
    list.innerHTML = html;

    const renderedCards = list.querySelectorAll('.employee-card');
    console.log(`✅ Rendered ${renderedCards.length} employee cards`);

    // Force visibility
    list.style.display = 'flex';
    list.style.visibility = 'visible';

    // Re-initialize drag after rendering
    setupEmployeeCardsDrag();
}

function setupSearchFilter() {
    const searchInput = document.getElementById('searchEmployee');
    if (!searchInput) {
        console.warn('Element #searchEmployee not found');
        return;
    }

    searchInput.addEventListener('input', function (e) {
        const searchTerm = e.target.value.toLowerCase();
        const cards = document.querySelectorAll('.employee-card');

        cards.forEach(card => {
            const nameEl = card.querySelector('.employee-name');
            const roleEl = card.querySelector('.employee-role');

            if (!nameEl || !roleEl) return;

            const name = nameEl.textContent.toLowerCase();
            const role = roleEl.textContent.toLowerCase();

            if (name.includes(searchTerm) || role.includes(searchTerm)) {
                card.style.display = 'flex';
            } else {
                card.style.display = 'none';
            }
        });
    });
}

// ========================================
// DRAG AND DROP
// ========================================

function initDragAndDrop() {
    setupEmployeeCardsDrag();
    setupScheduleCellsDrop();
}

function setupEmployeeCardsDrag() {
    const employeeCards = document.querySelectorAll('.employee-card');

    employeeCards.forEach(card => {
        card.addEventListener('dragstart', function (e) {
            draggedEmployee = {
                id: parseInt(this.dataset.id),
                name: this.dataset.name,
                role: this.dataset.role || ''
            };
            dragSource = 'sidebar';
            this.classList.add('dragging');
            e.dataTransfer.effectAllowed = 'copy';
            e.dataTransfer.setData('text/html', this.innerHTML);
        });

        card.addEventListener('dragend', function (e) {
            this.classList.remove('dragging');
        });
    });
}

function setupScheduleCellsDrop() {
    const scheduleCells = document.querySelectorAll('.schedule-cell');

    scheduleCells.forEach(cell => {
        cell.addEventListener('dragover', handleDragOver);
        cell.addEventListener('dragleave', handleDragLeave);
        cell.addEventListener('drop', handleDrop);
    });
}

function setupAssignedEmployeeDrag(element) {
    element.addEventListener('dragstart', function (e) {
        const parentCell = this.closest('.schedule-cell');
        const nameEl = this.querySelector('.assigned-employee-info span');

        draggedEmployee = {
            id: parseInt(this.dataset.id),
            name: nameEl ? nameEl.textContent : '',
            element: this
        };
        dragSource = 'cell';
        this.classList.add('dragging');
        e.dataTransfer.effectAllowed = 'move';
    });

    element.addEventListener('dragend', function (e) {
        this.classList.remove('dragging');
        dragSource = null;
    });
}

function handleDragOver(e) {
    if (e.preventDefault) {
        e.preventDefault();
    }
    this.classList.add('drag-over');
    e.dataTransfer.dropEffect = dragSource === 'sidebar' ? 'copy' : 'move';
    return false;
}

function handleDragLeave(e) {
    if (e.target === this) {
        this.classList.remove('drag-over');
    }
}

function handleDrop(e) {
    if (e.stopPropagation) {
        e.stopPropagation();
    }
    e.preventDefault();

    this.classList.remove('drag-over');

    if (!draggedEmployee) return false;

    const day = this.dataset.day;
    const shiftId = parseInt(this.dataset.shift);

    // Check if already assigned in this cell
    const existingAssignments = this.querySelectorAll('.assigned-employee');
    const alreadyAssigned = Array.from(existingAssignments).some(
        el => parseInt(el.dataset.id) === draggedEmployee.id
    );

    if (alreadyAssigned) {
        showNotification('Nhân viên đã được phân vào ca này!', 'warning');
        return false;
    }

    // If dragging from another cell, remove from old cell
    if (dragSource === 'cell' && draggedEmployee.element) {
        const oldCell = draggedEmployee.element.closest('.schedule-cell');
        draggedEmployee.element.remove();
        if (oldCell) {
            updateCellCount(oldCell);
        }
        showNotification('Đã di chuyển nhân viên sang ca mới', 'success');
    }

    // Add to new cell
    addEmployeeToCell(this, {
        id: draggedEmployee.id,
        name: draggedEmployee.name,
        role: draggedEmployee.role || ''
    });

    updateStats();
    draggedEmployee = null;
    dragSource = null;

    return false;
}

// ========================================
// CELL MANAGEMENT
// ========================================

function addEmployeeToCell(cell, employee) {
    if (!cell) {
        console.error('Cannot add employee to null cell');
        return;
    }

    const assignedDiv = document.createElement('div');
    assignedDiv.className = 'assigned-employee';
    assignedDiv.dataset.id = employee.id;
    assignedDiv.draggable = true;

    // ✅ Lưu scheduleId nếu có (để xóa từ DB sau này)
    if (employee.scheduleId) {
        assignedDiv.dataset.scheduleId = employee.scheduleId;
    }

    assignedDiv.innerHTML = `
        <div class="assigned-employee-info">
            <div class="assigned-avatar">${employee.name ? employee.name.charAt(0) : '?'}</div>
            <span title="${employee.name || ''}">${employee.name || 'Unknown'}</span>
        </div>
        <button class="remove-btn" onclick="removeEmployee(this)" title="Xóa khỏi ca">
            <i class="fas fa-times"></i>
        </button>
    `;

    setupAssignedEmployeeDrag(assignedDiv);

    cell.appendChild(assignedDiv);
    updateCellCount(cell);

    // Trigger animation
    setTimeout(() => {
        assignedDiv.style.opacity = '1';
        assignedDiv.style.transform = 'translateY(0)';
    }, 10);
}

function removeEmployee(btn) {
    const assignedDiv = btn.closest('.assigned-employee');
    if (!assignedDiv) return;

    const cell = assignedDiv.closest('.schedule-cell');

    assignedDiv.style.opacity = '0';
    assignedDiv.style.transform = 'translateY(-10px)';

    setTimeout(() => {
        assignedDiv.remove();
        if (cell) {
            updateCellCount(cell);
        }
        updateStats();
        showNotification('Đã xóa nhân viên khỏi ca', 'success');
    }, 200);
}

function updateCellCount(cell) {
    if (!cell) return;

    const count = cell.querySelectorAll('.assigned-employee').length;
    let countBadge = cell.querySelector('.cell-count');

    if (count > 0) {
        if (!countBadge) {
            countBadge = document.createElement('div');
            countBadge.className = 'cell-count';
            cell.appendChild(countBadge);
        }
        countBadge.textContent = count;
    } else {
        if (countBadge) {
            countBadge.remove();
        }
    }
}

// ========================================
// STATISTICS
// ========================================

function updateStats() {
    const assignedIds = new Set();
    document.querySelectorAll('.assigned-employee').forEach(el => {
        assignedIds.add(el.dataset.id);
    });

    const assignedCountEl = document.getElementById('assignedCount');
    const totalEmployeesEl = document.getElementById('totalEmployees');

    if (assignedCountEl) {
        assignedCountEl.textContent = assignedIds.size;
    }

    if (totalEmployeesEl) {
        totalEmployeesEl.textContent = employees.length;
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

    const formatDate = (date) => {
        const day = String(date.getDate()).padStart(2, '0');
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const year = date.getFullYear();
        return `${day}/${month}/${year}`;
    };

    const weekNumber = getWeekNumber(weekStart);
    const weekInfoEl = document.getElementById('weekInfo');

    if (weekInfoEl) {
        weekInfoEl.textContent = `Tuần ${weekNumber}: ${formatDate(weekStart)} - ${formatDate(weekEnd)}`;
    }
}

function updateDayHeaders() {
    const weekStart = getWeekStart(currentWeek);
    const dayHeaders = document.querySelectorAll('.day-header');
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    dayHeaders.forEach((header, index) => {
        const currentDay = new Date(weekStart);
        currentDay.setDate(currentDay.getDate() + index);

        const dateDiv = header.querySelector('.day-date');
        if (dateDiv) {
            const day = String(currentDay.getDate()).padStart(2, '0');
            const month = String(currentDay.getMonth() + 1).padStart(2, '0');
            dateDiv.textContent = `${day}/${month}`;
        }

        header.classList.remove('today');
        if (currentDay.getTime() === today.getTime()) {
            header.classList.add('today');
        }
    });
}

function getWeekNumber(date) {
    const d = new Date(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate()));
    const dayNum = d.getUTCDay() || 7;
    d.setUTCDate(d.getUTCDate() + 4 - dayNum);
    const yearStart = new Date(Date.UTC(d.getUTCFullYear(), 0, 1));
    return Math.ceil((((d - yearStart) / 86400000) + 1) / 7);
}

// ========================================
// SCHEDULE ACTIONS
// ========================================

async function clearSchedule() {
    if (!confirm('Bạn có chắc muốn xóa toàn bộ lịch làm việc trong tuần này?')) {
        return;
    }

    try {
        const weekStart = getWeekStart(currentWeek);
        const response = await fetch('/LichLamViec/ClearWeekSchedule', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ weekStart: weekStart.toISOString() })
        });

        const data = await response.json();

        if (data.success) {
            clearScheduleImmediate();
            showNotification(data.message, 'success');
        } else {
            showNotification('Lỗi: ' + data.message, 'error');
        }
    } catch (error) {
        console.error('Error clearing schedule:', error);
        showNotification('Lỗi kết nối server!', 'error');
    }
}

function clearScheduleImmediate() {
    document.querySelectorAll('.assigned-employee').forEach(el => el.remove());
    document.querySelectorAll('.cell-count').forEach(el => el.remove());
    updateStats();
}


// Thuật toán cân bằng
async function balancedAssign(config) {
    // Implementation tương tự autoAssign() ở trên
}


// Thuật toán tuần tự
async function sequentialAssign(config) {
    const cells = document.querySelectorAll('.schedule-cell');
    let empIndex = 0;

    cells.forEach(cell => {
        for (let i = 0; i < config.minPerShift && empIndex < employees.length; i++) {
            addEmployeeToCell(cell, employees[empIndex]);
            empIndex++;
            if (empIndex >= employees.length) empIndex = 0;
        }
    });
}

// ========================================
// EXPORT
// ========================================
//window.autoAssign = autoAssign;
//window.smartAutoAssign = smartAutoAssign;
async function saveSchedule() {
    const schedule = [];
    const cells = document.querySelectorAll('.schedule-cell');

    cells.forEach(cell => {
        const day = cell.dataset.day;
        const shiftId = parseInt(cell.dataset.shift);
        const assignedEmployees = Array.from(cell.querySelectorAll('.assigned-employee'))
            .map(el => {
                const nameEl = el.querySelector('.assigned-employee-info span');
                return {
                    id: parseInt(el.dataset.id),
                    name: nameEl ? nameEl.textContent : ''
                };
            });

        schedule.push({
            day: day,
            shiftId: shiftId,
            employees: assignedEmployees
        });
    });

    try {
        const weekStart = getWeekStart(currentWeek);

        // ✅ Thêm logging chi tiết
        console.log('📅 Week calculation:');
        console.log('  currentWeek:', currentWeek);
        console.log('  weekStart:', weekStart);
        console.log('  weekStart ISO:', weekStart.toISOString());
        console.log('  weekStart day of week:', weekStart.getDay(), '(0=Sun, 1=Mon)');

        // Kiểm tra các ô có nhân viên
        const cellsWithEmployees = schedule.filter(s => s.employees.length > 0);
        console.log('📦 Cells with employees:');
        cellsWithEmployees.forEach(cell => {
            console.log(`  ${cell.day.toUpperCase()}-Ca${cell.shiftId}: ${cell.employees.map(e => e.name).join(', ')}`);
        });

        const scheduleData = {
            weekStart: weekStart.toISOString(),
            weekNumber: getWeekNumber(weekStart),
            schedule: schedule
        };

        console.log('💾 Sending to server:', scheduleData);

        const response = await fetch('/LichLamViec/SaveSchedule', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(scheduleData)
        });

        const data = await response.json();
        console.log('📬 Server response:', data);

        if (data.success) {
            showNotification(data.message, 'success');
            console.log('🔄 Reloading schedule...');
            await loadScheduleFromServer();
        } else {
            showNotification('Lỗi: ' + data.message, 'error');
        }
    } catch (error) {
        console.error('❌ Error:', error);
        showNotification('Lỗi kết nối server!', 'error');
    }
}
// ========================================
// HELPER FUNCTIONS
// ========================================

function getWeekStart(date) {
    const d = new Date(date);
    const day = d.getDay(); // 0 = Sunday, 1 = Monday, ...

    // Tính số ngày cần trừ để về Thứ Hai
    const diff = d.getDate() - day + (day === 0 ? -6 : 1); // Nếu Chủ Nhật thì -6, không thì +1

    const weekStart = new Date(d.setDate(diff));
    weekStart.setHours(0, 0, 0, 0);

    console.log('🔍 getWeekStart calculation:');
    console.log('  Input date:', date);
    console.log('  Day of week:', day);
    console.log('  Diff:', diff);
    console.log('  Week start (Monday):', weekStart);
    console.log('  Week start day:', weekStart.getDay());

    return weekStart;
}

function showNotification(message, type = 'info') {
    const existing = document.querySelector('.toast-notification');
    if (existing) {
        existing.remove();
    }

    const notification = document.createElement('div');
    notification.className = `toast-notification toast-${type}`;

    const icon = {
        'success': 'fa-check-circle',
        'error': 'fa-times-circle',
        'warning': 'fa-exclamation-triangle',
        'info': 'fa-info-circle'
    }[type] || 'fa-info-circle';

    notification.innerHTML = `
        <i class="fas ${icon}"></i>
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
// EXPORT FUNCTIONS FOR GLOBAL ACCESS
// ========================================

window.previousWeek = previousWeek;
window.nextWeek = nextWeek;
window.clearSchedule = clearSchedule;
//window.autoAssign = autoAssign;
window.saveSchedule = saveSchedule;
window.removeEmployee = removeEmployee;

// ✅ Note: Initialization is now handled in Index.cshtml
// No automatic initialization here to prevent double-loading
console.log('✅ lichlamviec.js loaded successfully. Waiting for manual init() call...');