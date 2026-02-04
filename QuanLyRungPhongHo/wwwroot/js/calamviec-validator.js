/**
 * Client-side Validator cho Quản lý Ca Làm Việc
 * Validate realtime để giảm tải server và tăng trải nghiệm người dùng
 */

const CaLamViecValidatorClient = {
    // Regex patterns
    patterns: {
        time: /^([0-1][0-9]|2[0-3]):([0-5][0-9])$/
    },

    //Validate tên ca làm việc
    validateTenCa(value, fieldElement) {
        const trimmed = value.trim();

        if (!trimmed) {
            this.showError(fieldElement, 'Tên ca không được để trống!');
            return false;
        }

        if (trimmed.length < 3) {
            this.showError(fieldElement, 'Tên ca phải có ít nhất 3 ký tự!');
            return false;
        }

        if (trimmed.length > 100) {
            this.showError(fieldElement, 'Tên ca không được vượt quá 100 ký tự!');
            return false;
        }

        // Kiểm tra ký tự đặc biệt nguy hiểm
        if (/[<>"'%;()&+]/.test(trimmed)) {
            this.showError(fieldElement, 'Tên ca không được chứa ký tự đặc biệt: < > " \' % ; ( ) & +');
            return false;
        }

        // Kiểm tra khoảng trắng liên tiếp
        if (/\s{2,}/.test(trimmed)) {
            this.showError(fieldElement, 'Tên ca không được chứa nhiều khoảng trắng liên tiếp!');
            return false;
        }

        this.showSuccess(fieldElement, 'Tên ca hợp lệ');
        return true;
    },

    //Validate giờ (định dạng HH:mm)
    validateGio(value, fieldElement, fieldName = 'Giờ') {
        const trimmed = value.trim();

        if (!trimmed) {
            this.showError(fieldElement, `${fieldName} không được để trống!`);
            return false;
        }

        if (!this.patterns.time.test(trimmed)) {
            this.showError(fieldElement, `${fieldName} phải đúng định dạng HH:mm (VD: 08:00, 14:30)!`);
            return false;
        }

        // Parse và kiểm tra hợp lệ
        const parts = trimmed.split(':');
        const hour = parseInt(parts[0]);
        const minute = parseInt(parts[1]);

        if (hour < 0 || hour > 23) {
            this.showError(fieldElement, `${fieldName} không hợp lệ! Giờ phải từ 00 đến 23.`);
            return false;
        }

        if (minute < 0 || minute > 59) {
            this.showError(fieldElement, `${fieldName} không hợp lệ! Phút phải từ 00 đến 59.`);
            return false;
        }

        this.showSuccess(fieldElement, `${fieldName} hợp lệ`);
        return true;
    },

    //Validate logic giờ bắt đầu và kết thúc
    validateTimeLogic(gioBatDau, gioKetThuc, gioBatDauElement, gioKetThucElement) {
        if (!gioBatDau || !gioKetThuc) {
            return { isValid: false, errorMessage: 'Vui lòng nhập đầy đủ giờ bắt đầu và kết thúc!' };
        }

        try {
            const [startHour, startMin] = gioBatDau.split(':').map(Number);
            const [endHour, endMin] = gioKetThuc.split(':').map(Number);

            const startMinutes = startHour * 60 + startMin;
            let endMinutes = endHour * 60 + endMin;

            // Trường hợp ca qua đêm
            if (endMinutes < startMinutes) {
                endMinutes += 24 * 60;
            }

            if (endMinutes === startMinutes) {
                this.showError(gioKetThucElement, 'Giờ kết thúc phải sau giờ bắt đầu!');
                return { isValid: false, errorMessage: 'Giờ kết thúc phải sau giờ bắt đầu!' };
            }

            const durationMinutes = endMinutes - startMinutes;
            const durationHours = durationMinutes / 60;

            // Kiểm tra độ dài ca tối thiểu (30 phút)
            if (durationMinutes < 30) {
                this.showError(gioKetThucElement, 'Ca làm việc phải có ít nhất 30 phút!');
                return { isValid: false, errorMessage: 'Ca làm việc phải có ít nhất 30 phút!' };
            }

            // Kiểm tra độ dài ca tối đa (16 tiếng - cho phép ca qua đêm)
            if (durationHours > 16) {
                this.showError(gioKetThucElement, 'Ca làm việc không được vượt quá 16 tiếng!');
                return { isValid: false, errorMessage: 'Ca làm việc không được vượt quá 16 tiếng!' };
            }

            // Xóa lỗi nếu hợp lệ
            this.clearError(gioBatDauElement);
            this.clearError(gioKetThucElement);

            return {
                isValid: true,
                duration: durationHours.toFixed(1)
            };
        } catch (error) {
            this.showError(gioKetThucElement, 'Lỗi kiểm tra thời gian!');
            return { isValid: false, errorMessage: 'Lỗi kiểm tra thời gian!' };
        }
    },

    // Validate mô tả
    validateMoTa(value, fieldElement) {
        if (!value || value.trim() === '') {
            this.clearError(fieldElement);
            return true; // Mô tả không bắt buộc
        }

        const trimmed = value.trim();

        if (trimmed.length > 500) {
            this.showError(fieldElement, 'Mô tả không được vượt quá 500 ký tự!');
            return false;
        }

        // Kiểm tra ký tự nguy hiểm
        if (/[<>"']/.test(trimmed)) {
            this.showError(fieldElement, 'Mô tả không được chứa các ký tự: < > " \'');
            return false;
        }

        this.showSuccess(fieldElement, 'Mô tả hợp lệ');
        return true;
    },

    //Validate toàn bộ form
    validateForm() {
        const tenCa = document.getElementById('TenCa');
        const gioBatDau = document.getElementById('GioBatDau');
        const gioKetThuc = document.getElementById('GioKetThuc');
        const moTa = document.getElementById('MoTa');

        let isValid = true;

        // Validate tên ca
        if (tenCa && !this.validateTenCa(tenCa.value, tenCa)) {
            isValid = false;
        }

        // Validate giờ bắt đầu
        if (gioBatDau && !this.validateGio(gioBatDau.value, gioBatDau, 'Giờ bắt đầu')) {
            isValid = false;
        }

        // Validate giờ kết thúc
        if (gioKetThuc && !this.validateGio(gioKetThuc.value, gioKetThuc, 'Giờ kết thúc')) {
            isValid = false;
        }

        // Validate logic thời gian
        if (gioBatDau && gioKetThuc) {
            const timeLogicResult = this.validateTimeLogic(
                gioBatDau.value,
                gioKetThuc.value,
                gioBatDau,
                gioKetThuc
            );
            if (!timeLogicResult.isValid) {
                isValid = false;
            }
        }

        // Validate mô tả (optional)
        if (moTa && moTa.value.trim()) {
            if (!this.validateMoTa(moTa.value, moTa)) {
                isValid = false;
            }
        }

        return isValid;
    },

    //Hiển thị lỗi
    showError(fieldElement, message) {
        fieldElement.classList.remove('is-valid');
        fieldElement.classList.add('is-invalid');

        let feedback = fieldElement.parentNode.querySelector('.invalid-feedback');
        if (!feedback) {
            feedback = document.createElement('div');
            feedback.className = 'invalid-feedback';
            fieldElement.parentNode.appendChild(feedback);
        }
        feedback.textContent = message;
        feedback.style.display = 'block';
    },

    //Hiển thị thành công
    showSuccess(fieldElement, message = '') {
        fieldElement.classList.remove('is-invalid');
        fieldElement.classList.add('is-valid');

        let feedback = fieldElement.parentNode.querySelector('.valid-feedback');
        if (!feedback) {
            feedback = document.createElement('div');
            feedback.className = 'valid-feedback';
            fieldElement.parentNode.appendChild(feedback);
        }
        feedback.textContent = message;
        feedback.style.display = 'block';
    },

    //Xóa validation
    clearError(fieldElement) {
        fieldElement.classList.remove('is-valid', 'is-invalid');

        const invalidFeedback = fieldElement.parentNode.querySelector('.invalid-feedback');
        if (invalidFeedback) {
            invalidFeedback.style.display = 'none';
        }

        const validFeedback = fieldElement.parentNode.querySelector('.valid-feedback');
        if (validFeedback) {
            validFeedback.style.display = 'none';
        }
    }
};

// Export to window
window.CaLamViecValidatorClient = CaLamViecValidatorClient;

console.log('[calamviec-validator.js] Client-side validator đã tải!');