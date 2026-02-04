// Quản lý ca làm việc - Script chính
console.log('[calamviec.js] Đang tải...');

// Trạng thái global
let shifts = [];
let currentShift = null;
let deleteShiftId = null;

// Modal instances
let shiftModal = null;
let deleteModal = null;

// Flag khởi tạo
let isInitialized = false;

// Khởi tạo an toàn với nhiều fallback
function initCaLamViecPage() {
    if (isInitialized) {
        console.log('[INIT] Đã khởi tạo rồi, bỏ qua...');
        return;
    }

    console.log('[INIT] Đang khởi tạo quản lý ca làm việc...');

    // Kiểm tra Bootstrap đã load chưa
    if (typeof bootstrap === 'undefined') {
        console.error('Bootstrap chưa load! Thử lại sau 100ms...');
        setTimeout(initCaLamViecPage, 100);
        return;
    }

    // Kiểm tra các element cần thiết
    const shiftsTableBody = document.getElementById('shiftsTableBody');
    if (!shiftsTableBody) {
        console.error('Không tìm thấy DOM elements! Thử lại sau 100ms...');
        setTimeout(initCaLamViecPage, 100);
        return;
    }

    isInitialized = true;

    // Khởi tạo Bootstrap modals
    initModals();

    // Load dữ liệu ngay lập tức
    loadShifts();

    // Setup event listeners
    setupEventListeners();

    console.log('[INIT] Khởi tạo quản lý ca làm việc thành công!');
}

// Thử nhiều chiến lược khởi tạo
if (document.readyState === 'loading') {
    // DOM đang loading
    console.log('[INIT] DOM đang loading, chờ DOMContentLoaded...');
    document.addEventListener('DOMContentLoaded', initCaLamViecPage);
} else {
    // DOM đã loaded
    console.log('[INIT] DOM đã loaded, khởi tạo ngay...');
    // Delay nhỏ để đảm bảo tất cả script đã load
    setTimeout(initCaLamViecPage, 50);
}

// Fallback bổ sung: thử lại khi window.load
window.addEventListener('load', function () {
    if (!isInitialized) {
        console.log('[INIT] Thử khởi tạo lại trên window.load...');
        initCaLamViecPage();
    }
});

function initModals() {
    const shiftModalEl = document.getElementById('shiftModal');
    const deleteModalEl = document.getElementById('deleteModal');

    if (shiftModalEl) {
        try {
            shiftModal = new bootstrap.Modal(shiftModalEl);

            // Reset form khi đóng modal
            shiftModalEl.addEventListener('hidden.bs.modal', function () {
                resetForm();
            });
            console.log('[INIT] Khởi tạo shift modal thành công');
        } catch (error) {
            console.error('[INIT] Lỗi khởi tạo shift modal:', error);
        }
    }

    if (deleteModalEl) {
        try {
            deleteModal = new bootstrap.Modal(deleteModalEl);
            console.log('[INIT] Khởi tạo delete modal thành công');
        } catch (error) {
            console.error('[INIT] Lỗi khởi tạo delete modal:', error);
        }
    }
}

function setupEventListeners() {
    // Search input
    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        searchInput.addEventListener('input', debounce(filterShifts, 300));
        console.log('[INIT] Đã gắn search listener');
    }

    // Validate các trường form
    const tenCaInput = document.getElementById('TenCa');
    const gioBatDauInput = document.getElementById('GioBatDau');
    const gioKetThucInput = document.getElementById('GioKetThuc');
    const moTaInput = document.getElementById('MoTa');

    if (tenCaInput) {
        tenCaInput.addEventListener('input', () => {
            if (window.CaLamViecValidatorClient) {
                window.CaLamViecValidatorClient.validateTenCa(tenCaInput.value, tenCaInput);
            }
        });
        tenCaInput.addEventListener('blur', () => {
            if (window.CaLamViecValidatorClient) {
                window.CaLamViecValidatorClient.validateTenCa(tenCaInput.value, tenCaInput);
            }
        });
    }

    if (gioBatDauInput) {
        gioBatDauInput.addEventListener('input', validateTimes);
        gioBatDauInput.addEventListener('change', validateTimes);
    }

    if (gioKetThucInput) {
        gioKetThucInput.addEventListener('input', validateTimes);
        gioKetThucInput.addEventListener('change', validateTimes);
    }

    if (moTaInput) {
        moTaInput.addEventListener('input', () => {
            updateMoTaCount();
            if (window.CaLamViecValidatorClient) {
                window.CaLamViecValidatorClient.validateMoTa(moTaInput.value, moTaInput);
            }
        });
    }
}

