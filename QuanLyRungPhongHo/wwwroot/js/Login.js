// Toggle Password Visibility
document.addEventListener('DOMContentLoaded', function () {
    const togglePassword = document.getElementById('togglePassword');
    const passwordInput = document.getElementById('password');

    if (togglePassword && passwordInput) {
        togglePassword.addEventListener('click', function () {
            const type = passwordInput.getAttribute('type') === 'password' ? 'text' : 'password';
            passwordInput.setAttribute('type', type);

            // Toggle icon
            this.classList.toggle('fa-eye-slash');
            this.classList.toggle('fa-eye');
        });
    }

    // Input validation styling
    const usernameInput = document.getElementById('username');

    if (usernameInput) {
        usernameInput.addEventListener('keyup', function () {
            if (this.value.length < 3 && this.value.length > 0) {
                this.style.borderColor = '#ff9800';
            } else if (this.value.length >= 3) {
                this.style.borderColor = '#2d7a3e';
            } else {
                this.style.borderColor = '#e0e0e0';
            }
        });

        // Focus first input on page load
        usernameInput.focus();
    }

    if (passwordInput) {
        passwordInput.addEventListener('keyup', function () {
            if (this.value.length < 6 && this.value.length > 0) {
                this.style.borderColor = '#ff9800';
            } else if (this.value.length >= 6) {
                this.style.borderColor = '#2d7a3e';
            } else {
                this.style.borderColor = '#e0e0e0';
            }
        });
    }

    // Handle form submission - show loading state
    const loginForm = document.getElementById('loginForm');
    if (loginForm) {
        loginForm.addEventListener('submit', function (e) {
            const loginBtn = document.querySelector('.login-btn');
            if (loginBtn) {
                const originalContent = loginBtn.innerHTML;

                // Show loading state
                loginBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> <span>Đang xử lý...</span>';
                loginBtn.disabled = true;

                // Re-enable after 10 seconds (in case of error)
                setTimeout(function () {
                    loginBtn.innerHTML = originalContent;
                    loginBtn.disabled = false;
                }, 10000);
            }
        });
    }

    // Auto-hide alert messages after 5 seconds
    setTimeout(function () {
        const alerts = document.querySelectorAll('.alert-message.show');
        alerts.forEach(function (alert) {
            alert.style.transition = 'opacity 0.5s';
            alert.style.opacity = '0';
            setTimeout(function () {
                alert.style.display = 'none';
            }, 500);
        });
    }, 5000);
});