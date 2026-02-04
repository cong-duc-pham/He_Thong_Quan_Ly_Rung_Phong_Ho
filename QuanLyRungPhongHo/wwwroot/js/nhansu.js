// Đơn giản hóa - không dùng namespace, chỉ dùng inline handlers
// Load script này trực tiếp khi document ready

(function () {
    'use strict';

    // Mapping chức vụ -> quyền (theo phân quyền hệ thống)
    const CHUC_VU_QUYEN_MAP = {
        'Quản trị viên Tỉnh': 'Admin_Tinh',
        'Quản lý Xã': 'QuanLy_Xa',
        'Kiểm lâm viên': 'Kiem_Lam'
    };

    // Debounce search để tránh gọi API quá nhiều lần
    function debounce(func, wait) {
        let timeout;
        return function (...args) {
            clearTimeout(timeout);
            timeout = setTimeout(() => func(...args), wait);
        };
    }

    // Gọi API tìm kiếm
    function performSearch() {
        const searchInput = document.getElementById('nhansuSearchInput');
        const xaFilter = document.getElementById('nhansuXaFilter');
        const roleFilter = document.getElementById('nhansuRoleFilter');

        if (!searchInput || !xaFilter || !roleFilter) return;

        const searchString = searchInput.value.trim();
        const maXa = xaFilter.value;
        const role = roleFilter.value;

        fetch(`/NhanSu/SearchRealtime?searchString=${encodeURIComponent(searchString)}&maXaFilter=${encodeURIComponent(maXa)}&roleFilter=${encodeURIComponent(role)}`)
            .then(r => r.json())
            .then(data => {
                if (data.success) renderResults(data.items, data.totalRecords, data.message);
            })
            .catch(e => console.error('Lỗi tìm kiếm:', e));
    }

    // Render kết quả tìm kiếm
    function renderResults(items, totalRecords, message) {
        const tableBody = document.getElementById('nhansuTableBody');
        const mobileView = document.getElementById('nhansuMobileView');
        const searchStatus = document.getElementById('nhansuSearchStatus');
        const totalBadge = document.getElementById('nhansuTotal');

        if (!tableBody || !mobileView || !searchStatus) return;

        // Render table desktop
        if (items.length === 0) {
            tableBody.innerHTML = '<tr><td colspan="6" class="text-center text-muted py-3">Không có dữ liệu</td></tr>';
            mobileView.innerHTML = '';
        } else {
            let tableRows = '';
            items.forEach(item => {
                const emailDisplay = item.email ? `<div><small class="text-muted"><i class="bi bi-envelope"></i> ${item.email}</small></div>` : '';
                const statusBadge = item.trangThai
                    ? '<span class="badge bg-success">Hoạt động</span>'
                    : '<span class="badge bg-danger">Bị khóa</span>';
                const lockBtnIcon = item.trangThai ? 'bi-lock' : 'bi-unlock';
                const lockBtnText = item.trangThai ? '' : '';
                const lockBtnClass = item.trangThai ? 'btn-warning' : 'btn-success';

                tableRows += `
                    <tr class="${!item.trangThai ? 'table-secondary' : ''}">
                        <td>
                            <div class="fw-bold">${item.hoTen}</div>
                            ${statusBadge}
                        </td>
                        <td><span class="badge bg-primary">${item.chucVu}</span></td>
                        <td>
                            <div><small class="text-muted"><i class="bi bi-telephone"></i> ${item.sdt}</small></div>
                            ${emailDisplay}
                        </td>
                        <td>${item.tenXa}</td>
                        <td>
                            ${item.tenDangNhap ? `<span>${item.tenDangNhap}</span> <span class="text-muted">(${item.quyen})</span>` : '<span class="text-danger">Chưa có TK</span>'}
                        </td>
                        <td class="text-center">
                            <button class="btn btn-sm btn-outline-primary" onclick="editData(${item.maNV}, event)" title="Sửa thông tin">
                                <i class="bi bi-pencil"></i>
                            </button>
                            <button class="btn btn-sm btn-outline-${lockBtnClass}" onclick="toggleLock(${item.maNV}, ${item.trangThai})" title="${item.trangThai ? 'Khóa tài khoản' : 'Mở khóa tài khoản'}">
                                <i class="bi ${lockBtnIcon}"></i>
                            </button>
                        </td>
                    </tr>`;
            });
            tableBody.innerHTML = tableRows;

            // Render mobile view
            let mobileCards = '';
            items.forEach(item => {
                const emailMobileDisplay = item.email ? `<p class="mb-1 small"><i class="bi bi-envelope"></i> ${item.email}</p>` : '';
                const statusBadge = item.trangThai
                    ? '<span class="badge bg-success">Hoạt động</span>'
                    : '<span class="badge bg-danger">Bị khóa</span>';
                const lockBtnText = item.trangThai ? 'Khóa' : 'Mở khóa';
                const lockBtnClass = item.trangThai ? 'btn-warning' : 'btn-success';

                mobileCards += `
                    <div class="card mb-3 shadow-sm border-start border-4 ${item.trangThai ? 'border-success' : 'border-secondary'}">
                        <div class="card-body">
                            <div class="d-flex justify-content-between mb-2">
                                <h6 class="fw-bold text-success mb-0">${item.hoTen}</h6>
                                <div>
                                    <span class="badge bg-primary">${item.chucVu}</span>
                                    ${statusBadge}
                                </div>
                            </div>
                            <p class="mb-1 small"><i class="bi bi-geo-alt"></i> ${item.tenXa}</p>
                            <p class="mb-1 small"><i class="bi bi-telephone"></i> ${item.sdt}</p>
                            ${emailMobileDisplay}
                            <p class="mb-2 small"><i class="bi bi-person"></i> TK: ${item.tenDangNhap || 'Chưa có'}</p>
                            <div class="d-flex gap-2">
                                <button class="btn btn-outline-primary btn-sm flex-fill" onclick="editData(${item.maNV}, event)">
                                    <i class="bi bi-pencil"></i> Sửa
                                </button>
                                <button class="btn btn-outline-${lockBtnClass} btn-sm flex-fill" onclick="toggleLock(${item.maNV}, ${item.trangThai})">
                                    <i class="bi ${item.trangThai ? 'bi-lock' : 'bi-unlock'}"></i> ${lockBtnText}
                                </button>
                            </div>
                        </div>
                    </div>`;
            });
            mobileView.innerHTML = mobileCards;
        }

        // Cập nhật status message
        searchStatus.textContent = message;
        searchStatus.style.display = 'inline-block';

        // Cập nhật badge
        if (totalBadge) {
            totalBadge.textContent = totalRecords === 0 ? 'Không tìm thấy' :
                totalRecords === 1 ? '1 nhân sự' :
                    `${totalRecords} nhân sự`;
            totalBadge.className = totalRecords === 0 ? 'badge bg-danger text-white' : 'badge bg-info text-dark';
        }
    }

    // Khởi tạo search form
    function init() {
        const searchInput = document.getElementById('nhansuSearchInput');
        const xaFilter = document.getElementById('nhansuXaFilter');
        const roleFilter = document.getElementById('nhansuRoleFilter');

        if (!searchInput || !xaFilter || !roleFilter) return;

        const debouncedSearch = debounce(performSearch, 300);

        // Bind event listeners
        searchInput.addEventListener('input', debouncedSearch);
        xaFilter.addEventListener('change', performSearch);
        roleFilter.addEventListener('change', performSearch);

        // Xử lý Enter key
        [searchInput, xaFilter, roleFilter].forEach(el => {
            el.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    performSearch();
                }
            });
        });

        // Auto-close alert sau 5s
        document.querySelectorAll('.alert').forEach(alert => {
            setTimeout(() => {
                try {
                    new bootstrap.Alert(alert).close();
                } catch (e) { }
            }, 5000);
        });

        // Khởi tạo tooltips
        initTooltips();
    }

    // Khởi tạo Bootstrap tooltips
    function initTooltips() {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    window.NhanSuSearchInit = init;
})();

