// ===== SETTINGS PAGE JAVASCRIPT =====
// Enhanced with smooth animations and better UX

document.addEventListener('DOMContentLoaded', function() {
    // Initialize settings from localStorage
    initializeSettings();
    
    // Setup event listeners
    setupEventListeners();
    
    // Initialize tooltips (if Bootstrap tooltips are available)
    initializeTooltips();
});

// ===== SETTINGS KEYS =====
const SETTINGS_KEYS = {
    THEME: 'user-settings-theme',
    DENSITY: 'user-settings-density',
    COMPACT_SIDEBAR: 'user-settings-compact-sidebar',
    AUTO_REFRESH: 'user-settings-auto-refresh',
    HIGHLIGHT_ALERT: 'user-settings-highlight-alert',
    NOTIFY_EMAIL: 'user-settings-notify-email',
    NOTIFY_SMS: 'user-settings-notify-sms',
    NOTIFY_INAPP: 'user-settings-notify-inapp',
    CONTACT_EMAIL: 'user-settings-contact-email',
    CONTACT_PHONE: 'user-settings-contact-phone',
    CONTACT_NOTE: 'user-settings-contact-note'
};

const LAYOUT_STATE_KEY = 'qlrph.settings';

function readLayoutState() {
    try {
        const raw = localStorage.getItem(LAYOUT_STATE_KEY);
        if (raw) return JSON.parse(raw);
    } catch (e) {
        /* ignore parse errors */
    }
    return {};
}

function persistLayoutState(patch) {
    const current = readLayoutState();
    const next = { ...current, ...patch };
    try {
        localStorage.setItem(LAYOUT_STATE_KEY, JSON.stringify(next));
    } catch (e) {
        /* ignore storage errors */
    }
}

// ===== INITIALIZE SETTINGS =====
function initializeSettings() {
    const layoutState = readLayoutState();

    // Theme
    const theme = layoutState.theme || localStorage.getItem(SETTINGS_KEYS.THEME) || 'light';
    document.getElementById('setting-theme').value = theme;
    applyTheme(theme);
    persistLayoutState({ theme });
    
    // Density
    const density = layoutState.density || localStorage.getItem(SETTINGS_KEYS.DENSITY) || 'comfortable';
    document.getElementById('setting-density').value = density;
    applyDensity(density);
    persistLayoutState({ density });
    
    // Compact Sidebar
    const compactSidebar = layoutState.compactSidebar ?? (localStorage.getItem(SETTINGS_KEYS.COMPACT_SIDEBAR) === 'true');
    document.getElementById('setting-compact-sidebar').checked = compactSidebar;
    if (compactSidebar) {
        document.body.classList.add('compact-sidebar');
    }
    persistLayoutState({ compactSidebar });
    
    // Auto Refresh
    const autoRefresh = localStorage.getItem(SETTINGS_KEYS.AUTO_REFRESH) === 'true';
    document.getElementById('setting-auto-refresh').checked = autoRefresh;
    
    // Highlight Alert
    const highlightAlert = localStorage.getItem(SETTINGS_KEYS.HIGHLIGHT_ALERT) === 'true';
    document.getElementById('setting-highlight-alert').checked = highlightAlert;
    if (highlightAlert) {
        document.body.classList.add('highlight-alert');
    }
    persistLayoutState({ highlightAlert });
    
    // Notifications
    const notifyEmail = localStorage.getItem(SETTINGS_KEYS.NOTIFY_EMAIL) !== 'false';
    document.getElementById('setting-notify-email').checked = notifyEmail;
    
    const notifySms = localStorage.getItem(SETTINGS_KEYS.NOTIFY_SMS) === 'true';
    document.getElementById('setting-notify-sms').checked = notifySms;
    
    const notifyInapp = localStorage.getItem(SETTINGS_KEYS.NOTIFY_INAPP) !== 'false';
    document.getElementById('setting-notify-inapp').checked = notifyInapp;
    
    // Contact Information
    const contactEmail = localStorage.getItem(SETTINGS_KEYS.CONTACT_EMAIL) || '';
    document.getElementById('setting-contact-email').value = contactEmail;
    
    const contactPhone = localStorage.getItem(SETTINGS_KEYS.CONTACT_PHONE) || '';
    document.getElementById('setting-contact-phone').value = contactPhone;
    
    const contactNote = localStorage.getItem(SETTINGS_KEYS.CONTACT_NOTE) || '';
    document.getElementById('setting-contact-note').value = contactNote;
}

