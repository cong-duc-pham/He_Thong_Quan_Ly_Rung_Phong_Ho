// ========================================
// LICH LAM VIEC - SIMPLE VERSION
// ========================================

console.log('📦 [lichlamviec.js] Loading...');

// Global state
let employees = [];
let shifts = [];
let currentWeek = new Date();
let draggedEmployee = null;
let dragSource = null;

// ========================================
// MAIN INITIALIZATION
// ========================================

async function init() {
    console.log('🚀 [INIT] Starting...');
    
    try {
        // Load employees from API (for dynamic updates)
        console.log('📡 Loading employees from API...');
        await loadEmployees();
        
        // Load shifts
        console.log('📡 Loading shifts...');
        await loadShifts();
        
        // Setup UI
        console.log('🎨 Setting up UI...');
        updateWeekInfo();
        updateDayHeaders();
        setupSearchFilter();
        
        // Setup drag and drop for pre-rendered employee cards
        initDragAndDrop();
        
        // Load schedule
        console.log('📅 Loading schedule...');
        await loadScheduleFromServer();
        
        console.log('✅ Init complete!');
    } catch (error) {
        console.error('❌ Init error:', error);
        showNotification('Lỗi khởi tạo: ' + error.message, 'error');
    }
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
            // Don't render since View already has them
            // Just update stats
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
        
        updateStats();
    } catch (error) {
        console.error('Error loading schedule:', error);
    }
}

// ========================================
// UI RENDERING
// ========================================

function renderEmployeeList() {
    const list = document.getElementById('employeesList');
    
    if (!list) {
        console.error('Element #employeesList not found');
        return;
    }
    
    if (employees.length === 0) {
        list.innerHTML = '<div style="padding: 20px; text-align: center; color: #9ca3af;">Không có nhân viên</div>';
        return;
    }
    
    list.innerHTML = employees.map(emp => `
        <div class="employee-card" draggable="true" data-id="${emp.id}" data-name="${emp.name}" data-role="${emp.role || ''}">
            <div class="employee-avatar">${emp.name.charAt(0)}</div>
            <div class="employee-info">
                <div class="employee-name">${emp.name}</div>
                <div class="employee-role">${emp.role || 'Nhân viên'}</div>
            </div>
            <i class="fas fa-grip-vertical employee-drag-icon"></i>
        </div>
    `).join('');
    
    console.log(`✅ Rendered ${employees.length} employee cards`);
    setupEmployeeCardsDrag();
}

function updateStats() {
    const assignedIds = new Set();
    document.querySelectorAll('.assigned-employee').forEach(el => {
        assignedIds.add(el.dataset.id);
    });
    
    const totalEl = document.getElementById('totalEmployees');
    const assignedEl = document.getElementById('assignedCount');
    
    // Update total - use employees array or count from DOM
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

// ========================================
// DRAG AND DROP
// ========================================

function initDragAndDrop() {
    console.log('🎯 Setting up drag and drop...');
    setupEmployeeCardsDrag();
    setupScheduleCellsDrop();
    console.log('✅ Drag and drop ready!');
}

function setupEmployeeCardsDrag() {
    const employeeCards = document.querySelectorAll('.employee-card');
    console.log(`📋 Setting up drag for ${employeeCards.length} employee cards`);
    
    employeeCards.forEach(card => {
        card.addEventListener('dragstart', function (e) {
            draggedEmployee = {
                id: parseInt(this.dataset.id),
                name: this.dataset.name,
                role: this.dataset.role || ''
            };
            dragSource = 'sidebar';
            this.classList.add('dragging');
            console.log('🖱️ Dragging employee:', draggedEmployee);
        });
        
        card.addEventListener('dragend', function (e) {
            this.classList.remove('dragging');
        });
    });
}

function setupScheduleCellsDrop() {
    const cells = document.querySelectorAll('.schedule-cell');
    console.log(`📅 Setting up drop zones for ${cells.length} cells`);
    
    cells.forEach(cell => {
        cell.addEventListener('dragover', function (e) {
            e.preventDefault();
            this.classList.add('drag-over');
        });
        
        cell.addEventListener('dragleave', function (e) {
            this.classList.remove('drag-over');
        });
        
        cell.addEventListener('drop', function (e) {
            e.preventDefault();
            this.classList.remove('drag-over');
            
            if (draggedEmployee) {
                const day = this.dataset.day;
                const shift = this.dataset.shift;
                console.log(`📍 Dropped employee ${draggedEmployee.name} to ${day} shift ${shift}`);
                
                // Check if already exists
                const existing = this.querySelector(`[data-id="${draggedEmployee.id}"]`);
                if (!existing) {
                    addEmployeeToCell(this, draggedEmployee);
                    updateStats();
                    console.log('✅ Employee added to cell');
                } else {
                    console.log('⚠️ Employee already in this cell');
                }
            }
        });
    });
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
    
    // Setup drag for assigned employee
    employeeEl.addEventListener('dragstart', function (e) {
        draggedEmployee = {
            id: parseInt(this.dataset.id),
            name: this.querySelector('.assigned-employee-name').textContent,
            scheduleId: this.dataset.scheduleId
        };
        dragSource = 'cell';
        this.classList.add('dragging');
    });
    
    employeeEl.addEventListener('dragend', function (e) {
        this.classList.remove('dragging');
        if (dragSource === 'cell') {
            this.remove();
            updateStats();
        }
    });
    
    cell.appendChild(employeeEl);
}

function removeEmployee(btn) {
    const empEl = btn.closest('.assigned-employee');
    const empName = empEl.querySelector('.assigned-employee-name').textContent;
    
    if (confirm(`Xác nhận xóa "${empName}" khỏi ca làm việc?`)) {
        console.log(`🗑️ Removing employee: ${empName}`);
        empEl.remove();
        updateStats();
        showNotification(`Đã xóa ${empName} khỏi ca`, 'info');
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
        
        // Highlight today
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
        console.log('📦 Save response:', data);
        
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
    
    console.log('🗑️ Clearing schedule for current week');
    
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

window.init = init;
window.previousWeek = previousWeek;
window.nextWeek = nextWeek;
window.clearSchedule = clearSchedule;
window.saveSchedule = saveSchedule;
window.removeEmployee = removeEmployee;

console.log('✅ [lichlamviec.js] Loaded. window.init is ready.');
