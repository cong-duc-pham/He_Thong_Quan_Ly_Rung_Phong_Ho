// Đơn giản hóa - không dùng namespace, chỉ dùng inline handlers
// Load script này trực tiếp khi document ready

(function() {
    'use strict';

    // Debounce search để tránh gọi API quá nhiều lần
    function debounce(func, wait) {
        let timeout;
        return function(...args) {
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
                tableRows += `
                    <tr>
                        <td>
                            <div class="fw-bold">${item.hoTen}</div>
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
                            <button class="btn btn-sm btn-outline-primary" onclick="editData(${item.maNV}, event)">
                                <i class="bi bi-pencil"></i>
                            </button>
                            <button class="btn btn-sm btn-outline-danger" onclick="deleteData(${item.maNV})">
                                <i class="bi bi-trash"></i>
                            </button>
                        </td>
                    </tr>`;
            });
            tableBody.innerHTML = tableRows;

            // Render mobile view
            let mobileCards = '';
            items.forEach(item => {
                const emailMobileDisplay = item.email ? `<p class="mb-1 small"><i class="bi bi-envelope"></i> ${item.email}</p>` : '';
                mobileCards += `
                    <div class="card mb-3 shadow-sm border-start border-4 border-success">
                        <div class="card-body">
                            <div class="d-flex justify-content-between mb-2">
                                <h6 class="fw-bold text-success mb-0">${item.hoTen}</h6>
                                <span class="badge bg-primary">${item.chucVu}</span>
                            </div>
                            <p class="mb-1 small"><i class="bi bi-geo-alt"></i> ${item.tenXa}</p>
                            <p class="mb-1 small"><i class="bi bi-telephone"></i> ${item.sdt}</p>
                            ${emailMobileDisplay}
                            <p class="mb-2 small"><i class="bi bi-person"></i> TK: ${item.tenDangNhap || 'Chưa có'}</p>
                            <div class="d-flex gap-2">
                                <button class="btn btn-outline-primary btn-sm flex-fill" onclick="editData(${item.maNV}, event)">Sửa</button>
                                <button class="btn btn-outline-danger btn-sm flex-fill" onclick="deleteData(${item.maNV})">Xóa</button>
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
                } catch(e) {}
            }, 5000);
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    window.NhanSuSearchInit = init;
})();

// Mở modal thêm nhân sự mới
function openModal() {
    const form = document.getElementById('frmNhanSu');
    const modalElement = document.getElementById('nhanSuModal');
    if (!form || !modalElement) return;

    const modal = bootstrap.Modal.getInstance(modalElement) || new bootstrap.Modal(modalElement, { backdrop: 'static', keyboard: false });
    
    form.reset();
    form.classList.remove('was-validated');
    document.getElementById('MaNV').value = '0';
    document.getElementById('modalTitle').textContent = 'Thêm Nhân sự mới';
    
    const matKhauField = document.getElementById('MatKhau');
    matKhauField.required = true;
    matKhauField.placeholder = 'Tối thiểu 6 ký tự';
    
    const passRequired = document.getElementById('passRequired');
    const passNote = document.getElementById('passNote');
    if (passRequired) passRequired.style.display = 'inline';
    if (passNote) passNote.style.display = 'none';
    
    modal.show();
}

// Mở modal sửa nhân sự
function editData(id, event) {
    if (!id || id === 0) return;
    
    let btn = event?.target.closest('button');
    if (btn) {
        btn.disabled = true;
        btn.dataset.originalHTML = btn.innerHTML;
        btn.innerHTML = 'Đang tải...';
    }

    fetch('/NhanSu/GetById?id=' + id)
        .then(r => r.json())
        .then(res => {
            // Khôi phục nút
            if (btn) {
                btn.disabled = false;
                btn.innerHTML = btn.dataset.originalHTML || 'Sửa';
            }
            
            if (!res.success) return;

            const form = document.getElementById('frmNhanSu');
            const modalElement = document.getElementById('nhanSuModal');
            if (!form || !modalElement) return;

            form.classList.remove('was-validated');
            document.getElementById('MaNV').value = res.maNV;
            document.getElementById('HoTen').value = res.hoTen || '';
            document.getElementById('ChucVu').value = res.chucVu || '';
            document.getElementById('SDT').value = res.sdt || '';
            document.getElementById('Email').value = res.email || '';
            document.getElementById('MaXa').value = res.maXa || '';
            document.getElementById('TenDangNhap').value = res.tenDangNhap || '';
            document.getElementById('Quyen').value = res.quyen || 'NhanVien_Thon';
            
            // Mật khẩu không bắt buộc khi sửa
            const matKhauField = document.getElementById('MatKhau');
            matKhauField.value = '';
            matKhauField.required = false;
            matKhauField.placeholder = 'Để trống nếu không đổi';

            const passRequired = document.getElementById('passRequired');
            const passNote = document.getElementById('passNote');
            if (passRequired) passRequired.style.display = 'none';
            if (passNote) passNote.style.display = 'block';

            document.getElementById('modalTitle').textContent = 'Cập nhật Nhân sự';

            const modal = bootstrap.Modal.getInstance(modalElement) || new bootstrap.Modal(modalElement, { backdrop: 'static', keyboard: false });
            modal.show();
        })
        .catch(e => {
            if (btn) {
                btn.disabled = false;
                btn.innerHTML = btn.dataset.originalHTML || 'Sửa';
            }
            console.error('Lỗi:', e);
        });
}

// Lưu nhân sự
function saveData() {
    const form = document.getElementById('frmNhanSu');
    if (!form.checkValidity()) {
        form.classList.add('was-validated');
        return;
    }

    const btnSave = document.getElementById('btnSave');
    btnSave.disabled = true;
    btnSave.innerHTML = 'Đang lưu...';

    const formData = new FormData(form);
    const maNV = parseInt(formData.get('MaNV')) || 0;
    const matKhau = formData.get('MatKhau');

    // Nếu sửa mà không nhập mật khẩu thì bỏ field này
    if (maNV > 0 && !matKhau) formData.delete('MatKhau');

    const tokenInput = form.querySelector('input[name="__RequestVerificationToken"]');
    if (!tokenInput) {
        btnSave.disabled = false;
        btnSave.innerHTML = 'Lưu dữ liệu';
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
        btnSave.innerHTML = 'Lưu dữ liệu';
        
        if (res.success) {
            bootstrap.Modal.getInstance(document.getElementById('nhanSuModal'))?.hide();
            setTimeout(() => location.reload(), 500);
        }
    })
    .catch(() => {
        btnSave.disabled = false;
        btnSave.innerHTML = 'Lưu dữ liệu';
    });
}

// Xóa nhân sự
function deleteData(id) {
    if (!confirm('Bạn có chắc chắn muốn xóa nhân sự này và tài khoản liên quan?')) return;

    const form = document.getElementById('frmNhanSu');
    const tokenInput = form?.querySelector('input[name="__RequestVerificationToken"]');
    if (!tokenInput) return;

    fetch('/NhanSu/Delete', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': tokenInput.value
        },
        body: 'id=' + id
    })
    .then(r => r.json())
    .then(res => {
        if (res.success) setTimeout(() => location.reload(), 500);
    })
    .catch(e => console.error('Lỗi:', e));
}