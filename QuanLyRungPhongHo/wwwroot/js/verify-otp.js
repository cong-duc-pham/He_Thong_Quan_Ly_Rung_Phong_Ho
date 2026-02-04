// Xác thực mã OTP;Xử lý nhập liệu, đếm ngược thời gian và gửi lại mã


(function() {
    'use strict';

    const otpInputs = document.querySelectorAll('.otp-input');
    const otpCodeHidden = document.getElementById('otpCodeHidden');
    const timerText = document.getElementById('timerText');
    const resendBtn = document.getElementById('resendBtn');
    const resendTimerElement = document.getElementById('resendTimer');
    const submitBtn = document.getElementById('submitBtn');

    let remainingTime = parseInt(timerText?.dataset.remainingTime || '600', 10);
    let resendTimer = 30;

    // Khởi tạo xử lý input OTP
    function initOtpInputs() {
        otpInputs.forEach((input, index) => {
            input.addEventListener('keyup', function(e) {
                this.value = this.value.replace(/[^0-9]/g, '');

                if (this.value && index < otpInputs.length - 1) {
                    otpInputs[index + 1].focus();
                }

                updateOtpCode();
            });

            input.addEventListener('keydown', function(e) {
                if (e.key === 'Backspace' && !this.value && index > 0) {
                    otpInputs[index - 1].focus();
                }
            });

            input.addEventListener('paste', function(e) {
                e.preventDefault();
                const pastedData = (e.clipboardData || window.clipboardData).getData('text');
                const pastedDigits = pastedData.replace(/[^0-9]/g, '');

                pastedDigits.split('').forEach((digit, i) => {
                    if (index + i < otpInputs.length) {
                        otpInputs[index + i].value = digit;
                    }
                });

                updateOtpCode();
                const lastIndex = Math.min(index + pastedDigits.length, otpInputs.length - 1);
                otpInputs[lastIndex].focus();
            });
        });
    }

    // Đồng bộ giá trị vào hidden field
    function updateOtpCode() {
        if (otpCodeHidden) {
            const otpCode = Array.from(otpInputs).map(input => input.value).join('');
            otpCodeHidden.value = otpCode;
        }
    }

    // Đếm ngược thời gian hết hạn
    function updateTimer() {
        if (!timerText) return;

        const minutes = Math.floor(remainingTime / 60);
        const seconds = remainingTime % 60;
        
        timerText.textContent = `${minutes}:${seconds.toString().padStart(2, '0')}`;

        timerText.classList.remove('warning', 'danger');
        if (remainingTime <= 60) {
            timerText.classList.add('danger');
        } else if (remainingTime <= 180) {
            timerText.classList.add('warning');
        }

        if (remainingTime > 0) {
            remainingTime--;
            setTimeout(updateTimer, 1000);
        } else {
            handleTimerExpired();
        }
    }

    // Vô hiệu hóa form khi hết hạn
    function handleTimerExpired() {
        timerText.textContent = 'Hết hạn';
        if (submitBtn) submitBtn.disabled = true;
        otpInputs.forEach(input => input.disabled = true);
    }

    // Đếm ngược nút gửi lại
    function updateResendTimer() {
        if (!resendTimerElement || !resendBtn) return;

        if (resendTimer > 0) {
            resendTimerElement.textContent = resendTimer;
            resendTimer--;
            setTimeout(updateResendTimer, 1000);
        } else {
            resendBtn.disabled = false;
            resendBtn.innerHTML = 'Gửi lại mã OTP';
        }
    }

    // Xử lý gửi lại OTP
    function handleResendOtp(e) {
        e.preventDefault();
        
        // TODO: Tích hợp API gửi lại OTP
        console.log('Đang gửi lại OTP...');
        
        resendTimer = 30;
        resendBtn.disabled = true;
        updateResendTimer();
    }

    // Khởi tạo
    function init() {
        initOtpInputs();
        updateTimer();
        updateResendTimer();

        if (resendBtn) {
            resendBtn.addEventListener('click', handleResendOtp);
        }

        if (otpInputs.length > 0) {
            otpInputs[0].focus();
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
