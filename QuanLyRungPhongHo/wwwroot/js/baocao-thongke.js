/**
 * ====================================
 * BÁO CÁO THỐNG KÊ - JavaScript Module
 * ====================================
 */

const BaoCaoThongKe = {
    /**
     * Khởi tạo module
     */
    init: function() {
        this.initExportModals();
        this.initDateValidation();
        this.initFilterForm();
    },

    /**
     * Khởi tạo modal xuất file
     */
    initExportModals: function() {
        // Modal Xuất Excel
        const exportExcelModal = document.getElementById('exportExcelModal');
        if (exportExcelModal) {
            // Tự động cập nhật tên file khi thay đổi options
            const updateExcelFileName = () => {
                const maXa = document.getElementById('exportMaXaFilter').value;
                const tenXa = maXa ? document.getElementById('exportMaXaFilter').selectedOptions[0].text : 'ToanTinh';
                const tuNgay = document.getElementById('exportTuNgay').value;
                const denNgay = document.getElementById('exportDenNgay').value;
                
                const tenXaSlug = this.slugify(tenXa);
                const tuNgayStr = tuNgay ? tuNgay.replace(/-/g, '') : this.formatDate(new Date(Date.now() - 30*24*60*60*1000));
                const denNgayStr = denNgay ? denNgay.replace(/-/g, '') : this.formatDate(new Date());
                const timestamp = this.formatDateTime(new Date());
                
                const fileName = `BaoCao_${tenXaSlug}_${tuNgayStr}_${denNgayStr}_${timestamp}.xlsx`;
                document.getElementById('exportFileName').value = fileName;
            };

            document.getElementById('exportMaXaFilter').addEventListener('change', updateExcelFileName);
            document.getElementById('exportTuNgay').addEventListener('change', updateExcelFileName);
            document.getElementById('exportDenNgay').addEventListener('change', updateExcelFileName);
            
            // Cập nhật tên file lần đầu
            exportExcelModal.addEventListener('shown.bs.modal', updateExcelFileName);

            // Xử lý nút xuất Excel
            document.getElementById('btnConfirmExportExcel').addEventListener('click', () => {
                this.exportExcel();
            });
        }

        // Modal Xuất PDF
        const exportPdfModal = document.getElementById('exportPdfModal');
        if (exportPdfModal) {
            // Tự động cập nhật tên file PDF
            const updatePdfFileName = () => {
                const maXa = document.getElementById('pdfMaXaFilter').value;
                const tenXa = maXa ? document.getElementById('pdfMaXaFilter').selectedOptions[0].text : 'ToanTinh';
                const tuNgay = document.getElementById('pdfTuNgay').value;
                const denNgay = document.getElementById('pdfDenNgay').value;
                
                const tenXaSlug = this.slugify(tenXa);
                const tuNgayStr = tuNgay ? tuNgay.replace(/-/g, '') : this.formatDate(new Date(Date.now() - 30*24*60*60*1000));
                const denNgayStr = denNgay ? denNgay.replace(/-/g, '') : this.formatDate(new Date());
                const timestamp = this.formatDateTime(new Date());
                
                const fileName = `BaoCao_${tenXaSlug}_${tuNgayStr}_${denNgayStr}_${timestamp}.pdf`;
                document.getElementById('pdfFileName').value = fileName;
            };

            document.getElementById('pdfMaXaFilter').addEventListener('change', updatePdfFileName);
            document.getElementById('pdfTuNgay').addEventListener('change', updatePdfFileName);
            document.getElementById('pdfDenNgay').addEventListener('change', updatePdfFileName);
            
            // Cập nhật tên file lần đầu
            exportPdfModal.addEventListener('shown.bs.modal', updatePdfFileName);

            // Xử lý nút xuất PDF
            document.getElementById('btnConfirmExportPdf').addEventListener('click', () => {
                this.exportPdf();
            });
        }
    },

    /**
     * Xuất file Excel
     */
    exportExcel: function() {
        const maXa = document.getElementById('exportMaXaFilter').value;
        const tuNgay = document.getElementById('exportTuNgay').value;
        const denNgay = document.getElementById('exportDenNgay').value;

        // Build URL
        const params = new URLSearchParams();
        if (maXa) params.append('maXaFilter', maXa);
        if (tuNgay) params.append('tuNgay', tuNgay);
        if (denNgay) params.append('denNgay', denNgay);

        const url = `/BaoCaoThongKe/ExportCsv?${params.toString()}`;

        // Đóng modal
        const modal = bootstrap.Modal.getInstance(document.getElementById('exportExcelModal'));
        modal.hide();

        // Hiển thị loading
        this.showToast('Đang xuất file Excel...', 'info');

        // Tải file
        window.location.href = url;

        // Thông báo thành công sau 1.5s
        setTimeout(() => {
            this.showToast('Đã xuất file Excel thành công!', 'success');
        }, 1500);
    },

    /**
     * Xuất file PDF
     */
    exportPdf: function() {
        const maXa = document.getElementById('pdfMaXaFilter').value;
        const tuNgay = document.getElementById('pdfTuNgay').value;
        const denNgay = document.getElementById('pdfDenNgay').value;

        // Build URL
        const params = new URLSearchParams();
        if (maXa) params.append('maXaFilter', maXa);
        if (tuNgay) params.append('tuNgay', tuNgay);
        if (denNgay) params.append('denNgay', denNgay);

        const url = `/BaoCaoThongKe/ExportPdf?${params.toString()}`;

        // Đóng modal
        const modal = bootstrap.Modal.getInstance(document.getElementById('exportPdfModal'));
        modal.hide();

        // Hiển thị loading
        this.showToast('Đang chuẩn bị file PDF...', 'info');

        // Mở trang PDF trong tab mới
        window.open(url, '_blank');

        // Thông báo
        setTimeout(() => {
            this.showToast('Đã mở trang PDF. Sử dụng Ctrl+P để in hoặc lưu thành PDF.', 'success');
        }, 500);
    },

    /**
     * Chuyển đổi tên thành slug (không dấu, không khoảng trắng)
     */
    slugify: function(text) {
        const from = "àáãảạăằắẳẵặâầấẩẫậèéẻẽẹêềếểễệđùúủũụưừứửữựòóỏõọôồốổỗộơờớởỡợìíỉĩịäëïîöüûñçýỳỹỵỷ";
        const to   = "aaaaaaaaaaaaaaaaaeeeeeeeeeeeduuuuuuuuuuuoooooooooooooooooiiiiiaeiiouuncyyyyy";
        
        for (let i = 0, l = from.length; i < l; i++) {
            text = text.replace(new RegExp(from[i], "gi"), to[i]);
        }

        return text.toString().toLowerCase()
            .replace(/\s+/g, '')           // Xóa khoảng trắng
            .replace(/[^\w\-]+/g, '')     // Xóa ký tự đặc biệt
            .replace(/\-\-+/g, '')        // Xóa dấu gạch ngang trùng
            .replace(/^-+/, '')           // Xóa dấu gạch ngang đầu
            .replace(/-+$/, '');          // Xóa dấu gạch ngang cuối
    },

    /**
     * Format ngày: yyyyMMdd
     */
    formatDate: function(date) {
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        return `${year}${month}${day}`;
    },

    /**
     * Format ngày giờ: yyyyMMdd_HHmmss
     */
    formatDateTime: function(date) {
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        const hours = String(date.getHours()).padStart(2, '0');
        const minutes = String(date.getMinutes()).padStart(2, '0');
        const seconds = String(date.getSeconds()).padStart(2, '0');
        return `${year}${month}${day}_${hours}${minutes}${seconds}`;
    },

    /**
     * Validation cho Date Range
     */
    initDateValidation: function() {
        // Validation cho Export Excel Modal
        const exportTuNgay = document.getElementById('exportTuNgay');
        const exportDenNgay = document.getElementById('exportDenNgay');
        
        if (exportTuNgay && exportDenNgay) {
            const today = new Date().toISOString().split('T')[0];
            exportTuNgay.setAttribute('max', today);
            exportDenNgay.setAttribute('max', today);

            exportTuNgay.addEventListener('change', () => {
                if (exportDenNgay.value && exportTuNgay.value > exportDenNgay.value) {
                    this.showToast('Từ ngày không được lớn hơn Đến ngày', 'warning');
                    exportTuNgay.value = '';
                }
                if (exportTuNgay.value) {
                    exportDenNgay.setAttribute('min', exportTuNgay.value);
                }
            });

            exportDenNgay.addEventListener('change', () => {
                if (exportTuNgay.value && exportDenNgay.value < exportTuNgay.value) {
                    this.showToast('Đến ngày không được nhỏ hơn Từ ngày', 'warning');
                    exportDenNgay.value = '';
                }
                if (exportDenNgay.value) {
                    exportTuNgay.setAttribute('max', exportDenNgay.value);
                }
            });
        }

        // Validation cho Export PDF Modal
        const pdfTuNgay = document.getElementById('pdfTuNgay');
        const pdfDenNgay = document.getElementById('pdfDenNgay');
        
        if (pdfTuNgay && pdfDenNgay) {
            const today = new Date().toISOString().split('T')[0];
            pdfTuNgay.setAttribute('max', today);
            pdfDenNgay.setAttribute('max', today);

            pdfTuNgay.addEventListener('change', () => {
                if (pdfDenNgay.value && pdfTuNgay.value > pdfDenNgay.value) {
                    this.showToast('Từ ngày không được lớn hơn Đến ngày', 'warning');
                    pdfTuNgay.value = '';
                }
                if (pdfTuNgay.value) {
                    pdfDenNgay.setAttribute('min', pdfTuNgay.value);
                }
            });

            pdfDenNgay.addEventListener('change', () => {
                if (pdfTuNgay.value && pdfDenNgay.value < pdfTuNgay.value) {
                    this.showToast('Đến ngày không được nhỏ hơn Từ ngày', 'warning');
                    pdfDenNgay.value = '';
                }
                if (pdfDenNgay.value) {
                    pdfTuNgay.setAttribute('max', pdfDenNgay.value);
                }
            });
        }

        // Validation cho Filter Form chính
        const tuNgayInput = document.querySelector('input[name="tuNgay"]');
        const denNgayInput = document.querySelector('input[name="denNgay"]');

        if (tuNgayInput && denNgayInput) {
            const today = new Date().toISOString().split('T')[0];
            tuNgayInput.setAttribute('max', today);
            denNgayInput.setAttribute('max', today);

            tuNgayInput.addEventListener('change', () => {
                if (denNgayInput.value && tuNgayInput.value > denNgayInput.value) {
                    this.showToast('Từ ngày không được lớn hơn Đến ngày', 'warning');
                    tuNgayInput.value = '';
                }
                if (tuNgayInput.value) {
                    denNgayInput.setAttribute('min', tuNgayInput.value);
                }
            });

            denNgayInput.addEventListener('change', () => {
                if (tuNgayInput.value && denNgayInput.value < tuNgayInput.value) {
                    this.showToast('Đến ngày không được nhỏ hơn Từ ngày', 'warning');
                    denNgayInput.value = '';
                }
                if (denNgayInput.value) {
                    tuNgayInput.setAttribute('max', denNgayInput.value);
                }
            });
        }
    },

    /**
     * Xử lý Filter Form
     */
    initFilterForm: function() {
        const filterForm = document.querySelector('.digioi-filter-form');
        if (filterForm) {
            filterForm.addEventListener('submit', (e) => {
                this.showToast('Đang tải dữ liệu...', 'info');
            });
        }
    },

    /**
     * Hiển thị thông báo Toast
     */
    showToast: function(message, type = 'info') {
        // Tạo toast element
        const toastHtml = `
            <div class="toast align-items-center text-white bg-${type === 'success' ? 'success' : type === 'warning' ? 'warning' : type === 'error' ? 'danger' : 'primary'} border-0" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="d-flex">
                    <div class="toast-body">
                        <i class="bi bi-${type === 'success' ? 'check-circle' : type === 'warning' ? 'exclamation-triangle' : type === 'error' ? 'x-circle' : 'info-circle'}"></i>
                        ${message}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
            </div>
        `;

        // Tạo container nếu chưa có
        let toastContainer = document.querySelector('.toast-container');
        if (!toastContainer) {
            toastContainer = document.createElement('div');
            toastContainer.className = 'toast-container position-fixed top-0 end-0 p-3';
            toastContainer.style.zIndex = '9999';
            document.body.appendChild(toastContainer);
        }

        // Thêm toast vào container
        toastContainer.insertAdjacentHTML('beforeend', toastHtml);
        const toastElement = toastContainer.lastElementChild;

        // Hiển thị toast
        const toast = new bootstrap.Toast(toastElement, {
            autohide: true,
            delay: 3000
        });
        toast.show();

        // Xóa toast sau khi ẩn
        toastElement.addEventListener('hidden.bs.toast', () => {
            toastElement.remove();
        });
    }
};

// Khởi tạo khi DOM ready
document.addEventListener('DOMContentLoaded', function() {
    BaoCaoThongKe.init();
});