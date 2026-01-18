/* ===================================
   LÔ RỪNG - JavaScript
   Chức năng cascade Xã → Thôn
   =================================== */

var LoRung = LoRung || {};

/**
 * Cascade Dropdown Handler
 * Xử lý cascade dropdown Xã → Thôn
 */
LoRung.CascadeHandler = {
    /**
     * Khởi tạo cascade cho Create/Edit form
     */
    initCascadeForm: function () {
        const selectXa = $('#selectXa');
        const selectThon = $('#selectThon');

        if (selectXa.length && selectThon.length) {
            // Trigger change khi page load nếu có giá trị mặc định
            if (selectXa.val()) {
                this.loadThons(selectXa.val(), selectThon.val());
            }

            // Bind change event
            selectXa.on('change', () => {
                const maXa = selectXa.val();
                this.loadThons(maXa);
            });
        }
    },

    /**
     * Load danh sách Thôn theo Xã
     */
    loadThons: function (maXa, selectedValue = '') {
        const selectThon = $('#selectThon');

        // Disable dropdown và hiển thị loading
        selectThon.prop('disabled', true);
        selectThon.html('<option value="">Đang tải...</option>');

        if (!maXa) {
            selectThon.html('<option value="">-- Chọn thôn/bản --</option>');
            selectThon.prop('disabled', false);
            return;
        }

        // Gọi API để lấy danh sách Thôn
        $.ajax({
            url: '/DanhMucThon/GetByXa',
            type: 'GET',
            data: { maXa: maXa },
            success: function (data) {
                selectThon.html('<option value="">-- Chọn thôn/bản --</option>');

                if (data && data.length > 0) {
                    $.each(data, function (index, item) {
                        const option = $('<option></option>')
                            .val(item.maThon)
                            .text(item.tenThon);

                        // Set selected nếu trùng với giá trị đã chọn
                        if (item.maThon === selectedValue) {
                            option.prop('selected', true);
                        }

                        selectThon.append(option);
                    });
                } else {
                    selectThon.html('<option value="">Không có thôn/bản</option>');
                }

                selectThon.prop('disabled', false);
            },
            error: function (xhr, status, error) {
                console.error('Lỗi tải danh sách Thôn:', error);
                selectThon.html('<option value="">Lỗi tải dữ liệu</option>');
                selectThon.prop('disabled', false);

                if (typeof DiGioi !== 'undefined' && DiGioi.Utils) {
                    DiGioi.Utils.showError('Không thể tải danh sách Thôn/Bản');
                }
            }
        });
    },

    /**
     * Khởi tạo cascade cho Index filter
     */
    initCascadeFilter: function () {
        const filterXa = $('#filterXa');
        const filterThon = $('#filterThon');

        if (filterXa.length && filterThon.length) {
            // Load Thôn khi page load nếu đã chọn Xã
            if (filterXa.val()) {
                this.loadThonsForFilter(filterXa.val(), filterThon.val());
            }

            // Bind change event
            filterXa.on('change', () => {
                const maXa = filterXa.val();
                this.loadThonsForFilter(maXa);
            });
        }
    },

    /**
     * Load Thôn cho dropdown filter
     */
    loadThonsForFilter: function (maXa, selectedValue = '') {
        const filterThon = $('#filterThon');

        filterThon.prop('disabled', true);
        filterThon.html('<option value="">Đang tải...</option>');

        if (!maXa) {
            filterThon.html('<option value="">-- Tất cả thôn --</option>');
            filterThon.prop('disabled', false);
            return;
        }

        $.ajax({
            url: '/DanhMucThon/GetByXa',
            type: 'GET',
            data: { maXa: maXa },
            success: function (data) {
                filterThon.html('<option value="">-- Tất cả thôn --</option>');

                if (data && data.length > 0) {
                    $.each(data, function (index, item) {
                        const option = $('<option></option>')
                            .val(item.maThon)
                            .text(item.tenThon);

                        if (item.maThon === selectedValue) {
                            option.prop('selected', true);
                        }

                        filterThon.append(option);
                    });
                }

                filterThon.prop('disabled', false);
            },
            error: function (xhr, status, error) {
                console.error('Lỗi tải danh sách Thôn:', error);
                filterThon.html('<option value="">Lỗi tải dữ liệu</option>');
                filterThon.prop('disabled', false);
            }
        });
    }
};

/**
 * Form Validation
 */
