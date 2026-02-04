// Xác thực form đặt lại mật khẩuKiểm tra độ mạnh, yêu cầu ký tự và xác nhận khớp


(function() {
    'use strict';   

    const newPasswordInput = document.getElementById('newPassword');
    const confirmPasswordInput = document.getElementById('confirmPassword');
    const submitBtn = document.getElementById('submitBtn');
    const strengthFill = document.getElementById('strengthFill');
    const strengthText = document.getElementById('strengthText');
    const passwordMatchMessage = document.getElementById('passwordMatchMessage');
    const toggleNewPassword = document.getElementById('toggleNewPassword');
    const toggleConfirmPassword = document.getElementById('toggleConfirmPassword');

    const requirementIds = {
        length: 'req-length',
        upper: 'req-upper',
        lower: 'req-lower',
        number: 'req-number',
        special: 'req-special'
    };

    // Kiểm tra độ mạnh mật khẩu
    function checkPasswordStrength(password) {
        let strength = 0;
        
        const requirements = {
            length: password.length >= 8,
            upper: /[A-Z]/.test(password),
            lower: /[a-z]/.test(password),
            number: /\d/.test(password),
            special: /[@$!%*?&]/.test(password)
        };

        for (const key in requirements) {
            if (requirements[key]) strength++;
        }

        updateRequirementIndicators(requirements);
        updateStrengthBar(password.length, strength);

        return strength === 5;
    }

    // Cập nhật icon yêu cầu
    function updateRequirementIndicators(requirements) {
        for (const [key, value] of Object.entries(requirements)) {
            const element = document.getElementById(requirementIds[key]);
            if (element) {
                element.classList.toggle('met', value);
            }
        }
    }

    // Cập nhật thanh độ mạnh
    function updateStrengthBar(passwordLength, strength) {
        if (!strengthFill || !strengthText) return;

        strengthFill.className = 'strength-fill';
        strengthText.className = 'strength-text';

        if (passwordLength === 0) {
            strengthFill.style.width = '0%';
            strengthText.textContent = 'Nhập mật khẩu để kiểm tra độ mạnh';
        } else if (strength <= 2) {
            strengthFill.classList.add('weak');
            strengthText.textContent = 'Yếu - Cần cải thiện';
            strengthText.classList.add('weak');
        } else if (strength === 3 || strength === 4) {
            strengthFill.classList.add('medium');
            strengthText.textContent = 'Trung bình - Tạm được';
            strengthText.classList.add('medium');
        } else {
            strengthFill.classList.add('strong');
            strengthText.textContent = 'Mạnh - Tuyệt vời!';
            strengthText.classList.add('strong');
        }
    }

    // Kiểm tra mật khẩu xác nhận
    function checkPasswordMatch() {
        if (!confirmPasswordInput || !newPasswordInput || !passwordMatchMessage) {
            return false;
        }

        if (confirmPasswordInput.value === '') {
            passwordMatchMessage.textContent = '';
            return false;
        }

        const isMatch = newPasswordInput.value === confirmPasswordInput.value;

        if (isMatch) {
            passwordMatchMessage.innerHTML = 
                '<span style="color: #28a745;"><i class="fas fa-check-circle"></i> Mật khẩu khớp</span>';
        } else {
            passwordMatchMessage.innerHTML = 
                '<span style="color: #dc3545;"><i class="fas fa-times-circle"></i> Mật khẩu không khớp</span>';
        }

        return isMatch;
    }

    // Cập nhật trạng thái nút submit
    function updateSubmitButton() {
        if (!submitBtn || !newPasswordInput) return;

        const isPasswordStrong = checkPasswordStrength(newPasswordInput.value);
        const passwordsMatch = checkPasswordMatch();
        const isValid = isPasswordStrong && passwordsMatch;

        submitBtn.disabled = !isValid;
    }

    // Bật/tắt hiển thị mật khẩu
    function togglePasswordVisibility(toggleIcon, inputField) {
        if (!toggleIcon || !inputField) return;

        const isPassword = inputField.type === 'password';
        inputField.type = isPassword ? 'text' : 'password';
        toggleIcon.classList.toggle('fa-eye', !isPassword);
        toggleIcon.classList.toggle('fa-eye-slash', isPassword);
    }

    // Khởi tạo event listeners
    function initEventListeners() {
        if (newPasswordInput) {
            newPasswordInput.addEventListener('input', updateSubmitButton);
        }

        if (confirmPasswordInput) {
            confirmPasswordInput.addEventListener('input', updateSubmitButton);
        }

        if (toggleNewPassword) {
            toggleNewPassword.addEventListener('click', function() {
                togglePasswordVisibility(this, newPasswordInput);
            });
        }

        if (toggleConfirmPassword) {
            toggleConfirmPassword.addEventListener('click', function() {
                togglePasswordVisibility(this, confirmPasswordInput);
            });
        }
    }

    // Khởi tạo
    function init() {
        initEventListeners();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
