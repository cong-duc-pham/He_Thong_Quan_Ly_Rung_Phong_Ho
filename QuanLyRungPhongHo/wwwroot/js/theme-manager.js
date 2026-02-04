/**
 * Theme Manager - Quản lý theme cho ứng dụng
 * Hỗ trợ 3 chế độ: light, dark, auto
 */

class ThemeManager {
    constructor() {
        this.storageKey = 'qlrph.theme';
        this.themes = ['light', 'dark', 'auto'];
        this.currentTheme = this.loadTheme();
        this.initializeTheme();
        this.setupListeners();
    }

    /**
     * Tải theme từ localStorage hoặc sử dụng mặc định
     */
    loadTheme() {
        try {
            const saved = localStorage.getItem(this.storageKey);
            return saved && this.themes.includes(saved) ? saved : 'light';
        } catch (e) {
            console.warn('Không thể tải theme từ localStorage:', e);
            return 'light';
        }
    }

    /**
     * Lưu theme vào localStorage
     */
    saveTheme(theme) {
        try {
            localStorage.setItem(this.storageKey, theme);
        } catch (e) {
            console.warn('Không thể lưu theme vào localStorage:', e);
        }
    }

    /**
     * Kiểm tra xem hệ thống có đang dùng dark mode không
     */
    isSystemDark() {
        return window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
    }

    /**
     * Xác định theme thực tế cần áp dụng
     */
    getEffectiveTheme() {
        if (this.currentTheme === 'auto') {
            return this.isSystemDark() ? 'dark' : 'light';
        }
        return this.currentTheme;
    }

    /**
     * Áp dụng theme lên DOM
     */
    applyTheme(theme = null) {
        const effectiveTheme = theme ? theme : this.getEffectiveTheme();
        const root = document.documentElement;
        const body = document.body;

        // Xóa các class cũ
        root.classList.remove('dark', 'light');
        body.classList.remove('theme-dark', 'theme-light');

        // Thêm class mới
        if (effectiveTheme === 'dark') {
            root.classList.add('dark');
            body.classList.add('theme-dark');
        } else {
            root.classList.add('light');
            body.classList.add('theme-light');
        }

        // Cập nhật UI toggle button nếu có
        this.updateToggleButton();
    }

    /**
     * Khởi tạo theme ban đầu
     */
    initializeTheme() {
        this.applyTheme();
    }

    /**
     * Thiết lập event listeners
     */
    setupListeners() {
        // Lắng nghe thay đổi theme hệ thống (chỉ khi ở chế độ auto)
        if (window.matchMedia) {
            const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
            
            // Modern browsers
            if (mediaQuery.addEventListener) {
                mediaQuery.addEventListener('change', (e) => {
                    if (this.currentTheme === 'auto') {
                        this.applyTheme();
                    }
                });
            } 
            // Fallback cho browsers cũ
            else if (mediaQuery.addListener) {
                mediaQuery.addListener((e) => {
                    if (this.currentTheme === 'auto') {
                        this.applyTheme();
                    }
                });
            }
        }
    }

    /**
     * Chuyển đổi theme
     */
    setTheme(theme) {
        if (!this.themes.includes(theme)) {
            console.warn('Theme không hợp lệ:', theme);
            return;
        }

        this.currentTheme = theme;
        this.saveTheme(theme);
        this.applyTheme();

        // Dispatch custom event để các component khác có thể lắng nghe
        window.dispatchEvent(new CustomEvent('themechange', { 
            detail: { theme: theme, effectiveTheme: this.getEffectiveTheme() } 
        }));
    }

    /**
     * Chuyển đổi sang theme tiếp theo
     */
    cycleTheme() {
        const currentIndex = this.themes.indexOf(this.currentTheme);
        const nextIndex = (currentIndex + 1) % this.themes.length;
        this.setTheme(this.themes[nextIndex]);
    }

    /**
     * Toggle giữa light và dark (bỏ qua auto)
     */
    toggleTheme() {
        const effectiveTheme = this.getEffectiveTheme();
        const newTheme = effectiveTheme === 'dark' ? 'light' : 'dark';
        this.setTheme(newTheme);
    }

    /**
     * Lấy theme hiện tại
     */
    getTheme() {
        return this.currentTheme;
    }

    /**
     * Lấy theme thực tế đang hiển thị
     */
    getCurrentEffectiveTheme() {
        return this.getEffectiveTheme();
    }

    /**
     * Cập nhật UI của toggle button
     */
    updateToggleButton() {
        const toggleBtn = document.getElementById('themeToggle');
        if (!toggleBtn) return;

        const effectiveTheme = this.getEffectiveTheme();
        const iconElement = toggleBtn.querySelector('i');
        const textElement = toggleBtn.querySelector('.theme-text');

        if (iconElement) {
            iconElement.className = ''; // Clear classes
            if (this.currentTheme === 'auto') {
                iconElement.className = 'fas fa-circle-half-stroke';
            } else if (effectiveTheme === 'dark') {
                iconElement.className = 'fas fa-moon';
            } else {
                iconElement.className = 'fas fa-sun';
            }
        }

        if (textElement) {
            const labels = {
                'light': 'Sáng',
                'dark': 'Tối',
                'auto': 'Auto'
            };
            textElement.textContent = labels[this.currentTheme] || this.currentTheme;
        }

        // Cập nhật title
        const titles = {
            'light': 'Chế độ sáng',
            'dark': 'Chế độ tối',
            'auto': 'Tự động theo hệ thống'
        };
        toggleBtn.title = titles[this.currentTheme] || 'Đổi theme';
    }
}

// Khởi tạo theme manager ngay lập tức
const themeManager = new ThemeManager();

// Export để có thể sử dụng ở nơi khác
if (typeof window !== 'undefined') {
    window.themeManager = themeManager;
}
