/* ===================================
   Module QUẢN LÝ ĐỊA GIỚI - JavaScript
   Hệ Thống Quản Lý Rừng Phòng Hộ
   =================================== */

// Namespace cho module Địa Giới
var DiGioi = DiGioi || {};

/**
 * Utilities - Các hàm tiện ích chung
 */
DiGioi.Utils = {
    /**
     * Hiển thị thông báo thành công
     */
    showSuccess: function (message) {
        const alertHtml = `
            <div class="alert alert-success alert-dismissible fade show" role="alert">
                <i class="bi bi-check-circle-fill me-2"></i>${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;
        this.insertAlert(alertHtml);
    },

    /**
     * Hiển thị thông báo lỗi
     */
    showError: function (message) {
        const alertHtml = `
            <div class="alert alert-danger alert-dismissible fade show" role="alert">
                <i class="bi bi-exclamation-triangle-fill me-2"></i>${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;
        this.insertAlert(alertHtml);
    },

    /**
     * Hiển thị thông báo cảnh báo
     */
    showWarning: function (message) {
        const alertHtml = `
            <div class="alert alert-warning alert-dismissible fade show" role="alert">
                <i class="bi bi-exclamation-circle-fill me-2"></i>${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;
        this.insertAlert(alertHtml);
    },

    /**
     * Chèn alert vào container
     */
    insertAlert: function (html) {
        const container = $('.container').first();
        if (container.length) {
            container.prepend(html);
            // Auto dismiss sau 5 giây
            setTimeout(function () {
                $('.alert').fadeOut('slow', function () {
                    $(this).remove();
                });
            }, 5000);
        }
    },

    /**
     * Format số với dấu phân cách hàng nghìn
     */
    formatNumber: function (num) {
        return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ".");
    },

    /**
     * Format diện tích (2 chữ số thập phân)
     */
    formatArea: function (area) {
        return parseFloat(area).toFixed(2).replace('.', ',');
    },

    /**
     * Confirm xóa
     */
    confirmDelete: function (itemName) {
        return confirm(`Bạn có chắc chắn muốn xóa "${itemName}" không?`);
    },

    /**
     * Disable/Enable button với loading state
     */
    setButtonLoading: function (button, isLoading) {
        const $btn = $(button);
        if (isLoading) {
            $btn.prop('disabled', true);
            $btn.data('original-html', $btn.html());
            $btn.html('<span class="spinner-border spinner-border-sm me-2"></span>Đang xử lý...');
        } else {
            $btn.prop('disabled', false);
            $btn.html($btn.data('original-html'));
        }
    },

    /**
     * Validate form trước khi submit
     */
    validateForm: function (formId) {
        const form = $(formId);
        if (form.length && form.valid) {
            return form.valid();
        }
        return true;
    }
};

/**
 * Form Handler - Xử lý form chung
 */
DiGioi.FormHandler = {
    /**
     * Khởi tạo form validation
     */
    initValidation: function () {
        $('form').each(function () {
            if ($(this).data('validation') !== 'disabled') {
                $(this).validate({
                    errorClass: 'is-invalid',
                    validClass: 'is-valid',
                    errorElement: 'div',
                    errorPlacement: function (error, element) {
                        error.addClass('invalid-feedback');
                        element.closest('.mb-3').append(error);
                    }
                });
            }
        });
    },

    /**
     * Xử lý submit form với AJAX
     */
    submitFormAjax: function (formId, successCallback, errorCallback) {
        $(formId).on('submit', function (e) {
            e.preventDefault();

            if (!DiGioi.Utils.validateForm(formId)) {
                return false;
            }

            const $form = $(this);
            const $submitBtn = $form.find('button[type="submit"]');

            DiGioi.Utils.setButtonLoading($submitBtn, true);

            $.ajax({
                url: $form.attr('action'),
                type: $form.attr('method'),
                data: $form.serialize(),
                success: function (response) {
                    DiGioi.Utils.setButtonLoading($submitBtn, false);
                    if (successCallback) {
                        successCallback(response);
                    }
                },
                error: function (xhr, status, error) {
                    DiGioi.Utils.setButtonLoading($submitBtn, false);
                    if (errorCallback) {
                        errorCallback(xhr, status, error);
                    } else {
                        DiGioi.Utils.showError('Có lỗi xảy ra: ' + error);
                    }
                }
            });
        });
    }
};

/**
 * Search & Filter Handler
 */
DiGioi.SearchFilter = {
    /**
     * Debounce function cho search input
     */
    debounce: function (func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    },

    /**
     * Khởi tạo live search
     */
    initLiveSearch: function (inputSelector, searchCallback) {
        const debouncedSearch = this.debounce(searchCallback, 500);
        $(inputSelector).on('keyup', function () {
            debouncedSearch($(this).val());
        });
    },

    /**
     * Reset filters
     */
    resetFilters: function (formSelector) {
        $(formSelector + ' select, ' + formSelector + ' input[type="text"]').val('');
        $(formSelector).submit();
    }
};

/**
 * Table Handler - Xử lý bảng dữ liệu
 */
DiGioi.TableHandler = {
    /**
     * Thêm chức năng sort cho bảng
     */
    initTableSort: function (tableSelector) {
        $(tableSelector + ' thead th').each(function () {
            if ($(this).data('sortable') !== false) {
                $(this).css('cursor', 'pointer');
                $(this).on('click', function () {
                    // Sort logic here
                });
            }
        });
    },

    /**
     * Highlight row khi hover
     */
    initRowHighlight: function (tableSelector) {
        $(tableSelector + ' tbody tr').hover(
            function () {
                $(this).addClass('table-active');
            },
            function () {
                $(this).removeClass('table-active');
            }
        );
    }
};

/**
 * Badge Helper - Tạo badge động
 */
DiGioi.BadgeHelper = {
    /**
     * Tạo badge cho trạng thái rừng
     */
    createTrangThaiBadge: function (trangThai) {
        const badgeMap = {
            'Rừng giàu': 'bg-success',
            'Rừng trung bình': 'bg-info',
            'Rừng nghèo': 'bg-warning',
            'Đất trống': 'bg-secondary'
        };
        const badgeClass = badgeMap[trangThai] || 'bg-secondary';
        return `<span class="badge ${badgeClass}">${trangThai}</span>`;
    },

    /**
     * Tạo badge cho loại rừng
     */
    createLoaiRungBadge: function (loaiRung) {
        return `<span class="badge bg-success">${loaiRung}</span>`;
    }
};

/**
 * XaHandler - AJAX cho Danh Muc Xa
 */
DiGioi.XaHandler = {
    init: function () {
        this.container = $('#xa-page');
        if (!this.container.length) return;

        if (this.container.data('ajax-bound')) return;
        this.container.data('ajax-bound', true);

        this.endpoint = this.container.data('endpoint');
        this.pageSize = parseInt(this.container.attr('data-pagesize'), 10) || 10;
        this.state = {
            page: parseInt(this.container.attr('data-initial-page'), 10) || 1,
            search: this.container.attr('data-initial-search') || ''
        };

        this.$tableBody = $('#xa-table-body');
        this.$total = $('#xa-total');
        this.$pagination = $('#xa-pagination');
        this.$searchInput = $('#xa-search');

        this.bindEvents();
        this.loadData();
    },

    bindEvents: function () {
        const self = this;

        $('#xa-search-form').on('submit', function (e) {
            e.preventDefault();
            self.state.search = self.$searchInput.val();
            self.state.page = 1;
            self.loadData();
        });

        DiGioi.SearchFilter.initLiveSearch('#xa-search', function (value) {
            self.state.search = value;
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
    },

    setLoading: function () {
        this.$tableBody.html(`
            <tr>
                <td colspan="5" class="text-center text-muted py-4">
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
            searchString: self.state.search,
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
                self.showError('Không thể tải dữ liệu xã.');
            });
    },

    render: function (res) {
        const items = res.items || [];
        const pagination = res.pagination || {};
        const start = ((pagination.pageNumber || 1) - 1) * (pagination.pageSize || this.pageSize) + 1;

        if (!items.length) {
            this.$tableBody.html(`
                <tr>
                    <td colspan="5" class="text-center text-muted py-4">
                        <i class="bi bi-inbox fs-1"></i>
                        <div>Không có dữ liệu</div>
                    </td>
                </tr>`);
        } else {
            let rows = '';
            let stt = start;
            items.forEach(item => {
                rows += `
                    <tr>
                        <td>${stt}</td>
                        <td><strong>${item.maXa}</strong></td>
                        <td>${item.tenXa}</td>
                        <td class="text-center"><span class="badge bg-secondary">${item.soThon}</span></td>
                        <td class="text-center">
                            <div class="btn-group btn-group-sm">
                                <a href="/DanhMucXa/Edit/${item.maXa}" class="btn btn-warning"><i class="bi bi-pencil-square"></i></a>
                                <a href="/DanhMucXa/Delete/${item.maXa}" class="btn btn-danger"><i class="bi bi-trash"></i></a>
                            </div>
                        </td>
                    </tr>`;
                stt++;
            });
            this.$tableBody.html(rows);
        }

        if (pagination.totalRecords !== undefined) {
            this.$total.text(`Tổng số: ${pagination.totalRecords} xã`);
        }

        this.renderPagination(pagination);
    },

    renderPagination: function (pagination) {
        const totalPages = pagination.totalPages || 1;
        const current = pagination.pageNumber || 1;

        if (totalPages <= 1) {
            this.$pagination.empty();
            return;
        }

        let html = '<ul class="pagination justify-content-center">';
        html += `
            <li class="page-item ${current === 1 ? 'disabled' : ''}">
                <a class="page-link" data-page="${current - 1}" href="?pageNumber=${current - 1}&searchString=${encodeURIComponent(this.state.search || '')}">«</a>
            </li>`;

        for (let i = 1; i <= totalPages; i++) {
            if (Math.abs(i - current) <= 2 || i === 1 || i === totalPages) {
                const active = i === current ? 'active' : '';
                html += `
                    <li class="page-item ${active}">
                        <a class="page-link" data-page="${i}" href="?pageNumber=${i}&searchString=${encodeURIComponent(this.state.search || '')}">${i}</a>
                    </li>`;
            } else if (i === current - 3 || i === current + 3) {
                html += '<li class="page-item disabled"><span class="page-link">...</span></li>';
            }
        }

        html += `
            <li class="page-item ${current === totalPages ? 'disabled' : ''}">
                <a class="page-link" data-page="${current + 1}" href="?pageNumber=${current + 1}&searchString=${encodeURIComponent(this.state.search || '')}">»</a>
            </li>`;
        html += '</ul>';

        this.$pagination.html(html);
    },

    showError: function (message) {
        this.$tableBody.html(`
            <tr>
                <td colspan="5" class="text-center text-danger py-4">${message}</td>
            </tr>`);
    }
};

