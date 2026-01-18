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
 * Khởi tạo khi document ready
 */
$(document).ready(function () {
    // Khởi tạo validation cho tất cả form
    if (typeof $.fn.validate !== 'undefined') {
        DiGioi.FormHandler.initValidation();
    }

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