function validateTimes() {
    const gioBatDauInput = document.getElementById('GioBatDau');
    const gioKetThucInput = document.getElementById('GioKetThuc');
    const durationHint = document.getElementById('durationHint');

    if (!gioBatDauInput || !gioKetThucInput || !durationHint) return;

    const gioBatDau = gioBatDauInput.value;
    const gioKetThuc = gioKetThucInput.value;

    if (!gioBatDau || !gioKetThuc) {
        durationHint.innerHTML = '<i class="fas fa-info-circle"></i> Thời lượng ca: --';
        return;
    }

    if (window.CaLamViecValidatorClient) {
        // Validate giờ bắt đầu
        window.CaLamViecValidatorClient.validateGio(gioBatDau, gioBatDauInput, 'Giờ bắt đầu');

        // Validate giờ kết thúc
        window.CaLamViecValidatorClient.validateGio(gioKetThuc, gioKetThucInput, 'Giờ kết thúc');

        // Validate logic và tính thời lượng
        const result = window.CaLamViecValidatorClient.validateTimeLogic(gioBatDau, gioKetThuc, gioBatDauInput, gioKetThucInput);

        if (result.isValid && result.duration) {
            durationHint.innerHTML = `<i class="fas fa-check-circle text-success"></i> Thời lượng ca: ${result.duration} giờ`;
        } else if (!result.isValid) {
            durationHint.innerHTML = `<i class="fas fa-exclamation-triangle text-warning"></i> ${result.errorMessage || 'Kiểm tra lại giờ'}`;
        }
    }
}

function updateMoTaCount() {
    const moTaInput = document.getElementById('MoTa');
    const moTaCount = document.getElementById('moTaCount');

    if (moTaInput && moTaCount) {
        moTaCount.textContent = moTaInput.value.length;
    }
}

// Load danh sách ca làm việc
async function loadShifts() {
    try {
        console.log('[API] Đang tải danh sách ca làm việc...');

        const response = await fetch('/CaLamViec/GetAll', {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            },
            cache: 'no-cache'
        });

        console.log('[API] Response status:', response.status);

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        console.log('[API] Response data:', data);

        if (data.success) {
            shifts = data.data || [];
            console.log(`[API] Đã tải ${shifts.length} ca làm việc`);

            if (shifts.length === 0) {
                console.log('[API] Không tìm thấy ca làm việc nào trong database');
            }

            renderShiftsTable();
            updateStatistics();
        } else {
            console.error('[API] Tải ca làm việc thất bại:', data.message);
            showNotification(data.message || 'Không thể tải dữ liệu ca làm việc!', 'error');
            renderEmptyState();
        }
    } catch (error) {
        console.error('[API] Lỗi khi tải ca làm việc:', error);
        console.error('Chi tiết lỗi:', error.message);
        showNotification(`Lỗi kết nối máy chủ: ${error.message}`, 'error');
        renderEmptyState();
    }
}

function renderShiftsTable() {
    const tbody = document.getElementById('shiftsTableBody');
    if (!tbody) {
        console.error('Không tìm thấy element shiftsTableBody!');
        return;
    }

    if (shifts.length === 0) {
        renderEmptyState();
        return;
    }

    tbody.innerHTML = shifts.map(shift => {
        const duration = calculateDuration(shift.gioBatDau, shift.gioKetThuc);
        const statusClass = shift.trangThai ? 'success' : 'warning';
        const statusText = shift.trangThai ? 'Đang hoạt động' : 'Đã vô hiệu hóa';
        const statusIcon = shift.trangThai ? 'check-circle' : 'pause-circle';

        return `
            <tr>
                <td class="text-center">#${shift.maCa}</td>
                <td><strong>${escapeHtml(shift.tenCa)}</strong></td>
                <td>
                    <span class="time-badge">
                        <i class="fas fa-clock"></i>
                        ${shift.gioBatDau}
                    </span>
                </td>
                <td>
                    <span class="time-badge">
                        <i class="fas fa-clock"></i>
                        ${shift.gioKetThuc}
                    </span>
                </td>
                <td>
                    <span class="duration-badge">
                        <i class="fas fa-hourglass-half"></i>
                        ${duration}h
                    </span>
                </td>
                <td>${escapeHtml(shift.moTa || '--')}</td>
                <td>
                    <span class="badge badge-${statusClass}">
                        <i class="fas fa-${statusIcon}"></i>
                        ${statusText}
                    </span>
                </td>
                <td>
                    <div class="d-flex gap-2">
                        <button class="btn btn-sm btn-primary" onclick="editShift(${shift.maCa})" title="Sửa">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button class="btn btn-sm btn-${shift.trangThai ? 'warning' : 'success'}" 
                                onclick="toggleStatus(${shift.maCa})" 
                                title="${shift.trangThai ? 'Vô hiệu hóa' : 'Kích hoạt'}">
                            <i class="fas fa-power-off"></i>
                        </button>
                        <button class="btn btn-sm btn-danger" onclick="deleteShift(${shift.maCa})" title="Xóa">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </td>
            </tr>
        `;
    }).join('');

    console.log(`[RENDER] Đã render ${shifts.length} ca làm việc vào bảng`);
}

