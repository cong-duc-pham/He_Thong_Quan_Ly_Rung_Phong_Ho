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
                // SỬA: Lấy giá trị từ data-selected (được gán từ Server) thay vì .val()
                // Vì lúc page load, dropdown chưa có option nào nên .val() sẽ null
                const selectedThon = filterThon.data('selected');

                this.loadThonsForFilter(filterXa.val(), selectedThon);
            }

            // Bind change event
            filterXa.on('change', () => {
                const maXa = filterXa.val();
                // Khi người dùng tự đổi Xã thì reset Thôn về rỗng
                this.loadThonsForFilter(maXa, '');
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

                        // SỬA: Chuyển cả 2 về String để so sánh chính xác tuyệt đối
                        if (String(item.maThon) === String(selectedValue)) {
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
 * AJAX Handler - tải danh sách Lô rừng với phân trang
 */
LoRung.AjaxHandler = {
    init: function () {
        this.container = $('#lorung-page');
        if (!this.container.length) return;
        if (this.container.data('ajax-bound')) return;
        this.container.data('ajax-bound', true);

        this.endpoint = this.container.data('endpoint');
        this.pageSize = parseInt(this.container.attr('data-pagesize'), 10) || 15;
        this.state = {
            page: parseInt(this.container.attr('data-initial-page'), 10) || 1,
            xa: this.container.attr('data-initial-xa') || '',
            thon: this.container.attr('data-initial-thon') || '',
            loai: this.container.attr('data-initial-loai') || '',
            trangThai: this.container.attr('data-initial-trangthai') || ''
        };

        this.$form = $('#lorung-filter-form');
        this.$xa = $('#filterXa');
        this.$thon = $('#filterThon');
        this.$loai = $('#filterLoai');
        this.$trangThai = $('#filterTrangThai');
        this.$tableBody = $('#lorung-table-body');
        this.$total = $('#lorung-total');
        this.$pagination = $('#lorung-pagination');

        this.bindEvents();
        this.loadData();
    },

    bindEvents: function () {
        const self = this;

        this.$form.on('submit', function (e) {
            e.preventDefault();
            self.updateState();
            self.state.page = 1;
            self.loadData();
        });

        this.$xa.on('change', function () {
            self.state.xa = $(this).val();
            self.state.thon = '';
            self.state.page = 1;
            self.loadData();
        });

        this.$thon.on('change', function () {
            self.state.thon = $(this).val();
            self.state.page = 1;
            self.loadData();
        });

        this.$loai.on('change', function () {
            self.state.loai = $(this).val();
            self.state.page = 1;
            self.loadData();
        });

        this.$trangThai.on('change', function () {
            self.state.trangThai = $(this).val();
            self.state.page = 1;
            self.loadData();
        });

        this.$pagination.on('click', 'a[data-page]', function (e) {
            e.preventDefault();
            const target = parseInt($(this).data('page'), 10);
            if (!isNaN(target) && target >= 1) {
                self.state.page = target;
                self.loadData();
            }
        });

        $('#lorung-reset-filters').on('click', function (e) {
            e.preventDefault();
            self.$xa.val('');
            self.$thon.val('').html('<option value="">-- Tất cả thôn/bản --</option>');
            self.$loai.val('');
            self.$trangThai.val('');
            self.state = { page: 1, xa: '', thon: '', loai: '', trangThai: '' };
            self.loadData();
            LoRung.UIEnhancer.highlightActiveFilters();
        });
    },

    updateState: function () {
        this.state.xa = this.$xa.val();
        this.state.thon = this.$thon.val();
        this.state.loai = this.$loai.val();
        this.state.trangThai = this.$trangThai.val();
    },

    setLoading: function () {
        this.$tableBody.html(`
            <tr>
                <td colspan="10" class="text-center text-muted py-4">
                    <div class="spinner-border text-success" role="status"></div>
                    <div class="mt-2">Đang tải dữ liệu...</div>
                </td>
            </tr>`);
    },

    loadData: function () {
        if (!this.endpoint) return;
        const self = this;
        this.setLoading();

        $.getJSON(this.endpoint, {
            searchXa: self.state.xa,
            searchThon: self.state.thon,
            searchLoai: self.state.loai,
            searchTrangThai: self.state.trangThai,
            pageNumber: self.state.page,
            pageSize: self.pageSize
        })
            .done(function (res) {
                if (res.error) {
                    self.showError(res.error);
                    return;
                }
                self.render(res);
            })
            .fail(function () {
                self.showError('Không thể tải dữ liệu lô rừng.');
            });
    },

    render: function (res) {
        const items = res.items || [];
        const pagination = res.pagination || {};
        const start = ((pagination.pageNumber || 1) - 1) * (pagination.pageSize || this.pageSize) + 1;

        if (!items.length) {
            this.$tableBody.html(`
                <tr>
                    <td colspan="10" class="digioi-empty">
                        <i class="bi bi-inbox"></i>
                        <p>Không tìm thấy dữ liệu</p>
                    </td>
                </tr>`);
        } else {
            let rows = '';
            let stt = start;
            items.forEach(item => {
                const badgeClass = this.getTrangThaiBadge(item.trangThai);
                const dienTichText = item.dienTich ? DiGioi.Utils.formatArea(item.dienTich) : '—';

                rows += `
                    <tr>
                        <td>${stt}</td>
                        <td><strong>${item.soTieuKhu}</strong></td>
                        <td><strong>${item.soKhoanh}</strong></td>
                        <td><strong>${item.soLo}</strong></td>
                        <td>${item.tenThon || ''}</td>
                        <td><span class="digioi-badge digioi-badge-success">${item.tenXa || ''}</span></td>
                        <td>${dienTichText}</td>
                        <td>${item.loaiRung || ''}</td>
                        <td><span class="digioi-badge ${badgeClass}">${item.trangThai || ''}</span></td>
                        <td>
                            <div class="digioi-btn-group">
                                <a href="/LoRung/Edit/${item.maLo}" class="digioi-btn digioi-btn-warning"><i class="bi bi-pencil-square"></i></a>
                                <a href="/LoRung/Delete/${item.maLo}" class="digioi-btn digioi-btn-danger"><i class="bi bi-trash"></i></a>
                            </div>
                        </td>
                    </tr>`;
                stt++;
            });
            this.$tableBody.html(rows);
        }

        if (pagination.totalRecords !== undefined) {
            this.$total.text(`Tổng số: ${pagination.totalRecords} lô rừng`);
        }

        this.renderPagination(pagination);
        LoRung.UIEnhancer.highlightActiveFilters();
    },

    renderPagination: function (pagination) {
        const totalPages = pagination.totalPages || 1;
        const current = pagination.pageNumber || 1;

        if (totalPages <= 1) {
            this.$pagination.empty();
            return;
        }

        const xaParam = encodeURIComponent(this.state.xa || '');
        const thonParam = encodeURIComponent(this.state.thon || '');
        const loaiParam = encodeURIComponent(this.state.loai || '');
        const ttParam = encodeURIComponent(this.state.trangThai || '');

        const buildLink = (page, text, disabled) => {
            const cls = disabled ? 'disabled' : '';
            return `
                <a class="digioi-page ${cls} ${page === current ? 'active' : ''}" data-page="${page}" href="?pageNumber=${page}&searchXa=${xaParam}&searchThon=${thonParam}&searchLoai=${loaiParam}&searchTrangThai=${ttParam}">${text}</a>`;
        };

        let html = '';
        html += buildLink(current - 1, '‹', current === 1);

        for (let i = 1; i <= totalPages; i++) {
            if (Math.abs(i - current) <= 2 || i === 1 || i === totalPages) {
                html += buildLink(i, i, false);
            } else if (i === current - 3 || i === current + 3) {
                html += '<span class="digioi-page disabled">...</span>';
            }
        }

        html += buildLink(current + 1, '›', current === totalPages);
        this.$pagination.html(html);
    },

    getTrangThaiBadge: function (trangThai) {
        switch (trangThai) {
            case 'Rừng giàu':
                return 'digioi-badge-success';
            case 'Rừng trung bình':
                return 'digioi-badge-info';
            case 'Rừng nghèo':
                return 'digioi-badge-warning';
            case 'Đất trống':
                return 'digioi-badge-secondary';
            default:
                return 'digioi-badge-secondary';
        }
    },

    showError: function (message) {
        this.$tableBody.html(`
            <tr>
                <td colspan="10" class="text-center text-danger py-4">${message}</td>
            </tr>`);
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
        if (LoRung.AjaxHandler?.init) LoRung.AjaxHandler.init();
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