LoRung.FormValidation = {
    /**
     * Validate số tiểu khu, khoảnh, lô
     */
    validateTechnicalNumbers: function () {
        $('input[name="SoTieuKhu"], input[name="SoKhoanh"], input[name="SoLo"]').on('input', function () {
            const val = $(this).val();
            if (val && (val < 1 || val > 999)) {
                $(this).addClass('is-invalid');
                $(this).next('.invalid-feedback').remove();
                $(this).after('<div class="invalid-feedback">Giá trị phải từ 1 đến 999</div>');
            } else {
                $(this).removeClass('is-invalid');
                $(this).next('.invalid-feedback').remove();
            }
        });
    },

    /**
     * Validate diện tích
     */
    validateArea: function () {
        $('input[name="DienTich"]').on('input', function () {
            const val = parseFloat($(this).val());
            if (isNaN(val) || val <= 0 || val > 9999.99) {
                $(this).addClass('is-invalid');
                $(this).next('.invalid-feedback').remove();
                $(this).after('<div class="invalid-feedback">Diện tích phải từ 0.01 đến 9999.99 ha</div>');
            } else {
                $(this).removeClass('is-invalid');
                $(this).next('.invalid-feedback').remove();
            }
        });
    },

    /**
     * Validate form trước khi submit
     */
    validateBeforeSubmit: function () {
        $('form').on('submit', function (e) {
            let isValid = true;

            // Kiểm tra Thôn đã chọn chưa
            const selectThon = $('#selectThon');
            if (selectThon.length && !selectThon.val()) {
                isValid = false;
                selectThon.addClass('is-invalid');
                if (selectThon.next('.invalid-feedback').length === 0) {
                    selectThon.after('<div class="invalid-feedback">Vui lòng chọn Thôn/Bản</div>');
                }
            }

            // Kiểm tra diện tích
            const dienTich = $('input[name="DienTich"]');
            if (dienTich.length) {
                const val = parseFloat(dienTich.val());
                if (isNaN(val) || val <= 0 || val > 9999.99) {
                    isValid = false;
                    dienTich.addClass('is-invalid');
                }
            }

            if (!isValid) {
                e.preventDefault();
                if (typeof DiGioi !== 'undefined' && DiGioi.Utils) {
                    DiGioi.Utils.showError('Vui lòng kiểm tra lại thông tin form');
                }
            }
        });
    }
};

/**
 * UI Enhancements
 */
LoRung.UIEnhancer = {
    /**
     * Format số hiển thị trong table
     */
    formatTableNumbers: function () {
        $('.table tbody td[data-type="number"]').each(function () {
            const num = $(this).text();
            if (num && !isNaN(num)) {
                $(this).text(DiGioi.Utils.formatNumber(num));
            }
        });
    },

    /**
     * Format diện tích trong table
     */
    formatTableArea: function () {
        $('.table tbody td[data-type="area"]').each(function () {
            const area = $(this).text();
            if (area && !isNaN(area)) {
                $(this).text(DiGioi.Utils.formatArea(area) + ' ha');
            }
        });
    },

    /**
     * Thêm tooltip cho các nút
     */
    initTooltips: function () {
        $('[data-bs-toggle="tooltip"]').each(function () {
            new bootstrap.Tooltip(this);
        });
    },

    /**
     * Highlight filter đang active
     */
    highlightActiveFilters: function () {
        $('#filterXa, #filterThon, #filterLoai, #filterTrangThai').each(function () {
            if ($(this).val()) {
                $(this).addClass('border-success border-2');
            } else {
                $(this).removeClass('border-success border-2');
            }
        });
    }
};

/**
 * API Helper
 */
LoRung.APIHelper = {
    /**
     * Load chi tiết Lô Rừng
     */
    loadDetails: function (maLo, callback) {
        $.ajax({
            url: '/LoRung/GetDetails',
            type: 'GET',
            data: { id: maLo },
            success: function (data) {
                if (callback) {
                    callback(data);
                }
            },
            error: function (xhr, status, error) {
                console.error('Lỗi tải chi tiết Lô Rừng:', error);
                if (typeof DiGioi !== 'undefined' && DiGioi.Utils) {
                    DiGioi.Utils.showError('Không thể tải thông tin chi tiết');
                }
            }
        });
    },

    /**
     * Load danh sách Lô Rừng với filter
     */
    loadList: function (filters, callback) {
        $.ajax({
            url: '/LoRung/GetAll',
            type: 'GET',
            data: filters,
            success: function (data) {
                if (callback) {
                    callback(data);
                }
            },
            error: function (xhr, status, error) {
                console.error('Lỗi tải danh sách Lô Rừng:', error);
            }
        });
    }
};

/**
 * Khởi tạo khi document ready
 */
$(document).ready(function () {
    // Kiểm tra xem đang ở trang nào
    const currentPath = window.location.pathname.toLowerCase();

    // Index page - Filter cascade
    if (currentPath.includes('/lorung/index') || currentPath.endsWith('/lorung')) {
        LoRung.CascadeHandler.initCascadeFilter();
        LoRung.UIEnhancer.highlightActiveFilters();
    }

    // Create page
    if (currentPath.includes('/lorung/create')) {
        LoRung.CascadeHandler.initCascadeForm();
        LoRung.FormValidation.validateTechnicalNumbers();
        LoRung.FormValidation.validateArea();
        LoRung.FormValidation.validateBeforeSubmit();
    }

    // Edit page
    if (currentPath.includes('/lorung/edit')) {
        LoRung.CascadeHandler.initCascadeForm();
        LoRung.FormValidation.validateTechnicalNumbers();
        LoRung.FormValidation.validateArea();
        LoRung.FormValidation.validateBeforeSubmit();
    }

    // Khởi tạo tooltips trên mọi trang
    LoRung.UIEnhancer.initTooltips();
});