function renderEmptyState() {
    const tbody = document.getElementById('shiftsTableBody');
    if (!tbody) return;

    tbody.innerHTML = `
        <tr>
            <td colspan="8" class="text-center py-5">
                <i class="fas fa-inbox" style="font-size: 48px; opacity: 0.3;"></i>
                <p class="mt-3 text-muted">Chưa có ca làm việc nào</p>
                <button class="btn btn-primary mt-2" onclick="openAddModal()">
                    <i class="fas fa-plus"></i> Thêm ca đầu tiên
                </button>
            </td>
        </tr>
    `;
}

function updateStatistics() {
    const totalShifts = shifts.length;
    const activeShifts = shifts.filter(s => s.trangThai).length;
    const inactiveShifts = shifts.filter(s => !s.trangThai).length;

    // Tính tổng giờ của các ca đang hoạt động
    const totalHours = shifts
        .filter(s => s.trangThai)
        .reduce((sum, shift) => {
            return sum + calculateDuration(shift.gioBatDau, shift.gioKetThuc);
        }, 0);

    // Cập nhật UI
    const totalShiftsEl = document.getElementById('totalShifts');
    const activeShiftsEl = document.getElementById('activeShifts');
    const inactiveShiftsEl = document.getElementById('inactiveShifts');
    const totalHoursEl = document.getElementById('totalHours');

    if (totalShiftsEl) totalShiftsEl.textContent = totalShifts;
    if (activeShiftsEl) activeShiftsEl.textContent = activeShifts;
    if (inactiveShiftsEl) inactiveShiftsEl.textContent = inactiveShifts;
    if (totalHoursEl) totalHoursEl.textContent = totalHours.toFixed(1) + 'h';
}

// Thao tác với modal
function openAddModal() {
    currentShift = null;
    resetForm();

    // Cập nhật tiêu đề modal
    const modalTitle = document.getElementById('modalTitle');
    if (modalTitle) {
        modalTitle.innerHTML = '<i class="fas fa-plus-circle"></i> Thêm ca làm việc mới';
    }

    // Hiển thị modal
    if (shiftModal) {
        shiftModal.show();
    }
}

function editShift(maCa) {
    const shift = shifts.find(s => s.maCa === maCa);
    if (!shift) {
        showNotification('Không tìm thấy ca làm việc!', 'error');
        return;
    }

    currentShift = shift;

    // Cập nhật tiêu đề modal
    const modalTitle = document.getElementById('modalTitle');
    if (modalTitle) {
        modalTitle.innerHTML = '<i class="fas fa-edit"></i> Chỉnh sửa ca làm việc';
    }

    // Điền dữ liệu vào form
    document.getElementById('MaCa').value = shift.maCa;
    document.getElementById('TenCa').value = shift.tenCa;
    document.getElementById('GioBatDau').value = shift.gioBatDau;
    document.getElementById('GioKetThuc').value = shift.gioKetThuc;
    document.getElementById('MoTa').value = shift.moTa || '';

    // Cập nhật số ký tự
    updateMoTaCount();

    // Validate và hiển thị thời lượng
    validateTimes();

    // Hiển thị modal
    if (shiftModal) {
        shiftModal.show();
    }
}