// Toggle hiển thị mật khẩu
function togglePassword(fieldId) {
    const field = document.getElementById(fieldId);
    const icon = document.getElementById(fieldId + '-icon');

    if (!field || !icon) return;

    if (field.type === 'password') {
        field.type = 'text';
        icon.classList.remove('bi-eye');
        icon.classList.add('bi-eye-slash');
    } else {
        field.type = 'password';
        icon.classList.remove('bi-eye-slash');
        icon.classList.add('bi-eye');
    }
}

// Sửa lại hàm waitForValidator trong nhansu.js
function waitForValidator(callback, maxAttempts = 10) {
    let attempts = 0;

    const checkInterval = setInterval(() => {
        attempts++;
        // Kiểm tra xem object đã tồn tại trong window chưa
        if (window.NhanSuValidatorClient) {
            clearInterval(checkInterval);
            console.log('Validator found after', attempts, 'attempts');
            callback(true);
        } else if (attempts >= maxAttempts) {
            clearInterval(checkInterval);
            console.error(' Cannot find NhanSuValidatorClient in window scope.');
            callback(false);
        }
    }, 50); // Kiểm tra nhanh hơn (50ms)
}

// Mở modal thêm mới
function openModal() {
    const modalElement = document.getElementById('nhanSuModal');
    const form = document.getElementById('frmNhanSu');
    const modalTitle = document.getElementById('modalTitle');

    if (!form || !modalElement) {
        console.error('❌ Không tìm thấy form hoặc modal element!');
        return;
    }

    // Reset form
    form.reset();
    form.classList.remove('was-validated');

    // Đợi validator load xong trước khi clear validation
    waitForValidator((isReady) => {
        if (isReady && window.NhanSuValidatorClient) {
            ['HoTen', 'SDT', 'Email', 'TenDangNhap', 'MatKhau', 'ChucVu', 'MaXa'].forEach(id => {
                const field = document.getElementById(id);
                if (field) {
                    window.NhanSuValidatorClient.clearValidation(field);
                }
            });
        }
    });

    // Set giá trị mặc định
    document.getElementById('MaNV').value = '0';
    document.getElementById('Quyen').value = 'Kiem_Lam';

    // Hiển thị mật khẩu là bắt buộc
    const passRequired = document.getElementById('passRequired');
    const passNote = document.getElementById('passNote');
    if (passRequired) passRequired.style.display = 'inline';
    if (passNote) passNote.style.display = 'none';

    // Đặt mật khẩu là required
    const matKhauField = document.getElementById('MatKhau');
    if (matKhauField) {
        matKhauField.required = true;
        matKhauField.placeholder = 'Tối thiểu 6 ký tự';
    }

    // Xóa alert cũ
    const alerts = modalElement.querySelectorAll('.alert');
    alerts.forEach(alert => alert.remove());

    modalTitle.innerHTML = '<i class="bi bi-person-plus-fill me-2"></i>Thêm mới nhân sự';

    const modal = bootstrap.Modal.getInstance(modalElement) || new bootstrap.Modal(modalElement, { backdrop: 'static', keyboard: false });
    modal.show();

    // ===== KÍCH HOẠT VALIDATION NGAY KHI MỞ FORM =====
    modalElement.addEventListener('shown.bs.modal', function onModalShown() {
        console.log('🎯 Modal đã mở, đang kiểm tra validation...');

        // Đợi validator load xong
        waitForValidator((isReady) => {
            if (isReady) {
                window.NhanSuValidatorClient.bindEvents();
                console.log('✅ Validation đã được kích hoạt!');
            } else {
                console.error('❌ NhanSuValidatorClient không load được!');
                console.log('⚠️ Sử dụng HTML5 validation thay thế');
                
                // Fallback: Sử dụng HTML5 validation
                form.setAttribute('novalidate', '');
                form.addEventListener('submit', function(e) {
                    if (!form.checkValidity()) {
                        e.preventDefault();
                        e.stopPropagation();
                    }
                    form.classList.add('was-validated');
                });
            }
        });

        // Remove listener sau khi đã thực hiện
        modalElement.removeEventListener('shown.bs.modal', onModalShown);
    }, { once: true });
}