/**
 * ThonHandler - AJAX cho Danh Muc Thon
 */
DiGioi.ThonHandler = {
    init: function () {
        this.container = $('#thon-page');
        if (!this.container.length) return;

        if (this.container.data('ajax-bound')) return;
        this.container.data('ajax-bound', true);

        this.endpoint = this.container.data('endpoint');
        
        // Nếu không có endpoint, không init AJAX
        if (!this.endpoint) {
            console.log('Thon: No AJAX endpoint - using server-side rendering');
            return;
        }
        
        console.log('Thon: Initializing AJAX handler');
        this.pageSize = parseInt(this.container.attr('data-pagesize'), 10) || 10;
        this.state = {
            page: parseInt(this.container.attr('data-initial-page'), 10) || 1,
            search: this.container.attr('data-initial-search') || '',
            xa: this.container.attr('data-initial-xa') || ''
        };

        this.$tableBody = $('#thon-table-body');
        this.$total = $('#thon-total');
        this.$pagination = $('#thon-pagination');
        this.$searchInput = $('#thon-filter-search');
        this.$xaSelect = $('#thon-filter-xa');

        this.bindEvents();
        
        // Chỉ load data nếu table đang empty
        const hasData = this.$tableBody.find('tr').length > 0 && 
                       !this.$tableBody.find('.digioi-empty').length;
        
        if (!hasData || this.container.hasClass('force-reload')) {
            console.log('Thon: Loading initial data via AJAX');
            this.loadData();
        } else {
            console.log('Thon: Using server-rendered data');
        }
    },

    bindEvents: function () {
        const self = this;
        
        // Flag to prevent auto-trigger during initialization
        this.isInitializing = true;
        setTimeout(() => { self.isInitializing = false; }, 500);

        $('#thon-filter-form').on('submit', function (e) {
            e.preventDefault();
            self.state.search = self.$searchInput.val();
            self.state.xa = self.$xaSelect.val();
            self.state.page = 1;
            self.loadData();
        });

        this.$xaSelect.on('change', function () {
            if (self.isInitializing) return;
            self.state.xa = $(this).val();
            self.state.page = 1;
            self.loadData();
        });

        DiGioi.SearchFilter.initLiveSearch('#thon-filter-search', function (value) {
            if (self.isInitializing) return;
            self.state.search = value;
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
    },

    setLoading: function () {
        this.$tableBody.html(`
            <tr>
                <td colspan="6" class="text-center text-muted py-4">
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
            searchString: self.state.search,
            pageNumber: self.state.page,
            pageSize: self.pageSize
        })
            .done(function (res) {
                console.log('Thon API Response:', res);
                if (res.error) {
                    self.showError(res.error);
                    return;
                }
                if (res.items && res.items.length > 0) {
                    console.log('Thon First item:', res.items[0]);
                }
                self.render(res);
            })
            .fail(function (xhr, status, error) {
                console.error('Thon API Error:', error, xhr);
                self.showError('Không thể tải dữ liệu thôn.');
            });
    },

    render: function (res) {
        const items = res.items || [];
        const pagination = res.pagination || {};
        const start = ((pagination.pageNumber || 1) - 1) * (pagination.pageSize || this.pageSize) + 1;

        if (!items.length) {
            this.$tableBody.html(`
                <tr>
                    <td colspan="6" class="digioi-empty">
                        <i class="bi bi-inbox"></i>
                        <p>Không tìm thấy dữ liệu</p>
                    </td>
                </tr>`);
        } else {
            let rows = '';
            let stt = start;
            items.forEach(item => {
                const maThon = item.MaThon ?? '—';
                const tenThon = item.TenThon ?? '—';
                const tenXa = item.TenXa ?? '—';
                const soLoRung = item.SoLoRung ?? 0;
                
                rows += `
                    <tr>
                        <td>${stt}</td>
                        <td><strong>${maThon}</strong></td>
                        <td>${tenThon}</td>
                        <td><span class="digioi-badge digioi-badge-success">${tenXa}</span></td>
                        <td class="text-center"><span class="digioi-badge digioi-badge-secondary">${soLoRung}</span></td>
                        <td class="text-center">
                            <div class="digioi-btn-group">
                                <a href="/DanhMucThon/Edit/${item.MaThon}" class="digioi-btn digioi-btn-warning"><i class="bi bi-pencil-square"></i></a>
                                <a href="/DanhMucThon/Delete/${item.MaThon}" class="digioi-btn digioi-btn-danger"><i class="bi bi-trash"></i></a>
                            </div>
                        </td>
                    </tr>`;
                stt++;
            });
            this.$tableBody.html(rows);
        }

        if (pagination.totalRecords !== undefined) {
            this.$total.text(`Tổng số: ${pagination.totalRecords} thôn/bản`);
        }

        this.renderPagination(pagination);
    },

    renderPagination: function (pagination) {
        const totalPages = pagination.totalPages || 1;
        const current = pagination.pageNumber || 1;

        if (totalPages <= 1) {
            this.$pagination.empty();
            return;
        }

        let html = '';
        const searchParam = encodeURIComponent(this.state.search || '');
        const xaParam = encodeURIComponent(this.state.xa || '');

        const buildLink = (page, text, disabled) => {
            const cls = disabled ? 'disabled' : '';
            return `
                <a class="digioi-page ${cls} ${page === current ? 'active' : ''}" data-page="${page}" href="?pageNumber=${page}&searchXa=${xaParam}&searchString=${searchParam}">${text}</a>`;
        };

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

    showError: function (message) {
        this.$tableBody.html(`
            <tr>
                <td colspan="6" class="text-center text-danger py-4">${message}</td>
            </tr>`);
    }
};

/**
 * Khởi tạo khi document ready
 */
$(document).ready(function () {
    // Khởi tạo validation cho tất cả form
    if (typeof $.fn.validate !== 'undefined') {
        DiGioi.FormHandler.initValidation();
    }

    // Khởi tạo AJAX cho trang Địa Giới
    if (DiGioi.XaHandler?.init) DiGioi.XaHandler.init();
    if (DiGioi.ThonHandler?.init) DiGioi.ThonHandler.init();

    // Auto dismiss alerts sau 5 giây
    setTimeout(function () {
        $('.alert:not(.alert-permanent)').fadeOut('slow', function () {
            $(this).remove();
        });
    }, 5000);

    // Thêm confirmation cho các nút xóa
    $('.btn-delete, .delete-button').on('click', function (e) {
        if (!confirm('Bạn có chắc chắn muốn xóa không?')) {
            e.preventDefault();
            return false;
        }
    });

    // Thêm required asterisk cho các label bắt buộc
    $('input[required], select[required], textarea[required]').each(function () {
        const label = $('label[for="' + $(this).attr('id') + '"]');
        if (label.length && !label.find('.text-danger').length) {
            label.append(' <span class="text-danger">*</span>');
        }
    });

    // Focus vào input đầu tiên trong form
    $('form .form-control:not([readonly]):not([disabled]):first').focus();
});