function resetForm() {
    const form = document.getElementById('shiftForm');
    if (form) {
        form.reset();
    }

    // Reset hidden fields
    const maCaField = document.getElementById('MaCa');
    if (maCaField) {
        maCaField.value = '0';
    }

    // Xóa validation
    clearValidation();

    // Reset duration hint
    const durationHint = document.getElementById('durationHint');
    if (durationHint) {
        durationHint.innerHTML = '<i class="fas fa-info-circle"></i> Thời lượng ca: --';
    }

    // Reset số ký tự
    updateMoTaCount();

    currentShift = null;
}

// Các thao tác CRUD
async function saveShift() {
    try {
        // Validate form
        if (!window.CaLamViecValidatorClient || !window.CaLamViecValidatorClient.validateForm()) {
            showNotification('Vui lòng kiểm tra lại thông tin!', 'warning');
            return;
        }

        // Lấy dữ liệu form
        const maCa = parseInt(document.getElementById('MaCa').value) || null;
        const tenCa = document.getElementById('TenCa').value.trim();
        const gioBatDau = document.getElementById('GioBatDau').value;
        const gioKetThuc = document.getElementById('GioKetThuc').value;
        const moTa = document.getElementById('MoTa').value.trim();

        // Lấy CSRF token
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value
            || document.querySelector('input[type="hidden"][name="__RequestVerificationToken"]')?.value;

        if (!token) {
            console.error('Không tìm thấy CSRF token!');
            showNotification('Lỗi bảo mật! Vui lòng tải lại trang.', 'error');
            return;
        }

        // Chuẩn bị request
        const isEdit = maCa && maCa > 0;
        const url = isEdit ? '/CaLamViec/Update' : '/CaLamViec/Create';
        const method = isEdit ? 'PUT' : 'POST';

        const requestData = {
            MaCa: isEdit ? maCa : null,
            TenCa: tenCa,
            GioBatDau: gioBatDau,
            GioKetThuc: gioKetThuc,
            MoTa: moTa || null
        };

        console.log(`[API] ${isEdit ? 'Đang cập nhật' : 'Đang tạo'} ca làm việc:`, requestData);

        // Gửi request
        const response = await fetch(url, {
            method: method,
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(requestData)
        });

        const data = await response.json();
        console.log('[API] Response:', data);

        if (data.success) {
            showNotification(data.message, 'success');

            // Đóng modal
            if (shiftModal) {
                shiftModal.hide();
            }

            // Tải lại dữ liệu
            await loadShifts();
        } else {
            showNotification(data.message, 'error');
        }
    } catch (error) {
        console.error('[API] Lỗi khi lưu ca làm việc:', error);
        showNotification('Lỗi kết nối máy chủ!', 'error');
    }
}

async function toggleStatus(maCa) {
    try {
        const shift = shifts.find(s => s.maCa === maCa);
        if (!shift) {
            showNotification('Không tìm thấy ca làm việc!', 'error');
            return;
        }

        // Xác nhận hành động
        const action = shift.trangThai ? 'vô hiệu hóa' : 'kích hoạt';
        const confirmMsg = `Bạn có chắc chắn muốn ${action} ca "${shift.tenCa}"?`;

        if (!confirm(confirmMsg)) {
            return;
        }

        console.log(`[API] Đang chuyển trạng thái ca ${maCa}...`);

        // Lấy CSRF token
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value
            || document.querySelector('input[type="hidden"][name="__RequestVerificationToken"]')?.value;

        const response = await fetch('/CaLamViec/ToggleStatus', {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token || ''
            },
            body: JSON.stringify(maCa)
        });

        const data = await response.json();

        if (data.success) {
            console.log('[API] Chuyển trạng thái thành công');
            showNotification(data.message, 'success');
            await loadShifts();
        } else {
            console.error('[API] Chuyển trạng thái thất bại:', data.message);
            showNotification(data.message, 'error');
        }
    } catch (error) {
        console.error('[API] Lỗi khi chuyển trạng thái:', error);
        showNotification('Lỗi kết nối máy chủ!', 'error');
    }
}

function deleteShift(maCa) {
    const shift = shifts.find(s => s.maCa === maCa);
    if (!shift) {
        showNotification('Không tìm thấy ca làm việc!', 'error');
        return;
    }

    deleteShiftId = maCa;

    // Set tên ca trong modal
    const deleteShiftName = document.getElementById('deleteShiftName');
    if (deleteShiftName) {
        deleteShiftName.textContent = shift.tenCa;
    }

    // Hiển thị modal
    if (deleteModal) {
        deleteModal.show();
    }
}