// Sửa nhân sự
function editData(id, event) {
    // Lưu reference của button trước để tránh mất trong Promise chain
    let btn = null;
    let originalHTML = null;
    
    if (event) {
        btn = event.currentTarget;
        btn.disabled = true;
        originalHTML = btn.innerHTML;
        btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span>';
    }

    const modalElement = document.getElementById('nhanSuModal');
    const form = document.getElementById('frmNhanSu');
    const modalTitle = document.getElementById('modalTitle');

    if (!form || !modalElement) return;

    fetch(`/NhanSu/GetById?id=${id}`)
        .then(r => r.json())
        .then(data => {
            // Restore button state
            if (btn) {
                btn.disabled = false;
                btn.innerHTML = originalHTML || '<i class="bi bi-pencil"></i>';
            }

            if (!data.success) {
                alert(data.message);
                return;
            }

            // Reset form trước
            form.reset();
            form.classList.remove('was-validated');

            // Đợi validator load xong trước khi clear validation
            waitForValidator((isReady) => {
                if (isReady && window.NhanSuValidatorClient) {
                    ['HoTen', 'SDT', 'Email', 'TenDangNhap', 'MatKhau', 'ChucVu', 'MaXa'].forEach(fieldId => {
                        const field = document.getElementById(fieldId);
                        if (field) {
                            window.NhanSuValidatorClient.clearValidation(field);
                        }
                    });
                }
            });

            // Fill data
            document.getElementById('MaNV').value = data.maNV;
            document.getElementById('HoTen').value = data.hoTen || '';
            document.getElementById('ChucVu').value = data.chucVu || '';
            document.getElementById('SDT').value = data.sdt || '';
            document.getElementById('Email').value = data.email || '';
            document.getElementById('MaXa').value = data.maXa || '';
            document.getElementById('TenDangNhap').value = data.tenDangNhap || '';
            document.getElementById('Quyen').value = data.quyen || 'Kiem_Lam';

            // Mật khẩu không bắt buộc khi sửa
            const matKhauField = document.getElementById('MatKhau');
            const passRequired = document.getElementById('passRequired');
            const passNote = document.getElementById('passNote');

            if (matKhauField) {
                matKhauField.value = '';
                matKhauField.required = false;
                matKhauField.placeholder = 'Để trống nếu không đổi mật khẩu';
            }
            if (passRequired) passRequired.style.display = 'none';
            if (passNote) passNote.style.display = 'inline';

            // Cập nhật hint quyền
            const chucVuSelect = document.getElementById('ChucVu');
            if (chucVuSelect) {
                const event = new Event('change');
                chucVuSelect.dispatchEvent(event);
            }

            // Xóa alert cũ
            const alerts = modalElement.querySelectorAll('.alert');
            alerts.forEach(alert => alert.remove());

            modalTitle.innerHTML = '<i class="bi bi-pencil-square me-2"></i>Chỉnh sửa nhân sự';

            const modal = bootstrap.Modal.getInstance(modalElement) || new bootstrap.Modal(modalElement, { backdrop: 'static', keyboard: false });
            modal.show();

            // ===== KÍCH HOẠT VALIDATION NGAY KHI MỞ FORM =====
            modalElement.addEventListener('shown.bs.modal', function onModalShown() {
                console.log('🎯 Modal sửa đã mở, đang kiểm tra validation...');

                // Đợi validator load xong
                waitForValidator((isReady) => {
                    if (isReady) {
                        window.NhanSuValidatorClient.bindEvents();
                        console.log('✅ Validation đã được kích hoạt!');
                    } else {
                        console.error('❌ NhanSuValidatorClient không load được!');
                        console.log('⚠️ Sử dụng HTML5 validation thay thế');
                        
                        // Fallback: Sử dụng HTML5 validation
                        form.setAttribute('novalidate', '');
                        form.addEventListener('submit', function(e) {
                            if (!form.checkValidity()) {
                                e.preventDefault();
                                e.stopPropagation();
                            }
                            form.classList.add('was-validated');
                        });
                    }
                });

                modalElement.removeEventListener('shown.bs.modal', onModalShown);
            }, { once: true });
        })
        .catch(e => {
            // Restore button state on error
            if (btn) {
                btn.disabled = false;
                btn.innerHTML = originalHTML || '<i class="bi bi-pencil"></i>';
            }
            console.error('Lỗi:', e);
            alert('Lỗi khi tải dữ liệu: ' + e.message);
        });
}

