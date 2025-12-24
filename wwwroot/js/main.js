document.addEventListener('DOMContentLoaded', function () {
    const menuToggle = document.getElementById('menuToggle');
    const sidebar = document.getElementById('sidebar');

    // Kiểm tra chế độ mobile
    function isMobile() {
        return window.innerWidth <= 992;
    }

    // Toggle sidebar - desktop: collapsed, mobile: active
    if (menuToggle && sidebar) {
        menuToggle.addEventListener('click', function (e) {
            e.stopPropagation(); // Ngăn event bubbling

            if (isMobile()) {
                sidebar.classList.toggle('active');
            } else {
                sidebar.classList.toggle('collapsed');

                // Đóng tất cả submenu khi thu gọn sidebar
                if (sidebar.classList.contains('collapsed')) {
                    document.querySelectorAll('.has-submenu.open').forEach(item => {
                        item.classList.remove('open');
                    });
                }
            }
        });
    }

    // Xử lý submenu
    document.querySelectorAll('.has-submenu > a').forEach(item => {
        item.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();

            const parent = this.parentElement;

            // QUAN TRỌNG: Không cho mở submenu nếu sidebar đang collapsed
            if (!isMobile() && sidebar.classList.contains('collapsed')) {
                // Có thể mở rộng sidebar trước khi mở submenu
                sidebar.classList.remove('collapsed');
                // Sau đó mới mở submenu
                setTimeout(() => {
                    parent.classList.toggle('open');
                }, 300); // Đợi animation sidebar xong
                return;
            }

            // Toggle submenu bình thường
            parent.classList.toggle('open');
        });
    });

    // Đóng sidebar khi click bên ngoài (mobile)
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

    // Highlight menu hiện tại
    const currentPath = window.location.pathname;
    document.querySelectorAll('.menu-item a, .submenu a').forEach(link => {
        const href = link.getAttribute('href');
        if (href && currentPath.includes(href) && href !== '#') {
            // Đánh dấu menu item
            const menuItem = link.closest('.menu-item');
            if (menuItem) {
                menuItem.classList.add('active');
            }

            // Nếu trong submenu, mở parent
            const submenuParent = link.closest('.has-submenu');
            if (submenuParent) {
                submenuParent.classList.add('open');
            }

            // Đánh dấu submenu link
            if (link.closest('.submenu')) {
                link.classList.add('active');
            }
        }
    });
});