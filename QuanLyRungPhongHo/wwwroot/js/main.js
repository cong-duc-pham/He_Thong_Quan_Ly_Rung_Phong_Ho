document.addEventListener('DOMContentLoaded', function () {
    const menuToggle = document.getElementById('menuToggle');
    const sidebar = document.getElementById('sidebar');

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

    document.addEventListener('click', function (e) {
        if (isMobile() && sidebar.classList.contains('active')) {
            if (!sidebar.contains(e.target) && !menuToggle.contains(e.target)) {
                sidebar.classList.remove('active');
            }
        }
    });

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

    const currentPath = window.location.pathname;
    document.querySelectorAll('.menu-item a, .submenu a').forEach(link => {
        const href = link.getAttribute('href');
        if (href && currentPath.includes(href) && href !== '#') {
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
});