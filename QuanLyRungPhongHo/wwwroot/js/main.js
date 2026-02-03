    document.addEventListener('DOMContentLoaded', function () {
    const menuToggle = document.getElementById('menuToggle');
    const sidebar = document.getElementById('sidebar');
    const mainContent = document.querySelector('main[role="main"]');

    function isMobile() {
        return window.innerWidth <= 992;
    }

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

    // Xử lý submenu toggle
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

    // ==========================================
    // 1. AJAX NAVIGATION (GLOBAL DELEGATION)
    // Sửa lỗi: Dùng Event Delegation để bắt sự kiện click
    // cho cả Sidebar lẫn nội dung bên trong Main
    // ==========================================
    document.body.addEventListener('click', function (e) {
        // Tìm thẻ a gần nhất từ vị trí click
        const link = e.target.closest('a');

        // Nếu không phải link hoặc không có href, bỏ qua
        if (!link || !link.getAttribute('href')) return;

        const href = link.getAttribute('href');

        // Bỏ qua các link đặc biệt, link ngoài, hoặc download
        if (href === '#' ||
            href.startsWith('javascript:') ||
            href.startsWith('mailto:') ||
            href.startsWith('tel:') ||
            link.getAttribute('target') === '_blank' ||
            link.hasAttribute('download') ||
            link.classList.contains('no-ajax')) { // Thêm class no-ajax nếu muốn link hoạt động thường
            return;
        }

        // Kiểm tra xem link có thuộc về ứng dụng nội bộ không (để tránh load link Google, v.v...)
        // Ở đây giả sử ứng dụng chạy trên root '/', nếu khác bạn cần check domain
        const targetUrl = new URL(link.href, window.location.origin);
        if (targetUrl.origin !== window.location.origin) return;

        // Xử lý UI cho Sidebar (Active state)
        if (link.closest('.sidebar')) {
            handleSidebarActiveState(link);
        }

        // CHẶN RELOAD VÀ GỌI AJAX
        e.preventDefault();
        loadPage(href);
    });

    // Hàm xử lý active state cho sidebar (tách riêng để gọn code)
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

    // ==========================================
    // 2. FORM SUBMISSION HANDLER
    // Hàm xử lý submit form bằng AJAX thay vì reload
    // ==========================================
    function setupAjaxFormSubmission() {
        const forms = mainContent.querySelectorAll('form:not(.no-ajax)');

        forms.forEach(form => {
            // Xóa listener cũ để tránh duplicate (nếu có)
            form.removeEventListener('submit', handleFormSubmit);
            form.addEventListener('submit', handleFormSubmit);
        });
    }

    async function handleFormSubmit(e) {
        e.preventDefault();
        const form = e.target;

        // Validate form nếu có jQuery Validate (từ digioi.js)
        if (typeof $ !== 'undefined' && $(form).valid && !$(form).valid()) {
            return; // Dừng nếu form không hợp lệ
        }

        // Hiển thị loading button
        const submitBtn = form.querySelector('button[type="submit"]');
        let originalBtnContent = '';
        if (submitBtn) {
            originalBtnContent = submitBtn.innerHTML;
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Đang xử lý...';
        }

        try {
            const formData = new FormData(form);
            const response = await fetch(form.action, {
                method: form.method || 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (!response.ok) throw new Error('Network response was not ok');

            // Lấy URL đích (trong trường hợp redirect) hoặc URL hiện tại
            const responseURL = response.url;
            const html = await response.text();

            processResponseHtml(html, responseURL);

        } catch (error) {
            console.error('Error submitting form:', error);
            if (typeof DiGioi !== 'undefined' && DiGioi.Utils) {
                DiGioi.Utils.showError('Có lỗi xảy ra khi gửi dữ liệu.');
            } else {
                alert('Có lỗi xảy ra!');
            }
        } finally {
            // Reset button
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.innerHTML = originalBtnContent;
            }
        }
    }


    // Hàm load trang bằng AJAX (Core function)
    function loadPage(url) {
        // Hiển thị loading indicator
        mainContent.innerHTML = `
            <div class="text-center mt-5">
                <div class="spinner-border text-primary" role="status" style="width: 3rem; height: 3rem;">
                    <span class="visually-hidden">Loading...</span>
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
                if (!response.ok) throw new Error('Network response was not ok');
                return response.text();
            })
            .then(html => {
                processResponseHtml(html, url);
            })
            .catch(error => {
                console.error('Error loading page:', error);
                mainContent.innerHTML = `
                <div class="alert alert-danger mt-5" role="alert">
                    <h4 class="alert-heading">Lỗi tải trang!</h4>
                    <p>Không thể tải nội dung. Vui lòng thử lại.</p>
                    <hr>
                    <button class="btn btn-primary" onclick="location.reload()">
                        <i class="fas fa-sync-alt"></i> Tải lại trang
                    </button>
                </div>
            `;
            });
    }

    // Hàm xử lý HTML trả về và cập nhật giao diện
    function processResponseHtml(html, url) {
        const parser = new DOMParser();
        const doc = parser.parseFromString(html, 'text/html');
        const newMainContent = doc.querySelector('main[role="main"]');

        if (newMainContent) {
            mainContent.innerHTML = newMainContent.innerHTML;

            // Chỉ pushState nếu URL khác URL hiện tại (tránh duplicate khi submit form)
            if (window.location.href !== url) {
                window.history.pushState({ url: url }, '', url);
            }

            const newTitle = doc.querySelector('title');
            if (newTitle) document.title = newTitle.textContent;

            reinitializeScripts();
        } else {
            // Trường hợp server trả về partial view hoặc json lỗi
            mainContent.innerHTML = html;
            reinitializeScripts();
        }
    }

    // Re-initialize scripts sau khi load nội dung mới
    function reinitializeScripts() {
        // 1. Kích hoạt xử lý Form AJAX cho các form mới load
        setupAjaxFormSubmission();

        // 2. Execute inline scripts
        const scripts = mainContent.querySelectorAll('script');
        scripts.forEach(script => {
            if (script.textContent.trim()) {
                try {
                    const newScript = document.createElement('script');
                    newScript.textContent = script.textContent;
                    document.body.appendChild(newScript);
                    document.body.removeChild(newScript);
                } catch (e) {
                    console.error('Error executing inline script:', e);
                }
            }
        });

        // 3. Re-initialize NhanSu module
        if (typeof window.NhanSuSearchInit === 'function') {
            const currentPath = window.location.pathname.toLowerCase();
            if (currentPath.includes('/nhansu')) {
                setTimeout(() => {
                    window.NhanSuSearchInit();
                    console.log('✅ NhanSu module re-initialized');
                }, 100);
            }
        }

        // 4. Re-initialize LoRung modules
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

        // 5. Re-initialize DiGioi modules
        if (typeof DiGioi !== 'undefined') {
            const currentPath = window.location.pathname.toLowerCase();
            setTimeout(() => {
                if (currentPath.includes('/danhmucxa') && DiGioi.XaHandler?.init) DiGioi.XaHandler.init();
                if (currentPath.includes('/danhmucthon') && DiGioi.ThonHandler?.init) DiGioi.ThonHandler.init();

                if (DiGioi.FormHandler?.initValidation) DiGioi.FormHandler.initValidation();
            }, 100);
        }

        // 6. Re-initialize Bootstrap components
        if (typeof bootstrap !== 'undefined') {
            [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]')).map(el => new bootstrap.Tooltip(el));
            [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]')).map(el => new bootstrap.Popover(el));
        }

        // 7. Bind Confirm Delete events
        $('.btn-delete, .delete-button').off('click').on('click', function (e) {
            if (!confirm('Bạn có chắc chắn muốn xóa không?')) {
                e.preventDefault();
                e.stopPropagation();
                return false;
            }
        });

        console.log('✅ Scripts re-initialized successfully');
    }

    // Xử lý nút back/forward của browser
    window.addEventListener('popstate', function (e) {
        if (e.state && e.state.url) {
            loadPage(e.state.url);
        } else {
            // Fallback nếu không có state (lần đầu vào trang)
            loadPage(window.location.href);
        }
    });

    // Click outside sidebar trên mobile
    document.addEventListener('click', function (e) {
        if (isMobile() && sidebar.classList.contains('active')) {
            if (!sidebar.contains(e.target) && !menuToggle.contains(e.target)) {
                sidebar.classList.remove('active');
            }
        }
    });

    // Xử lý resize
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

    // Save initial state
    window.history.replaceState({ url: window.location.href }, '', window.location.href);

    // Initial Scripts setup for the first load
    reinitializeScripts();

    // Highlight initial active menu
    const currentPath = window.location.pathname;
    document.querySelectorAll('.menu-item a, .submenu a').forEach(link => {
        if (link.getAttribute('href') && currentPath.includes(link.getAttribute('href'))) {
            handleSidebarActiveState(link);
        }
    });

    console.log('✅ Main.js initialized with Global AJAX Handling');
});