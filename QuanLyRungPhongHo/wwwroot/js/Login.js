// Toggle Password Visibility
document.addEventListener('DOMContentLoaded', function () {
    const togglePassword = document.getElementById('togglePassword');
    const passwordInput = document.getElementById('password');
    const usernameInput = document.getElementById('username');
    const rememberMeCheckbox = document.getElementById('rememberMe');
    const loginForm = document.getElementById('loginForm');

    console.log('DOM Loaded - Checkbox:', rememberMeCheckbox);

    // Load saved credentials on page load
    if (rememberMeCheckbox) {
        loadSavedCredentials();

        // Force enable click on checkbox - workaround for any blocking element
        const wrapper = rememberMeCheckbox.closest('.remember-checkbox-wrapper');
        if (wrapper) {
            wrapper.addEventListener('click', function(e) {
                // If clicking on wrapper but not directly on checkbox, toggle it
                if (e.target !== rememberMeCheckbox) {
                    e.preventDefault();
                    rememberMeCheckbox.checked = !rememberMeCheckbox.checked;
                    // Manually trigger change event
                    const event = new Event('change', { bubbles: true });
                    rememberMeCheckbox.dispatchEvent(event);
                    console.log('Manual toggle - Checkbox now:', rememberMeCheckbox.checked);
                }
            });
        }
    }

    if (togglePassword && passwordInput) {
        togglePassword.addEventListener('click', function () {
            const type = passwordInput.getAttribute('type') === 'password' ? 'text' : 'password';
            passwordInput.setAttribute('type', type);
            this.classList.toggle('fa-eye-slash');
            this.classList.toggle('fa-eye');
        });
    }

    // Input validation styling
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

        if (!usernameInput.value) {
            usernameInput.focus();
        }
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

    // Handle Remember Me checkbox - SIMPLIFIED
    if (rememberMeCheckbox) {
        rememberMeCheckbox.addEventListener('change', function () {
            console.log('Checkbox state changed to:', this.checked);
            if (!this.checked) {
                clearSavedCredentials();
            }
        });
    }

    // Handle form submission
    if (loginForm) {
        loginForm.addEventListener('submit', function (e) {
            const loginBtn = document.querySelector('.login-btn');
            if (loginBtn) {
                const originalContent = loginBtn.innerHTML;
                loginBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> <span>Đang xử lý...</span>';
                loginBtn.disabled = true;

                setTimeout(function () {
                    loginBtn.innerHTML = originalContent;
                    loginBtn.disabled = false;
                }, 10000);
            }

            // Save or clear credentials
            if (rememberMeCheckbox && rememberMeCheckbox.checked) {
                saveCredentials();
            } else {
                clearSavedCredentials();
            }
        });
    }

    // Auto-hide alert messages
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

    // Credential management functions
    function saveCredentials() {
        const username = usernameInput ? usernameInput.value : '';
        const password = passwordInput ? passwordInput.value : '';

        if (username && password) {
            try {
                localStorage.setItem('saved_username', btoa(username));
                localStorage.setItem('saved_password', btoa(password));
                localStorage.setItem('remember_login', 'true');
                console.log('✓ Credentials saved');
            } catch (e) {
                console.error('Error saving credentials:', e);
            }
        }
    }

    function loadSavedCredentials() {
        try {
            const rememberLogin = localStorage.getItem('remember_login');

            if (rememberLogin === 'true') {
                const savedUsername = localStorage.getItem('saved_username');
                const savedPassword = localStorage.getItem('saved_password');

                if (savedUsername && savedPassword && usernameInput && passwordInput) {
                    usernameInput.value = atob(savedUsername);
                    passwordInput.value = atob(savedPassword);

                    if (rememberMeCheckbox) {
                        rememberMeCheckbox.checked = true;
                    }

                    if (usernameInput.value.length >= 3) {
                        usernameInput.style.borderColor = '#2d7a3e';
                    }
                    if (passwordInput.value.length >= 6) {
                        passwordInput.style.borderColor = '#2d7a3e';
                    }
                    
                    console.log('✓ Credentials loaded');
                }
            }
        } catch (e) {
            console.error('Error loading credentials:', e);
            clearSavedCredentials();
        }
    }

    function clearSavedCredentials() {
        try {
            localStorage.removeItem('saved_username');
            localStorage.removeItem('saved_password');
            localStorage.removeItem('remember_login');
            console.log('✓ Credentials cleared');
        } catch (e) {
            console.error('Error clearing credentials:', e);
        }
    }
});