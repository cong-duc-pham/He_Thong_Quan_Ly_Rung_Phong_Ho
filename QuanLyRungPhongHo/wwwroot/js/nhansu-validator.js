/**
 * VALIDATION REALTIME CHO QUẢN LÝ NHÂN SỰ
 * Validate ngay khi người dùng đang nhập liệu
 */

const NhanSuValidatorClient = {
    // Regex patterns
    patterns: {
        hoTen: /^[a-zA-ZÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚĂĐĨŨƠàáâãèéêìíòóôõùúăđĩũơƯĂẠẢẤẦẨẪẬẮẰẲẴẶẸẺẼỀỀỂưăạảấầẩẫậắằẳẵặẹẻẽềềểỄỆỈỊỌỎỐỒỔỖỘỚỜỞỠỢỤỦỨỪễệỉịọỏốồổỗộớờởỡợụủứừỬỮỰỲỴÝỶỸửữựỳỵýỷỹ\s]{2,100}$/,
        sdt: /^(0|\+84)(3[2-9]|5[6|8|9]|7[0|6-9]|8[1-9]|9[0-9])[0-9]{7}$/,
        email: /^[a-zA-Z0-9]([a-zA-Z0-9._-]*[a-zA-Z0-9])?@[a-zA-Z0-9]([a-zA-Z0-9-]*[a-zA-Z0-9])?(\.[a-zA-Z]{2,})+$/,
        tenDangNhap: /^[a-zA-Z0-9_]{5,50}$/,
        password: /^.{6,100}$/
    },

    // Danh sách chức vụ hợp lệ
    chucVuHopLe: ['Kiểm lâm', 'Phó Kiểm lâm', 'Trưởng trạm', 'Phó trạm', 'Nhân viên bảo vệ rừng', 'Cán bộ kỹ thuật', 'Hướng dẫn viên'],

    // Danh sách đầu số hợp lệ VN
    dauSoHopLe: ['032', '033', '034', '035', '036', '037', '038', '039',
                 '056', '058', '059',
                 '070', '076', '077', '078', '079',
                 '081', '082', '083', '084', '085', '086', '087', '088', '089',
                 '090', '091', '092', '093', '094', '096', '097', '098', '099'],

    /**
     * Validate Họ Tên
     */
    validateHoTen(value, fieldElement) {
        const trimmed = value.trim();
        
        if (!trimmed) {
            this.showError(fieldElement, 'Họ tên không được để trống!');
            return false;
        }

        if (trimmed.length < 2) {
            this.showError(fieldElement, 'Họ tên phải có ít nhất 2 ký tự!');
            return false;
        }

        if (trimmed.length > 100) {
            this.showError(fieldElement, 'Họ tên không được vượt quá 100 ký tự!');
            return false;
        }

        if (!this.patterns.hoTen.test(trimmed)) {
            this.showError(fieldElement, 'Họ tên chỉ được chứa chữ cái tiếng Việt và khoảng trắng!');
            return false;
        }

        // Kiểm tra không chứa số
        if (/[0-9]/.test(trimmed)) {
            this.showError(fieldElement, 'Họ tên không được chứa số!');
            return false;
        }

        // Kiểm tra không chứa ký tự đặc biệt
        if (/[!@#$%^&*()_+=\[\]{};:'",.<>?/\\|`~]/.test(trimmed)) {
            this.showError(fieldElement, 'Họ tên không được chứa ký tự đặc biệt!');
            return false;
        }

        // Kiểm tra không có nhiều khoảng trắng liên tiếp
        if (/\s{2,}/.test(trimmed)) {
            this.showError(fieldElement, 'Họ tên không được chứa nhiều khoảng trắng liên tiếp!');
            return false;
        }

        // Kiểm tra phải có ít nhất 2 từ
        const words = trimmed.split(' ').filter(w => w.length > 0);
        if (words.length < 2) {
            this.showError(fieldElement, 'Họ tên phải có ít nhất 2 từ (Họ và Tên)!');
            return false;
        }

        this.showSuccess(fieldElement, '✓ Họ tên hợp lệ');
        return true;
    },

    /**
     * Validate Số Điện Thoại
     */
    validateSDT(value, fieldElement) {
        let sdt = value.trim().replace(/[\s\-\.]/g, ''); // Xóa khoảng trắng, dấu gạch ngang, dấu chấm

        if (!sdt) {
            this.showError(fieldElement, 'Số điện thoại không được để trống!');
            return false;
        }

        // Kiểm tra chỉ chứa số và dấu +
        if (!/^[\+0-9]+$/.test(sdt)) {
            this.showError(fieldElement, 'Số điện thoại chỉ được chứa số và dấu +!');
            return false;
        }

        // Chuẩn hóa
        if (sdt.startsWith('+84')) {
            sdt = '0' + sdt.substring(3);
        } else if (sdt.startsWith('84')) {
            sdt = '0' + sdt.substring(2);
        }

        // Kiểm tra độ dài chính xác
        if (sdt.length !== 10) {
            this.showError(fieldElement, 'Số điện thoại Việt Nam phải có đúng 10 chữ số!');
            return false;
        }

        // Kiểm tra đầu số hợp lệ
        const dauSo = sdt.substring(0, 3);
        if (!this.dauSoHopLe.includes(dauSo)) {
            this.showError(fieldElement, `Đầu số '${dauSo}' không hợp lệ! Vui lòng kiểm tra lại.`);
            return false;
        }

        // Validate pattern
        if (!this.patterns.sdt.test(sdt)) {
            this.showError(fieldElement, 'Số điện thoại không đúng định dạng Việt Nam!');
            return false;
        }

        this.showSuccess(fieldElement, `✓ SĐT hợp lệ: ${sdt}`);
        
        // Tự động chuẩn hóa giá trị trong input
        if (fieldElement.value !== sdt) {
            fieldElement.value = sdt;
        }
        
        return true;
    },

    /**
     * Validate Email
     */
    validateEmail(value, fieldElement) {
        const trimmed = value.trim().toLowerCase();

        // Email BẮT BUỘC
        if (!trimmed) {
            this.showError(fieldElement, 'Email không được để trống!');
            return false;

        if (!this.patterns.email.test(trimmed)) {
            this.showError(fieldElement, 'Email không đúng định dạng! (VD: example@domain.com)');
            return false;
        }

        // Kiểm tra không chứa dấu chấm liên tiếp
        if (trimmed.includes('..')) {
            this.showError(fieldElement, 'Email không được chứa dấu chấm liên tiếp!');
            return false;
        }

        // Kiểm tra không bắt đầu/kết thúc bằng dấu chấm
        if (trimmed.startsWith('.') || trimmed.endsWith('.')) {
            this.showError(fieldElement, 'Email không được bắt đầu hoặc kết thúc bằng dấu chấm!');
            return false;
        }

        this.showSuccess(fieldElement, '✓ Email hợp lệ');
        
        // Tự động chuẩn hóa
        if (fieldElement.value !== trimmed) {
            fieldElement.value = trimmed;
        }
        
        return true;
    },

    /**
     * Validate Tên Đăng Nhập
     */
    validateTenDangNhap(value, fieldElement) {
        const trimmed = value.trim();

        if (!trimmed) {
            this.showError(fieldElement, 'Tên đăng nhập không được để trống!');
            return false;
        }

        if (trimmed.length < 5) {
            this.showError(fieldElement, 'Tên đăng nhập phải có ít nhất 5 ký tự!');
            return false;
        }

        if (trimmed.length > 50) {
            this.showError(fieldElement, 'Tên đăng nhập không được vượt quá 50 ký tự!');
            return false;
        }

        if (!this.patterns.tenDangNhap.test(trimmed)) {
            this.showError(fieldElement, 'Tên đăng nhập chỉ được chứa chữ cái (a-z, A-Z), số (0-9) và dấu gạch dưới (_)!');
            return false;
        }

        if (trimmed.startsWith('_') || trimmed.endsWith('_')) {
            this.showError(fieldElement, 'Tên đăng nhập không được bắt đầu hoặc kết thúc bằng dấu gạch dưới!');
            return false;
        }

        if (trimmed.includes('__')) {
            this.showError(fieldElement, 'Tên đăng nhập không được chứa nhiều dấu gạch dưới liên tiếp!');
            return false;
        }

        if (/^\d+$/.test(trimmed)) {
            this.showError(fieldElement, 'Tên đăng nhập không được chỉ toàn số!');
            return false;
        }

        // Blacklist
        const blacklist = ['admin', 'root', 'system', 'administrator', 'superuser', 'test', 'guest'];
        if (blacklist.includes(trimmed.toLowerCase())) {
            this.showError(fieldElement, `Tên đăng nhập '${trimmed}' không được phép sử dụng!`);
            return false;
        }

        this.showSuccess(fieldElement, '✓ Tên đăng nhập hợp lệ');
        return true;
    },

    /**
     * Validate Mật Khẩu
     */
    validateMatKhau(value, fieldElement, isRequired) {
        if (!value) {
            if (isRequired) {
                this.showError(fieldElement, 'Mật khẩu không được để trống!');
                return false;
            } else {
                this.clearValidation(fieldElement);
                return true;
            }
        }

        if (value.length < 6) {
            this.showError(fieldElement, 'Mật khẩu phải có ít nhất 6 ký tự!');
            return false;
        }

        if (value.length > 100) {
            this.showError(fieldElement, 'Mật khẩu không được vượt quá 100 ký tự!');
            return false;
        }

        // Kiểm tra độ mạnh
        const hasUpper = /[A-Z]/.test(value);
        const hasLower = /[a-z]/.test(value);
        const hasDigit = /[0-9]/.test(value);
        const hasSpecial = /[!@#$%^&*()_+=\[\]{};:'"\\|,.<>?/~`-]/.test(value);

        const criteriaCount = (hasUpper ? 1 : 0) + (hasLower ? 1 : 0) + (hasDigit ? 1 : 0) + (hasSpecial ? 1 : 0);

        if (criteriaCount < 3) {
            this.showError(fieldElement, 'Mật khẩu phải chứa ít nhất 3 trong 4: chữ hoa, chữ thường, số, ký tự đặc biệt!');
            return false;
        }

        if (value.includes(' ')) {
            this.showError(fieldElement, 'Mật khẩu không được chứa khoảng trắng!');
            return false;
        }

        // Weak passwords
        const weakPasswords = ['123456', 'password', '12345678', 'qwerty', 'abc123', '111111', '123123'];
        if (weakPasswords.some(wp => value.toLowerCase().includes(wp))) {
            this.showError(fieldElement, 'Mật khẩu quá phổ biến, vui lòng chọn mật khẩu khác!');
            return false;
        }

        this.showSuccess(fieldElement, '✓ Mật khẩu đủ mạnh');
        return true;
    },

    /**
     * Validate Chức Vụ
     */
    validateChucVu(value, fieldElement) {
        if (!value) {
            this.showError(fieldElement, 'Vui lòng chọn chức vụ!');
            return false;
        }

        if (!this.chucVuHopLe.includes(value)) {
            this.showError(fieldElement, 'Chức vụ không hợp lệ!');
            return false;
        }

        this.showSuccess(fieldElement, '✓');
        return true;
    },

    /**
     * Validate Quyền
     */
    validateQuyen(value, fieldElement) {
        const quyenHopLe = ['Admin_Tinh', 'QuanLy_Xa', 'Kiem_Lam', 'NhanVien_Thon'];
        
        if (!value) {
            this.showError(fieldElement, 'Vui lòng chọn quyền hạn!');
            return false;
        }

        if (!quyenHopLe.includes(value)) {
            this.showError(fieldElement, 'Quyền không hợp lệ!');
            return false;
        }

        this.showSuccess(fieldElement, '✓');
        return true;
    },

    /**
     * Hiển thị lỗi
     */
    showError(fieldElement, message) {
        fieldElement.classList.remove('is-valid');
        fieldElement.classList.add('is-invalid');
        
        // Tìm hoặc tạo feedback element
        let feedback = fieldElement.nextElementSibling;
        if (!feedback || !feedback.classList.contains('invalid-feedback')) {
            feedback = document.createElement('div');
            feedback.className = 'invalid-feedback';
            fieldElement.parentNode.insertBefore(feedback, fieldElement.nextSibling);
        }
        feedback.textContent = message;
        feedback.style.display = 'block';
    },

    /**
     * Hiển thị thành công
     */
    showSuccess(fieldElement, message = '') {
        fieldElement.classList.remove('is-invalid');
        fieldElement.classList.add('is-valid');
        
        // Tìm hoặc tạo feedback element
        let feedback = fieldElement.parentNode.querySelector('.valid-feedback');
        if (!feedback) {
            feedback = document.createElement('div');
            feedback.className = 'valid-feedback';
            fieldElement.parentNode.appendChild(feedback);
        }
        feedback.textContent = message;
        feedback.style.display = 'block';
    },

    /**
     * Xóa validation
     */
    clearValidation(fieldElement) {
        fieldElement.classList.remove('is-valid', 'is-invalid');
        
        const invalidFeedback = fieldElement.parentNode.querySelector('.invalid-feedback');
        if (invalidFeedback) {
            invalidFeedback.style.display = 'none';
        }
        
        const validFeedback = fieldElement.parentNode.querySelector('.valid-feedback');
        if (validFeedback) {
            validFeedback.style.display = 'none';
        }
    },

    /**
     * Khởi tạo validation realtime cho form
     */
    init() {
        console.log('🔧 Đang khởi tạo validation realtime...');
        this.bindEvents();
        console.log('✅ Validation realtime đã được khởi tạo!');
    },

    /**
     * Bind events vào form fields
     */
    bindEvents() {
        console.log('📋 Binding events vào form fields...');

        // Họ Tên
        const hoTenField = document.getElementById('HoTen');
        console.log('🔍 HoTen field:', hoTenField);
        if (hoTenField) {
            console.log('✅ Binding events to HoTen');
            hoTenField.addEventListener('input', () => {
                console.log('🎯 HoTen input event fired!');
                this.validateHoTen(hoTenField.value, hoTenField);
            });
            hoTenField.addEventListener('blur', () => {
                this.validateHoTen(hoTenField.value, hoTenField);
            });
        } else {
            console.error('❌ HoTen field NOT FOUND!');
        }

        // SĐT
        const sdtField = document.getElementById('SDT');
        console.log('🔍 SDT field:', sdtField);
        if (sdtField) {
            console.log('✅ Binding events to SDT');
            sdtField.addEventListener('input', () => {
                console.log('🎯 SDT input event fired!');
                this.validateSDT(sdtField.value, sdtField);
            });
            sdtField.addEventListener('blur', () => {
                this.validateSDT(sdtField.value, sdtField);
            });
        } else {
            console.error('❌ SDT field NOT FOUND!');
        }

        // Email
        const emailField = document.getElementById('Email');
        console.log('🔍 Email field:', emailField);
        if (emailField) {
            console.log('✅ Binding events to Email');
            emailField.addEventListener('input', () => {
                console.log('🎯 Email input event fired!');
                this.validateEmail(emailField.value, emailField);
            });
            emailField.addEventListener('blur', () => {
                this.validateEmail(emailField.value, emailField);
            });
        } else {
            console.error('❌ Email field NOT FOUND!');
        }

        // Tên Đăng Nhập
        const tenDangNhapField = document.getElementById('TenDangNhap');
        if (tenDangNhapField) {
            tenDangNhapField.addEventListener('input', () => {
                this.validateTenDangNhap(tenDangNhapField.value, tenDangNhapField);
            });
            tenDangNhapField.addEventListener('blur', () => {
                this.validateTenDangNhap(tenDangNhapField.value, tenDangNhapField);
            });
        }

        // Mật Khẩu
        const matKhauField = document.getElementById('MatKhau');
        if (matKhauField) {
            matKhauField.addEventListener('input', () => {
                const isRequired = document.getElementById('MaNV').value === '0';
                this.validateMatKhau(matKhauField.value, matKhauField, isRequired);
            });
            matKhauField.addEventListener('blur', () => {
                const isRequired = document.getElementById('MaNV').value === '0';
                this.validateMatKhau(matKhauField.value, matKhauField, isRequired);
            });
        }

        // Chức Vụ
        const chucVuField = document.getElementById('ChucVu');
        if (chucVuField) {
            chucVuField.addEventListener('change', () => {
                this.validateChucVu(chucVuField.value, chucVuField);
            });
        }

        // Quyền
        const quyenField = document.getElementById('Quyen');
        if (quyenField) {
            quyenField.addEventListener('change', () => {
                this.validateQuyen(quyenField.value, quyenField);
            });
        }

        // Mã Xã
        const maXaField = document.getElementById('MaXa');
        if (maXaField) {
            maXaField.addEventListener('change', () => {
                if (!maXaField.value) {
                    this.showError(maXaField, 'Vui lòng chọn địa bàn!');
                } else {
                    this.showSuccess(maXaField, '✓');
                }
            });
        }

        console.log('✅ Events đã được bind!');
    },

    /**
     * Validate toàn bộ form trước khi submit
     */
    validateForm() {
        const hoTen = document.getElementById('HoTen');
        const sdt = document.getElementById('SDT');
        const email = document.getElementById('Email');
        const tenDangNhap = document.getElementById('TenDangNhap');
        const matKhau = document.getElementById('MatKhau');
        const chucVu = document.getElementById('ChucVu');
        const quyen = document.getElementById('Quyen');
        const maXa = document.getElementById('MaXa');
        const maNV = document.getElementById('MaNV');

        const isNew = maNV.value === '0';

        let isValid = true;

        if (hoTen && !this.validateHoTen(hoTen.value, hoTen)) isValid = false;
        if (sdt && !this.validateSDT(sdt.value, sdt)) isValid = false;
        if (email && !this.validateEmail(email.value, email)) isValid = false;
        if (tenDangNhap && !this.validateTenDangNhap(tenDangNhap.value, tenDangNhap)) isValid = false;
        if (matKhau && !this.validateMatKhau(matKhau.value, matKhau, isNew)) isValid = false;
        if (chucVu && !this.validateChucVu(chucVu.value, chucVu)) isValid = false;
        if (quyen && !this.validateQuyen(quyen.value, quyen)) isValid = false;
        
        if (maXa && !maXa.value) {
            this.showError(maXa, 'Vui lòng chọn địa bàn!');
            isValid = false;
        }

        return isValid;
    }
};

// Khởi tạo khi document ready
document.addEventListener('DOMContentLoaded', function() {
    NhanSuValidatorClient.init();
});

// Export để có thể gọi từ nhansu.js
window.NhanSuValidatorClient = NhanSuValidatorClient;