// ===== SETUP EVENT LISTENERS =====
function setupEventListeners() {
    // Theme change
    document.getElementById('setting-theme').addEventListener('change', function(e) {
        const theme = e.target.value;
        localStorage.setItem(SETTINGS_KEYS.THEME, theme);
        applyTheme(theme);
        persistLayoutState({ theme });
        showSuccessAlert('Đã thay đổi chủ đề giao diện');
    });
    
    // Density change
    document.getElementById('setting-density').addEventListener('change', function(e) {
        const density = e.target.value;
        localStorage.setItem(SETTINGS_KEYS.DENSITY, density);
        applyDensity(density);
        persistLayoutState({ density });
        showSuccessAlert('Đã thay đổi mật độ hiển thị');
    });
    
    // Compact Sidebar toggle
    document.getElementById('setting-compact-sidebar').addEventListener('change', function(e) {
        const isCompact = e.target.checked;
        localStorage.setItem(SETTINGS_KEYS.COMPACT_SIDEBAR, isCompact);
        if (isCompact) {
            document.body.classList.add('compact-sidebar');
        } else {
            document.body.classList.remove('compact-sidebar');
        }
        persistLayoutState({ compactSidebar: isCompact });
        showSuccessAlert('Đã ' + (isCompact ? 'thu gọn' : 'mở rộng') + ' thanh điều hướng');
    });
    
    // Auto Refresh toggle
    document.getElementById('setting-auto-refresh').addEventListener('change', function(e) {
        const isEnabled = e.target.checked;
        localStorage.setItem(SETTINGS_KEYS.AUTO_REFRESH, isEnabled);
        if (isEnabled) {
            startAutoRefresh();
        } else {
            stopAutoRefresh();
        }
        showSuccessAlert('Đã ' + (isEnabled ? 'bật' : 'tắt') + ' tự động làm mới');
    });
    
    // Highlight Alert toggle
    document.getElementById('setting-highlight-alert').addEventListener('change', function(e) {
        const isEnabled = e.target.checked;
        localStorage.setItem(SETTINGS_KEYS.HIGHLIGHT_ALERT, isEnabled);
        if (isEnabled) {
            document.body.classList.add('highlight-alert');
        } else {
            document.body.classList.remove('highlight-alert');
        }
        persistLayoutState({ highlightAlert: isEnabled });
        showSuccessAlert('Đã ' + (isEnabled ? 'bật' : 'tắt') + ' làm nổi bật cảnh báo');
    });
    
    // Notification preferences
    document.getElementById('setting-notify-email').addEventListener('change', function(e) {
        localStorage.setItem(SETTINGS_KEYS.NOTIFY_EMAIL, e.target.checked);
        showSuccessAlert('Đã cập nhật tùy chọn thông báo Email');
    });
    
    document.getElementById('setting-notify-sms').addEventListener('change', function(e) {
        localStorage.setItem(SETTINGS_KEYS.NOTIFY_SMS, e.target.checked);
        showSuccessAlert('Đã cập nhật tùy chọn thông báo SMS');
    });
    
    document.getElementById('setting-notify-inapp').addEventListener('change', function(e) {
        localStorage.setItem(SETTINGS_KEYS.NOTIFY_INAPP, e.target.checked);
        showSuccessAlert('Đã cập nhật tùy chọn thông báo trong hệ thống');
    });
    
    // Save contact information
    document.getElementById('btn-save-contact').addEventListener('click', function() {
        saveContactInformation();
    });
    
    // Add Enter key support for inputs
    const inputs = ['setting-contact-email', 'setting-contact-phone'];
    inputs.forEach(id => {
        document.getElementById(id).addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                saveContactInformation();
            }
        });
    });
}

// ===== APPLY THEME =====
function applyTheme(theme) {
    const root = document.documentElement;
    document.body.classList.remove('theme-light', 'theme-dark');

    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    let isDark = false;

    if (theme === 'auto') {
        isDark = prefersDark;
        document.body.classList.add(prefersDark ? 'theme-dark' : 'theme-light');
    } else {
        isDark = theme === 'dark';
        document.body.classList.add('theme-' + theme);
    }

    root.classList.toggle('dark', isDark);
}