async function confirmDelete() {
    if (!deleteShiftId) return;

    try {
        console.log(`[API] Đang xóa ca ${deleteShiftId}...`);

        // Lấy CSRF token
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value
            || document.querySelector('input[type="hidden"][name="__RequestVerificationToken"]')?.value;

        const response = await fetch('/CaLamViec/Delete', {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token || ''
            },
            body: JSON.stringify(deleteShiftId)
        });

        const data = await response.json();

        if (data.success) {
            console.log('[API] Xóa ca làm việc thành công');
            showNotification(data.message, 'success');

            // Đóng modal
            if (deleteModal) {
                deleteModal.hide();
            }

            deleteShiftId = null;
            await loadShifts();
        } else {
            console.error('[API] Xóa ca làm việc thất bại:', data.message);
            showNotification(data.message, 'error');
        }
    } catch (error) {
        console.error('[API] Lỗi khi xóa ca làm việc:', error);
        showNotification('Lỗi kết nối máy chủ!', 'error');
    }
}

// Lọc và tìm kiếm
function filterShifts() {
    const searchInput = document.getElementById('searchInput');
    if (!searchInput) return;

    const searchTerm = searchInput.value.toLowerCase().trim();

    if (!searchTerm) {
        renderShiftsTable();
        return;
    }

    const filtered = shifts.filter(shift => {
        return shift.tenCa.toLowerCase().includes(searchTerm) ||
            shift.moTa?.toLowerCase().includes(searchTerm) ||
            shift.gioBatDau.includes(searchTerm) ||
            shift.gioKetThuc.includes(searchTerm);
    });

    // Render kết quả lọc
    const tbody = document.getElementById('shiftsTableBody');
    if (!tbody) return;

    if (filtered.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="8" class="text-center py-5">
                    <i class="fas fa-search" style="font-size: 48px; opacity: 0.3;"></i>
                    <p class="mt-3 text-muted">Không tìm thấy kết quả phù hợp với "${escapeHtml(searchTerm)}"</p>
                </td>
            </tr>
        `;
        return;
    }

    // Tạm thời set shifts = filtered để render
    const originalShifts = [...shifts];
    shifts = filtered;
    renderShiftsTable();
    shifts = originalShifts;
}

function refreshData() {
    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        searchInput.value = '';
    }

    showNotification('Đang làm mới dữ liệu...', 'info');
    loadShifts();
}

// Hàm tiện ích
function clearValidation() {
    const inputs = document.querySelectorAll('.form-control');
    inputs.forEach(input => {
        input.classList.remove('is-valid', 'is-invalid');
    });

    const feedbacks = document.querySelectorAll('.invalid-feedback, .valid-feedback');
    feedbacks.forEach(fb => {
        fb.style.display = 'none';
    });
}

function calculateDuration(gioBatDau, gioKetThuc) {
    try {
        if (!gioBatDau || !gioKetThuc) {
            return 0;
        }

        const [startHour, startMin] = gioBatDau.split(':').map(Number);
        const [endHour, endMin] = gioKetThuc.split(':').map(Number);

        // Validate numbers
        if (isNaN(startHour) || isNaN(startMin) || isNaN(endHour) || isNaN(endMin)) {
            console.warn('Định dạng thời gian không hợp lệ:', gioBatDau, gioKetThuc);
            return 0;
        }

        let startMinutes = startHour * 60 + startMin;
        let endMinutes = endHour * 60 + endMin;

        // Ca qua đêm
        if (endMinutes < startMinutes) {
            endMinutes += 24 * 60;
        }

        const durationMinutes = endMinutes - startMinutes;
        const hours = durationMinutes / 60;

        return parseFloat(hours.toFixed(1));
    } catch (error) {
        console.error('Lỗi tính thời lượng:', error);
        return 0;
    }
}

function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `toast-notification toast-${type}`;

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

// Export các hàm ra window
window.openAddModal = openAddModal;
window.editShift = editShift;
window.saveShift = saveShift;
window.toggleStatus = toggleStatus;
window.deleteShift = deleteShift;
window.confirmDelete = confirmDelete;
window.refreshData = refreshData;

console.log('[calamviec.js] Đã tải thành công!');