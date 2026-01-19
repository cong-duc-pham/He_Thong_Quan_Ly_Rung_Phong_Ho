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

    // AJAX Navigation - Ngăn reload trang
    function setupAjaxNavigation() {
        document.querySelectorAll('.menu-item a[href], .submenu a[href]').forEach(link => {
            const href = link.getAttribute('href');

            // Bỏ qua các link đặc biệt
            if (!href ||
                href === '#' ||
                href === 'javascript:void(0)' ||
                href.includes('Logout') ||
                link.closest('form') ||
                (link.closest('.has-submenu') && link.parentElement.classList.contains('has-submenu'))) {
                return;
            }

            link.addEventListener('click', function (e) {
                e.preventDefault();

                // Xóa active từ tất cả menu items
                document.querySelectorAll('.menu-item').forEach(item => {
                    item.classList.remove('active');
                });
                document.querySelectorAll('.submenu a').forEach(a => {
                    a.classList.remove('active');
                });

                // Thêm active vào item được click
                const menuItem = this.closest('.menu-item');
                if (menuItem) {
                    menuItem.classList.add('active');
                }

                if (this.closest('.submenu')) {
                    this.classList.add('active');
                    const submenuParent = this.closest('.has-submenu');
                    if (submenuParent) {
                        submenuParent.classList.add('open');
                    }
                }

                // Đóng sidebar trên mobile
                if (isMobile() && sidebar.classList.contains('active')) {
                    sidebar.classList.remove('active');
                }

                // Load content bằng AJAX
                loadPage(href);
            });
        });
    }

    // Hàm load trang bằng AJAX
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

        // Sử dụng fetch API
        fetch(url, {
            method: 'GET',
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.text();
            })
            .then(html => {
                // Parse HTML response
                const parser = new DOMParser();
                const doc = parser.parseFromString(html, 'text/html');

                // Lấy nội dung main
                const newMainContent = doc.querySelector('main[role="main"]');

                if (newMainContent) {
                    mainContent.innerHTML = newMainContent.innerHTML;

                    // Update URL trong browser
                    window.history.pushState({ url: url }, '', url);

                    // Update title
                    const newTitle = doc.querySelector('title');
                    if (newTitle) {
                        document.title = newTitle.textContent;
                    }

                    // CRITICAL: Re-initialize scripts cho trang mới
                    reinitializeScripts();
                } else {
                    mainContent.innerHTML = html;
                    reinitializeScripts();
                }
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

    // Re-initialize scripts sau khi load nội dung mới
    function reinitializeScripts() {
        // 1. Execute inline scripts from loaded content
        const scripts = mainContent.querySelectorAll('script');
        scripts.forEach(script => {
            if (script.textContent.trim()) {
                try {
                    // Create new script element to execute
                    const newScript = document.createElement('script');
                    newScript.textContent = script.textContent;
                    document.body.appendChild(newScript);
                    document.body.removeChild(newScript);
                } catch (e) {
                    console.error('Error executing inline script:', e);
                }
            }
        });

        // 2. Re-initialize LoRung cascade handlers
        if (typeof LoRung !== 'undefined') {
            const currentPath = window.location.pathname.toLowerCase();

            // Đợi một chút để DOM được render hoàn toàn
            setTimeout(() => {
                // Index page - Filter cascade
                if (currentPath.includes('/lorung/index') || currentPath.endsWith('/lorung')) {
                    if (LoRung.CascadeHandler && LoRung.CascadeHandler.initCascadeFilter) {
                        LoRung.CascadeHandler.initCascadeFilter();
                    }
                    if (LoRung.UIEnhancer && LoRung.UIEnhancer.highlightActiveFilters) {
                        LoRung.UIEnhancer.highlightActiveFilters();
                    }
                }

                // Create page
                if (currentPath.includes('/lorung/create')) {
                    if (LoRung.CascadeHandler && LoRung.CascadeHandler.initCascadeForm) {
                        LoRung.CascadeHandler.initCascadeForm();
                    }
                    if (LoRung.FormValidation) {
                        LoRung.FormValidation.validateTechnicalNumbers();
                        LoRung.FormValidation.validateArea();
                        LoRung.FormValidation.validateBeforeSubmit();
                    }
                }

                // Edit page
                if (currentPath.includes('/lorung/edit')) {
                    if (LoRung.CascadeHandler && LoRung.CascadeHandler.initCascadeForm) {
                        LoRung.CascadeHandler.initCascadeForm();
                    }
                    if (LoRung.FormValidation) {
                        LoRung.FormValidation.validateTechnicalNumbers();
                        LoRung.FormValidation.validateArea();
                        LoRung.FormValidation.validateBeforeSubmit();
                    }
                }

                // Initialize tooltips
                if (LoRung.UIEnhancer && LoRung.UIEnhancer.initTooltips) {
                    LoRung.UIEnhancer.initTooltips();
                }
            }, 100);
        }

        // 3. Re-initialize DiGioi handlers if exists
        if (typeof DiGioi !== 'undefined') {
            const currentPath = window.location.pathname.toLowerCase();

            setTimeout(() => {
                // DanhMucXa
                if (currentPath.includes('/danhmucxa')) {
                    if (DiGioi.XaHandler && DiGioi.XaHandler.init) {
                        DiGioi.XaHandler.init();
                    }
                }

                // DanhMucThon
                if (currentPath.includes('/danhmucthon')) {
                    if (DiGioi.ThonHandler && DiGioi.ThonHandler.init) {
                        DiGioi.ThonHandler.init();
                    }
                }
            }, 100);
        }

        // 4. Re-initialize Bootstrap components
        if (typeof bootstrap !== 'undefined') {
            // Tooltips
            const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
            tooltipTriggerList.map(function (tooltipTriggerEl) {
                return new bootstrap.Tooltip(tooltipTriggerEl);
            });

            // Popovers
            const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
            popoverTriggerList.map(function (popoverTriggerEl) {
                return new bootstrap.Popover(popoverTriggerEl);
            });
        }

        // 5. Re-bind form submissions if needed
        const forms = mainContent.querySelectorAll('form');
        forms.forEach(form => {
            // Add any form-specific initialization here
        });

        // 6. Dispatch custom event for other modules
        document.dispatchEvent(new CustomEvent('contentLoaded', {
            detail: { url: window.location.href }
        }));

        console.log('✅ Scripts re-initialized successfully');
    }

    // Xử lý nút back/forward của browser
    window.addEventListener('popstate', function (e) {
        if (e.state && e.state.url) {
            loadPage(e.state.url);
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

    // Đánh dấu menu active dựa trên URL hiện tại
    const currentPath = window.location.pathname;
    document.querySelectorAll('.menu-item a, .submenu a').forEach(link => {
        const href = link.getAttribute('href');
        if (href && currentPath.includes(href) && href !== '#' && href !== 'javascript:void(0)') {
            const menuItem = link.closest('.menu-item');
            if (menuItem) {
                menuItem.classList.add('active');
            }

            const submenuParent = link.closest('.has-submenu');
            if (submenuParent) {
                submenuParent.classList.add('open');
            }

            if (link.closest('.submenu')) {
                link.classList.add('active');
            }
        }
    });

    // Initialize AJAX navigation
    setupAjaxNavigation();

    // Save initial state
    window.history.replaceState({ url: window.location.href }, '', window.location.href);

    console.log('✅ Main.js initialized');
});