// Lưu nhân sự
function saveData() {
    const form = document.getElementById('frmNhanSu');

    // Đợi validator, sau đó validate
    waitForValidator((isReady) => {
        if (isReady && window.NhanSuValidatorClient) {
            // === VALIDATE VỚI REALTIME VALIDATION ===
            if (!window.NhanSuValidatorClient.validateForm()) {
                // Hiển thị thông báo
                const alertDiv = document.createElement('div');
                alertDiv.className = 'alert alert-danger alert-dismissible fade show mt-3';
                alertDiv.innerHTML = `
                    <i class="bi bi-exclamation-triangle"></i> 
                    Vui lòng kiểm tra lại các trường đánh dấu đỏ!
                    <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
                `;

                const modalBody = form.parentElement;
                const existingAlert = modalBody.querySelector('.alert-danger');
                if (existingAlert) existingAlert.remove();

                modalBody.insertBefore(alertDiv, form);

                // Auto close sau 5s
                setTimeout(() => {
                    try {
                        new bootstrap.Alert(alertDiv).close();
                    } catch (e) { }
                }, 5000);

                return;
            }
        } else {
            // Fallback: Nếu không có NhanSuValidatorClient, dùng native HTML5 validation
            if (!form.checkValidity()) {
                form.classList.add('was-validated');
                return;
            }
        }

        // Tiến hành lưu dữ liệu
        proceedSave();
    });
}

