document.addEventListener('DOMContentLoaded', function () {
    const menuToggle = document.getElementById('menuToggle');
    const sidebar = document.getElementById('sidebar');
    const mainContent = document.querySelector('main[role="main"]');

    function isMobile() {
        return window.innerWidth <= 992;
    }

    // Toggle menu và sidebar
    if (menuToggle && sidebar) {
        menuToggle.addEventListener('click', function (e) {
            e.stopPropagation();

            if (isMobile()) {
                sidebar.classList.toggle('active');
            } else {
                sidebar.classList.toggle('collapsed');

                if (sidebar.classList.contains('collapsed')) {
                    document.querySelectorAll('.has-submenu.open').forEach(item => {
                        item.classList.remove('open');
                    });
                }
            }
        });
    }

    // Xử lý click submenu
    document.querySelectorAll('.has-submenu > a').forEach(item => {
        item.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();

            const parent = this.parentElement;

            if (!isMobile() && sidebar.classList.contains('collapsed')) {
                sidebar.classList.remove('collapsed');
                setTimeout(() => {
                    parent.classList.toggle('open');
                }, 300);
                return;
            }

            parent.classList.toggle('open');
        });
    });

    // AJAX navigation - bắt click link và load AJAX thay vì reload
    document.body.addEventListener('click', function (e) {
        const link = e.target.closest('a');

        if (!link || !link.getAttribute('href')) return;

        const href = link.getAttribute('href');

        // Bỏ qua các link đặc biệt
        if (href === '#' ||
            href.startsWith('javascript:') ||
            href.startsWith('mailto:') ||
            href.startsWith('tel:') ||
            link.getAttribute('target') === '_blank' ||
            link.hasAttribute('download') ||
            link.classList.contains('no-ajax')) {
            return;
        }

        // Kiểm tra link nội bộ
        const targetUrl = new URL(link.href, window.location.origin);
        if (targetUrl.origin !== window.location.origin) return;

        if (link.closest('.sidebar')) {
            handleSidebarActiveState(link);
        }

        e.preventDefault();
        loadPage(href);
    });

    function handleSidebarActiveState(link) {
        document.querySelectorAll('.menu-item').forEach(item => item.classList.remove('active'));
        document.querySelectorAll('.submenu a').forEach(a => a.classList.remove('active'));

        const menuItem = link.closest('.menu-item');
        if (menuItem) menuItem.classList.add('active');

        if (link.closest('.submenu')) {
            link.classList.add('active');
            const submenuParent = link.closest('.has-submenu');
            if (submenuParent) submenuParent.classList.add('open');
        }

        if (isMobile() && sidebar.classList.contains('active')) {
            sidebar.classList.remove('active');
        }
    }

    // Setup AJAX form submission
    function setupAjaxFormSubmission() {
        const forms = mainContent.querySelectorAll('form:not(.no-ajax)');

        forms.forEach(form => {
            form.removeEventListener('submit', handleFormSubmit);
            form.addEventListener('submit', handleFormSubmit);
        });
    }

    async function handleFormSubmit(e) {
        e.preventDefault();
        const form = e.target;
        const method = (form.method || 'GET').toUpperCase();
        const action = form.action || window.location.href;

        // Kiểm tra validation
        if (typeof $ !== 'undefined' && $(form).valid && !$(form).valid()) {
            return;
        }

        const submitBtn = form.querySelector('button[type="submit"]');
        let originalBtnContent = '';
        if (submitBtn) {
            originalBtnContent = submitBtn.innerHTML;
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Đang xử lý...';
        }

        try {
            const formData = new FormData(form);
            let requestUrl = action;
            let fetchOptions = {
                method,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            };

            if (method === 'GET') {
                const params = new URLSearchParams(formData);
                const qs = params.toString();
                if (qs) {
                    requestUrl += (requestUrl.includes('?') ? '&' : '?') + qs;
                }
            } else {
                fetchOptions.body = formData;
            }

            const response = await fetch(requestUrl, fetchOptions);

            if (!response.ok) throw new Error('Không thể kết nối tới server');

            const responseURL = response.url;
            const html = await response.text();

            processResponseHtml(html, responseURL);

        } catch (error) {
            console.error('Lỗi:', error);
            if (typeof DiGioi !== 'undefined' && DiGioi.Utils) {
                DiGioi.Utils.showError('Có lỗi xảy ra khi gửi dữ liệu.');
            } else {
                alert('Có lỗi xảy ra!');
            }
        } finally {
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.innerHTML = originalBtnContent;
            }
        }
    }

    // Load trang AJAX
    function loadPage(url) {
        mainContent.innerHTML = `
            <div class="text-center mt-5">
                <div class="spinner-border text-primary" role="status" style="width: 3rem; height: 3rem;">
                    <span class="visually-hidden">Đang tải...</span>
                </div>
                <p class="mt-3">Đang tải...</p>
            </div>
        `;

        fetch(url, {
            method: 'GET',
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        })
            .then(response => {
                if (!response.ok) throw new Error('Không thể tải trang');
                return response.text();
            })
            .then(html => {
                processResponseHtml(html, url);
            })
            .catch(error => {
                console.error('Lỗi tải trang:', error);
                mainContent.innerHTML = `
                <div class="alert alert-danger mt-5" role="alert">
                    <h4 class="alert-heading">Lỗi!</h4>
                    <p>Không thể tải nội dung. Vui lòng thử lại.</p>
                    <hr>
                    <button class="btn btn-primary" onclick="location.reload()">
                        Tải lại trang
                    </button>
                </div>
            `;
            });
    }

    // Xử lý HTML trả về từ server
    function processResponseHtml(html, url) {
        const parser = new DOMParser();
        const doc = parser.parseFromString(html, 'text/html');
        const newMainContent = doc.querySelector('main[role="main"]');

        if (newMainContent) {
            mainContent.innerHTML = newMainContent.innerHTML;

            if (window.location.href !== url) {
                window.history.pushState({ url: url }, '', url);
            }

            const newTitle = doc.querySelector('title');
            if (newTitle) document.title = newTitle.textContent;

            reinitializeScripts();
        } else {
            mainContent.innerHTML = html;
            reinitializeScripts();
        }
    }

    // Re-init scripts sau khi load nội dung mới (AJAX)
    function reinitializeScripts() {
        setupAjaxFormSubmission();

        // Chạy inline scripts có trong nội dung mới
        const scripts = mainContent.querySelectorAll('script');
        scripts.forEach(script => {
            if (script.textContent.trim()) {
                try {
                    const newScript = document.createElement('script');
                    newScript.textContent = script.textContent;
                    document.body.appendChild(newScript);
                    document.body.removeChild(newScript);
                } catch (e) {
                    console.error('Lỗi:', e);
                }
            }
        });

        // Re-init BaoCaoThongKe nếu đang ở trang báo cáo thống kê
        const currentPath = window.location.pathname.toLowerCase();
        if (currentPath.includes('/baocaothongke')) {
            if (typeof BaoCaoThongKe !== 'undefined' && BaoCaoThongKe.init) {
                BaoCaoThongKe.init();
            }
        }

        // Re-init NhanSu search nếu đang ở trang quản lý nhân sự
        if (typeof window.NhanSuSearchInit === 'function') {
            const currentPath = window.location.pathname.toLowerCase();
            if (currentPath.includes('/nhansu')) {
                window.NhanSuSearchInit();
            }
        }

        if (typeof LoRung !== 'undefined') {
            const currentPath = window.location.pathname.toLowerCase();
            setTimeout(() => {
                if (currentPath.includes('/lorung/index') || currentPath.endsWith('/lorung')) {
                    if (LoRung.AjaxHandler?.init) LoRung.AjaxHandler.init();
                    if (LoRung.CascadeHandler?.initCascadeFilter) LoRung.CascadeHandler.initCascadeFilter();
                    if (LoRung.UIEnhancer?.highlightActiveFilters) LoRung.UIEnhancer.highlightActiveFilters();
                }
                if (currentPath.includes('/lorung/create') || currentPath.includes('/lorung/edit')) {
                    if (LoRung.CascadeHandler?.initCascadeForm) LoRung.CascadeHandler.initCascadeForm();
                    if (LoRung.FormValidation) {
                        LoRung.FormValidation.validateTechnicalNumbers();
                        LoRung.FormValidation.validateArea();
                        LoRung.FormValidation.validateBeforeSubmit();
                    }
                }
                if (LoRung.UIEnhancer?.initTooltips) LoRung.UIEnhancer.initTooltips();
            }, 100);
        }

        if (typeof DiGioi !== 'undefined') {
            const currentPath = window.location.pathname.toLowerCase();
            setTimeout(() => {
                if (currentPath.includes('/danhmucxa') && DiGioi.XaHandler?.init) DiGioi.XaHandler.init();
                if (currentPath.includes('/danhmucthon') && DiGioi.ThonHandler?.init) DiGioi.ThonHandler.init();

                if (DiGioi.FormHandler?.initValidation) DiGioi.FormHandler.initValidation();
            }, 100);
        }

        if (typeof bootstrap !== 'undefined') {
            [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]')).map(el => new bootstrap.Tooltip(el));
            [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]')).map(el => new bootstrap.Popover(el));
        }

        if (typeof $ !== 'undefined') {
            $('.btn-delete, .delete-button').off('click').on('click', function (e) {
                if (!confirm('Bạn có chắc chắn muốn xóa không?')) {
                    e.preventDefault();
                    e.stopPropagation();
                    return false;
                }
            });
        }
    }

    // Xử lý back/forward button
    window.addEventListener('popstate', function (e) {
        if (e.state && e.state.url) {
            loadPage(e.state.url);
        } else {
            loadPage(window.location.href);
        }
    });

    // Đóng sidebar khi click ngoài (mobile)
    document.addEventListener('click', function (e) {
        if (isMobile() && sidebar.classList.contains('active')) {
            if (!sidebar.contains(e.target) && !menuToggle.contains(e.target)) {
                sidebar.classList.remove('active');
            }
        }
    });

    // Xử lý resize window
    let resizeTimer;
    window.addEventListener('resize', function () {
        clearTimeout(resizeTimer);
        resizeTimer = setTimeout(function () {
            if (!isMobile()) {
                sidebar.classList.remove('active');
            } else {
                sidebar.classList.remove('collapsed');
            }
        }, 250);
    });

    window.history.replaceState({ url: window.location.href }, '', window.location.href);
    reinitializeScripts();

    const currentPath = window.location.pathname;
    document.querySelectorAll('.menu-item a, .submenu a').forEach(link => {
        if (link.getAttribute('href') && currentPath.includes(link.getAttribute('href'))) {
            handleSidebarActiveState(link);
        }
    });
});