// ===== APPLY DENSITY =====
function applyDensity(density) {
    // Remove existing density classes
    document.body.classList.remove('density-comfortable', 'density-compact', 'density-spacious');
    document.body.classList.add('density-' + density);
}

// ===== SAVE CONTACT INFORMATION =====
function saveContactInformation() {
    const email = document.getElementById('setting-contact-email').value.trim();
    const phone = document.getElementById('setting-contact-phone').value.trim();
    const note = document.getElementById('setting-contact-note').value.trim();
    
    // Validate email if provided
    if (email && !isValidEmail(email)) {
        showErrorAlert('Địa chỉ email không hợp lệ');
        return;
    }
    
    // Save to localStorage
    localStorage.setItem(SETTINGS_KEYS.CONTACT_EMAIL, email);
    localStorage.setItem(SETTINGS_KEYS.CONTACT_PHONE, phone);
    localStorage.setItem(SETTINGS_KEYS.CONTACT_NOTE, note);
    
    // Show success message with animation
    showSuccessAlert('Đã lưu cài đặt cá nhân (lưu trên trình duyệt này)');
    
    // Add button animation
    const button = document.getElementById('btn-save-contact');
    button.classList.add('btn-success');
    button.innerHTML = '<i class="fa-solid fa-check me-2"></i>Đã lưu!';
    
    setTimeout(() => {
        button.classList.remove('btn-success');
        button.classList.add('btn-primary');
        button.innerHTML = '<i class="fa-solid fa-floppy-disk me-2"></i>Lưu cài đặt cá nhân';
    }, 2000);
}

// ===== VALIDATION =====
function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

// ===== ALERT FUNCTIONS =====
function showSuccessAlert(message) {
    const alert = document.getElementById('settings-alert');
    alert.classList.remove('d-none', 'alert-danger');
    alert.classList.add('alert-success');
    alert.innerHTML = '<i class="fa-solid fa-circle-check me-2"></i>' + message;
    
    // Auto hide after 3 seconds
    setTimeout(() => {
        alert.classList.add('d-none');
    }, 3000);
}

function showErrorAlert(message) {
    const alert = document.getElementById('settings-alert');
    alert.classList.remove('d-none', 'alert-success');
    alert.classList.add('alert-danger');
    alert.innerHTML = '<i class="fa-solid fa-circle-exclamation me-2"></i>' + message;
    
    // Auto hide after 4 seconds
    setTimeout(() => {
        alert.classList.add('d-none');
    }, 4000);
}

// ===== AUTO REFRESH =====
let autoRefreshInterval = null;

function startAutoRefresh() {
    if (autoRefreshInterval) return; // Already running
    
    autoRefreshInterval = setInterval(() => {
        console.log('Auto-refreshing data...');
        // Here you would implement your data refresh logic
        // For example: refreshDashboardData();
    }, 600000); // 10 minutes
}

function stopAutoRefresh() {
    if (autoRefreshInterval) {
        clearInterval(autoRefreshInterval);
        autoRefreshInterval = null;
    }
}

// Start auto refresh if enabled
if (localStorage.getItem(SETTINGS_KEYS.AUTO_REFRESH) === 'true') {
    startAutoRefresh();
}

// ===== INITIALIZE TOOLTIPS =====
function initializeTooltips() {
    // Bootstrap 5 tooltips
    if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }
}

// ===== LISTEN FOR SYSTEM THEME CHANGES =====
if (window.matchMedia) {
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', e => {
        const theme = localStorage.getItem(SETTINGS_KEYS.THEME);
        if (theme === 'auto') {
            applyTheme('auto');
        }
    });
}

// ===== KEYBOARD SHORTCUTS =====
document.addEventListener('keydown', function(e) {
    // Ctrl/Cmd + S to save contact info
    if ((e.ctrlKey || e.metaKey) && e.key === 's') {
        e.preventDefault();
        saveContactInformation();
    }
});

// ===== EXPORT FOR TESTING =====
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        saveContactInformation,
        applyTheme,
        applyDensity,
        isValidEmail
    };
}