function proceedSave() {
    const form = document.getElementById('frmNhanSu');
    const btnSave = document.getElementById('btnSave');
    
    btnSave.disabled = true;
    btnSave.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Đang lưu...';

    const formData = new FormData(form);
    const maNV = parseInt(formData.get('MaNV')) || 0;
    const matKhau = formData.get('MatKhau');

    // Nếu sửa mà không nhập mật khẩu thì bỏ field này
    if (maNV > 0 && !matKhau) formData.delete('MatKhau');

    const tokenInput = form.querySelector('input[name="__RequestVerificationToken"]');
    if (!tokenInput) {
        btnSave.disabled = false;
        btnSave.innerHTML = '<i class="bi bi-save"></i> Lưu dữ liệu';
        return;
    }

    fetch('/NhanSu/Save', {
        method: 'POST',
        body: new URLSearchParams(formData),
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': tokenInput.value
        }
    })
        .then(r => r.json())
        .then(res => {
            btnSave.disabled = false;
            btnSave.innerHTML = '<i class="bi bi-save"></i> Lưu dữ liệu';

            if (res.success) {
                // Hiển thị thông báo thành công
                const successAlert = document.createElement('div');
                successAlert.className = 'alert alert-success alert-dismissible fade show mt-3';
                successAlert.innerHTML = `
                <i class="bi bi-check-circle"></i> ${res.message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            `;

                const modalBody = form.parentElement;
                const existingAlert = modalBody.querySelector('.alert');
                if (existingAlert) existingAlert.remove();

                modalBody.insertBefore(successAlert, form);

                // Đóng modal và reload sau 1s
                setTimeout(() => {
                    bootstrap.Modal.getInstance(document.getElementById('nhanSuModal'))?.hide();
                    location.reload();
                }, 1000);
            } else {
                // Xóa bất kỳ alert nào đang hiển thị ở trên
                const modalBody = form.parentElement;
                const existingAlert = modalBody.querySelector('.alert');
                if (existingAlert) existingAlert.remove();

                // Hiển thị lỗi INLINE dưới input nếu có errorField
                if (res.errorField) {
                    const field = document.getElementById(res.errorField);

                    if (field && window.NhanSuValidatorClient) {
                        // Hiển thị lỗi ngay dưới input
                        window.NhanSuValidatorClient.showError(field, res.message);
                        // Focus vào field bị lỗi
                        field.focus();
                        field.scrollIntoView({ behavior: 'smooth', block: 'center' });
                    } else if (field) {
                        // Fallback: Không có validator, tự tạo error message
                        field.classList.add('is-invalid');
                        const feedback = field.parentElement.querySelector('.invalid-feedback');
                        if (feedback) {
                            feedback.textContent = res.message;
                            feedback.style.display = 'block';
                        } else {
                            // Tạo mới invalid-feedback
                            const newFeedback = document.createElement('div');
                            newFeedback.className = 'invalid-feedback d-block';
                            newFeedback.textContent = res.message;
                            field.parentElement.appendChild(newFeedback);
                        }
                        field.focus();
                    }
                }
            }
        })
        .catch(err => {
            btnSave.disabled = false;
            btnSave.innerHTML = '<i class="bi bi-save"></i> Lưu dữ liệu';

            console.error('Lỗi:', err);

            const errorAlert = document.createElement('div');
            errorAlert.className = 'alert alert-danger alert-dismissible fade show mt-3';
            errorAlert.innerHTML = `
            <i class="bi bi-x-circle"></i> Lỗi kết nối server!
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;

            const modalBody = form.parentElement;
            const existingAlert = modalBody.querySelector('.alert');
            if (existingAlert) existingAlert.remove();

            modalBody.insertBefore(errorAlert, form);
        });
}

// Khóa/Mở khóa tài khoản (thay vì xóa)
function toggleLock(id, currentStatus) {
    const action = currentStatus ? 'khóa' : 'mở khóa';
    const confirmMsg = currentStatus
        ? 'Bạn có chắc chắn muốn KHÓA tài khoản này?\n\nTài khoản bị khóa sẽ không thể đăng nhập vào hệ thống.'
        : 'Bạn có chắc chắn muốn MỞ KHÓA tài khoản này?\n\nSau khi mở khóa, tài khoản có thể đăng nhập bình thường.';

    if (!confirm(confirmMsg)) return;

    const form = document.getElementById('frmNhanSu');
    const tokenInput = form?.querySelector('input[name="__RequestVerificationToken"]');
    if (!tokenInput) return;

    fetch('/NhanSu/ToggleLock', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': tokenInput.value
        },
        body: 'id=' + id
    })
        .then(r => r.json())
        .then(res => {
            if (res.success) {
                // Hiển thị thông báo
                alert(res.message);
                // Reload lại trang để cập nhật trạng thái
                setTimeout(() => location.reload(), 500);
            } else {
                alert('Lỗi: ' + res.message);
            }
        })
        .catch(e => {
            console.error('Lỗi:', e);
            alert('Lỗi kết nối đến server!');
        });
}