// Mở popup thêm nhân sự mới
function openModal() {
    console.log('=== Mở modal thêm mới ===');
    
    const form = document.getElementById('frmNhanSu');
    const modalElement = document.getElementById('nhanSuModal');
    
    // Đóng modal cũ nếu có
    const existingModal = bootstrap.Modal.getInstance(modalElement);
    if (existingModal) {
        existingModal.hide();
        existingModal.dispose();
    }
    
    // Reset form về trạng thái ban đầu
    form.reset();
    form.classList.remove('was-validated');
    
    // Chế độ thêm mới: MaNV = 0
    document.getElementById('MaNV').value = '0';
    document.getElementById('modalTitle').textContent = "Thêm Nhân sự mới";
    document.getElementById('TenDangNhap').readOnly = false;
    
    // Khi thêm mới thì mật khẩu bắt buộc nhập
    const matKhauField = document.getElementById('MatKhau');
    matKhauField.value = '';
    matKhauField.required = true;
    matKhauField.placeholder = 'Tối thiểu 6 ký tự';
    
    const passRequired = document.getElementById('passRequired');
    const passNote = document.getElementById('passNote');
    if (passRequired) passRequired.style.display = 'inline';
    if (passNote) passNote.style.display = 'none';
    
    // Mở modal (không cho đóng khi click ngoài)
    const modal = new bootstrap.Modal(modalElement, {
        backdrop: 'static',
        keyboard: false
    });
    modal.show();
    
    console.log('Modal đã mở ở chế độ THÊM MỚI');
}

// Mở popup sửa thông tin nhân sự
function editData(id, event) {
    console.log('=== Sửa nhân sự ID:', id, '===');
    
    // Kiểm tra ID có hợp lệ không
    if (!id || id === 0) {
        alert('Lỗi: ID không hợp lệ!');
        return;
    }
    
    // Hiện loading trên nút sửa
    let btn = null;
    if (event) {
        btn = event.target.closest('button');
        if (btn) {
            btn.disabled = true;
            const originalHTML = btn.innerHTML;
            btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span>';
            btn.dataset.originalHTML = originalHTML;
        }
    }
    
    // Gọi API lấy thông tin nhân sự
    const url = '/NhanSu/GetById?id=' + id;
    console.log('Đang tải dữ liệu từ:', url);
    
    fetch(url)
        .then(response => {
            console.log('HTTP Status:', response.status);
            
            if (!response.ok) {
                throw new Error('Lỗi HTTP ' + response.status);
            }
            
            return response.json();
        })
        .then(res => {
            console.log('Dữ liệu nhận được:', res);
            
            // Khôi phục nút về trạng thái bình thường
            if (btn) {
                btn.disabled = false;
                btn.innerHTML = btn.dataset.originalHTML || '<i class="bi bi-pencil"></i>';
            }
            
            // Kiểm tra server có trả lỗi không
            if (res.success === false) {
                alert(res.message || "Không tìm thấy dữ liệu!");
                return;
            }
            
            // Chuẩn hóa dữ liệu (server trả về camelCase)
            const data = {
                maNV: res.maNV || res.MaNV,
                hoTen: res.hoTen || res.HoTen || '',
                chucVu: res.chucVu || res.ChucVu || '',
                sdt: res.sdt || res.SDT || '',
                maXa: res.maXa || res.MaXa || '',
                tenDangNhap: res.tenDangNhap || res.TenDangNhap || '',
                quyen: res.quyen || res.Quyen || 'NhanVien_Thon'
            };
            
            console.log('Dữ liệu đã chuẩn hóa:', data);
            
            if (!data.maNV || data.maNV === 0) {
                console.error('Lỗi: MaNV không hợp lệ:', data.maNV);
                alert('Không nhận được dữ liệu từ server!');
                return;
            }
            
            // Lấy form và modal
            const modalElement = document.getElementById('nhanSuModal');
            const form = document.getElementById('frmNhanSu');
            
            if (!modalElement || !form) {
                alert('Lỗi: Không tìm thấy form!');
                return;
            }
            
            // Đóng modal cũ nếu có
            const existingModal = bootstrap.Modal.getInstance(modalElement);
            if (existingModal) {
                existingModal.hide();
                existingModal.dispose();
            }
            
            // Xóa validation cũ
            form.classList.remove('was-validated');
            
            // Điền dữ liệu vào form
            document.getElementById('MaNV').value = data.maNV;
            document.getElementById('HoTen').value = data.hoTen;
            document.getElementById('ChucVu').value = data.chucVu;
            document.getElementById('SDT').value = data.sdt;
            document.getElementById('MaXa').value = data.maXa;
            document.getElementById('TenDangNhap').value = data.tenDangNhap;
            document.getElementById('Quyen').value = data.quyen;
            
            console.log('Form đã được điền dữ liệu');
            
            // Khi sửa thì mật khẩu không bắt buộc (để trống = không đổi)
            const matKhauField = document.getElementById('MatKhau');
            matKhauField.value = '';
            matKhauField.required = false;
            matKhauField.placeholder = 'Để trống nếu không đổi';
            
            const passRequired = document.getElementById('passRequired');
            const passNote = document.getElementById('passNote');
            if (passRequired) passRequired.style.display = 'none';
            if (passNote) passNote.style.display = 'block';
            
            // Đổi tiêu đề modal
            document.getElementById('modalTitle').textContent = "Cập nhật Nhân sự";
            
            // Mở modal
            const modal = new bootstrap.Modal(modalElement, {
                backdrop: 'static',
                keyboard: false
            });
            modal.show();
            
            console.log('Modal sửa đã mở thành công');
        })
        .catch(error => {
            console.error('Lỗi:', error);
            
            // Khôi phục nút
            if (btn) {
                btn.disabled = false;
                btn.innerHTML = btn.dataset.originalHTML || '<i class="bi bi-pencil"></i>';
            }
            
            alert("Lỗi: " + error.message);
        });
}

// Lưu dữ liệu (thêm mới hoặc cập nhật)
function saveData() {
    const form = document.getElementById('frmNhanSu');
    const maNVValue = document.getElementById('MaNV').value;
    
    console.log('=== Đang lưu dữ liệu ===');
    console.log('MaNV:', maNVValue);
    console.log('Chế độ:', parseInt(maNVValue) > 0 ? 'CẬP NHẬT' : 'THÊM MỚI');
    
    // Kiểm tra validation HTML5
    if (!form.checkValidity()) {
        form.classList.add('was-validated');
        alert('Vui lòng điền đầy đủ thông tin bắt buộc!');
        return;
    }
    
    // Disable nút lưu và hiện loading
    const btnSave = document.getElementById('btnSave');
    btnSave.disabled = true;
    btnSave.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Đang lưu...';
    
    const formData = new FormData(form);
    
    // Log để debug
    console.log('Dữ liệu form:');
    for (let [key, value] of formData.entries()) {
        console.log(`  ${key}: "${value}"`);
    }
    
    const maNV = parseInt(formData.get('MaNV')) || 0;
    const matKhau = formData.get('MatKhau');
    
    // Nếu đang sửa và không nhập mật khẩu thì xóa field đó (= không đổi pass)
    if (maNV > 0 && !matKhau) {
        formData.delete('MatKhau');
        console.log('Không thay đổi mật khẩu');
    }
    
    // Lấy CSRF token (bắt buộc cho POST request)
    const tokenInput = form.querySelector('input[name="__RequestVerificationToken"]');
    if (!tokenInput) {
        alert('Lỗi: Không tìm thấy CSRF token!');
        btnSave.disabled = false;
        btnSave.innerHTML = '<i class="bi bi-save"></i> Lưu dữ liệu';
        return;
    }
    
    // Gọi API lưu
    fetch('/NhanSu/Save', {
        method: 'POST',
        body: new URLSearchParams(formData),
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': tokenInput.value
        }
    })
    .then(response => {
        if (!response.ok) {
            throw new Error('HTTP ' + response.status);
        }
        return response.json();
    })
    .then(res => {
        console.log('Kết quả:', res);
        
        // Khôi phục nút lưu
        btnSave.disabled = false;
        btnSave.innerHTML = '<i class="bi bi-save"></i> Lưu dữ liệu';
        
        if (res.success) {
            // Đóng modal
            const modalElement = document.getElementById('nhanSuModal');
            const modal = bootstrap.Modal.getInstance(modalElement);
            if (modal) modal.hide();
            
            // Thông báo và reload trang
            alert(res.message);
            window.location.reload();
        } else {
            alert("Lỗi: " + res.message);
        }
    })
    .catch(error => {
        // Khôi phục nút lưu
        btnSave.disabled = false;
        btnSave.innerHTML = '<i class="bi bi-save"></i> Lưu dữ liệu';
        alert("Lỗi: " + error.message);
        console.error('Error:', error);
    });
}

// Xóa nhân sự
function deleteData(id) {
    // Confirm trước khi xóa
    if (!confirm("Bạn có chắc chắn muốn xóa nhân sự này và tài khoản liên quan?")) {
        return;
    }
    
    // Lấy CSRF token
    const form = document.getElementById('frmNhanSu');
    const tokenInput = form.querySelector('input[name="__RequestVerificationToken"]');
    
    if (!tokenInput) {
        alert('Lỗi: Không tìm thấy CSRF token!');
        return;
    }
    
    // Gọi API xóa
    fetch('/NhanSu/Delete', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': tokenInput.value
        },
        body: 'id=' + id
    })
    .then(response => response.json())
    .then(res => {
        alert(res.success ? res.message : "Lỗi: " + res.message);
        if (res.success) window.location.reload();
    })
    .catch(error => {
        alert("Lỗi: " + error.message);
        console.error('Error:', error);
    });
}

// Tự động ẩn alert sau 5 giây
document.addEventListener('DOMContentLoaded', function() {
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(function(alert) {
        setTimeout(function() {
            try {
                const bsAlert = new bootstrap.Alert(alert);
                bsAlert.close();
            } catch (e) {
                // Alert đã bị đóng rồi, không làm gì
                console.log('Alert đã đóng');
            }
        }, 5000);
    });
});