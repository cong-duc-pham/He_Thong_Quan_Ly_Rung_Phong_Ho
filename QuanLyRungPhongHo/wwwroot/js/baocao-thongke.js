const BaoCaoThongKe = {
    init: function () {
        this.initExportModals();
        this.initDateValidation();
        this.initFilterForm();
    },

    initExportModals: function () {
        // Setup Excel export modal
        const exportExcelModal = document.getElementById('exportExcelModal');
        if (exportExcelModal) {
            const updateExcelFileName = () => {
                const maXa = document.getElementById('exportMaXaFilter').value;
                const tenXa = maXa ? document.getElementById('exportMaXaFilter').selectedOptions[0].text : 'ToanTinh';
                const tuNgay = document.getElementById('exportTuNgay').value;
                const denNgay = document.getElementById('exportDenNgay').value;

                const tenXaSlug = this.slugify(tenXa);
                const tuNgayStr = tuNgay ? tuNgay.replace(/-/g, '') : this.formatDate(new Date(Date.now() - 30 * 24 * 60 * 60 * 1000));
                const denNgayStr = denNgay ? denNgay.replace(/-/g, '') : this.formatDate(new Date());
                const timestamp = this.formatDateTime(new Date());

                const fileName = `BaoCao_${tenXaSlug}_${tuNgayStr}_${denNgayStr}_${timestamp}.xlsx`;
                document.getElementById('exportFileName').value = fileName;
            };

            document.getElementById('exportMaXaFilter').addEventListener('change', updateExcelFileName);
            document.getElementById('exportTuNgay').addEventListener('change', () => {
                updateExcelFileName();
                this.validateExcelDateInputs();
            });
            document.getElementById('exportDenNgay').addEventListener('change', () => {
                updateExcelFileName();
                this.validateExcelDateInputs();
            });
            exportExcelModal.addEventListener('shown.bs.modal', updateExcelFileName);

            document.getElementById('btnConfirmExportExcel').addEventListener('click', () => {
                this.exportExcel();
            });
        }

        // Setup PDF export modal
        const exportPdfModal = document.getElementById('exportPdfModal');
        if (exportPdfModal) {
            const updatePdfFileName = () => {
                const maXa = document.getElementById('pdfMaXaFilter').value;
                const tenXa = maXa ? document.getElementById('pdfMaXaFilter').selectedOptions[0].text : 'ToanTinh';
                const tuNgay = document.getElementById('pdfTuNgay').value;
                const denNgay = document.getElementById('pdfDenNgay').value;

                const tenXaSlug = this.slugify(tenXa);
                const tuNgayStr = tuNgay ? tuNgay.replace(/-/g, '') : this.formatDate(new Date(Date.now() - 30 * 24 * 60 * 60 * 1000));
                const denNgayStr = denNgay ? denNgay.replace(/-/g, '') : this.formatDate(new Date());
                const timestamp = this.formatDateTime(new Date());

                const fileName = `BaoCao_${tenXaSlug}_${tuNgayStr}_${denNgayStr}_${timestamp}.pdf`;
                document.getElementById('pdfFileName').value = fileName;
            };

            document.getElementById('pdfMaXaFilter').addEventListener('change', updatePdfFileName);
            document.getElementById('pdfTuNgay').addEventListener('change', () => {
                updatePdfFileName();
                this.validatePdfDateInputs();
            });
            document.getElementById('pdfDenNgay').addEventListener('change', () => {
                updatePdfFileName();
                this.validatePdfDateInputs();
            });
            exportPdfModal.addEventListener('shown.bs.modal', updatePdfFileName);

            document.getElementById('btnConfirmExportPdf').addEventListener('click', () => {
                this.exportPdf();
            });
        }
    },

    validateExcelDateInputs: function () {
        const tuNgay = document.getElementById('exportTuNgay');
        const denNgay = document.getElementById('exportDenNgay');
        const tuNgayError = document.getElementById('exportTuNgayError');
        const denNgayError = document.getElementById('exportDenNgayError');
        const tuNgayErrorMsg = document.getElementById('exportTuNgayErrorMsg');
        const denNgayErrorMsg = document.getElementById('exportDenNgayErrorMsg');

        let hasError = false;

        // Clear error states
        tuNgay.classList.remove('is-invalid');
        denNgay.classList.remove('is-invalid');
        tuNgayError.classList.add('d-none');
        denNgayError.classList.add('d-none');

        // Check Từ ngày
        if (!tuNgay.value) {
            tuNgay.classList.add('is-invalid');
            tuNgayErrorMsg.textContent = 'Vui lòng chọn ngày bắt đầu';
            tuNgayError.classList.remove('d-none');
            hasError = true;
        }

        // Check Đến ngày
        if (!denNgay.value) {
            denNgay.classList.add('is-invalid');
            denNgayErrorMsg.textContent = 'Vui lòng chọn ngày kết thúc';
            denNgayError.classList.remove('d-none');
            hasError = true;
        }

        // Check date range
        if (tuNgay.value && denNgay.value && tuNgay.value > denNgay.value) {
            tuNgay.classList.add('is-invalid');
            denNgay.classList.add('is-invalid');
            tuNgayErrorMsg.textContent = 'Từ ngày không được lớn hơn Đến ngày';
            tuNgayError.classList.remove('d-none');
            hasError = true;
        }

        return !hasError;
    },

    validatePdfDateInputs: function () {
        const tuNgay = document.getElementById('pdfTuNgay');
        const denNgay = document.getElementById('pdfDenNgay');
        const tuNgayError = document.getElementById('pdfTuNgayError');
        const denNgayError = document.getElementById('pdfDenNgayError');
        const tuNgayErrorMsg = document.getElementById('pdfTuNgayErrorMsg');
        const denNgayErrorMsg = document.getElementById('pdfDenNgayErrorMsg');

        let hasError = false;

        // Clear error states
        tuNgay.classList.remove('is-invalid');
        denNgay.classList.remove('is-invalid');
        tuNgayError.classList.add('d-none');
        denNgayError.classList.add('d-none');

        // Check Từ ngày
        if (!tuNgay.value) {
            tuNgay.classList.add('is-invalid');
            tuNgayErrorMsg.textContent = 'Vui lòng chọn ngày bắt đầu';
            tuNgayError.classList.remove('d-none');
            hasError = true;
        }

        // Check Đến ngày
        if (!denNgay.value) {
            denNgay.classList.add('is-invalid');
            denNgayErrorMsg.textContent = 'Vui lòng chọn ngày kết thúc';
            denNgayError.classList.remove('d-none');
            hasError = true;
        }

        // Check date range
        if (tuNgay.value && denNgay.value && tuNgay.value > denNgay.value) {
            tuNgay.classList.add('is-invalid');
            denNgay.classList.add('is-invalid');
            tuNgayErrorMsg.textContent = 'Từ ngày không được lớn hơn Đến ngày';
            tuNgayError.classList.remove('d-none');
            hasError = true;
        }

        return !hasError;
    },

    exportExcel: function () {
        const maXa = document.getElementById('exportMaXaFilter').value;
        const tuNgay = document.getElementById('exportTuNgay').value;
        const denNgay = document.getElementById('exportDenNgay').value;

        // Validate date inputs
        if (!this.validateExcelDateInputs()) {
            return;
        }

        const params = new URLSearchParams();
        if (maXa) params.append('maXaFilter', maXa);
        if (tuNgay) params.append('tuNgay', tuNgay);
        if (denNgay) params.append('denNgay', denNgay);

        const url = `/BaoCaoThongKe/ExportCsv?${params.toString()}`;
        const modal = bootstrap.Modal.getInstance(document.getElementById('exportExcelModal'));
        modal.hide();

        this.showToast('Đang xuất file Excel...', 'info');
        window.location.href = url;

        setTimeout(() => {
            this.showToast('Đã xuất file Excel thành công!', 'success');
        }, 1500);
    },

    exportPdf: function () {
        const maXa = document.getElementById('pdfMaXaFilter').value;
        const tuNgay = document.getElementById('pdfTuNgay').value;
        const denNgay = document.getElementById('pdfDenNgay').value;

        // Validate date inputs
        if (!this.validatePdfDateInputs()) {
            return;
        }

        const params = new URLSearchParams();
        if (maXa) params.append('maXaFilter', maXa);
        if (tuNgay) params.append('tuNgay', tuNgay);
        if (denNgay) params.append('denNgay', denNgay);

        const url = `/BaoCaoThongKe/ExportPdf?${params.toString()}`;
        const modal = bootstrap.Modal.getInstance(document.getElementById('exportPdfModal'));
        modal.hide();

        this.showToast('Đang chuẩn bị file PDF...', 'info');
        window.open(url, '_blank');

        setTimeout(() => {
            this.showToast('Đã mở trang PDF. Sử dụng Ctrl+P để in hoặc lưu thành PDF.', 'success');
        }, 500);
    },

    validateExportDates: function (tuNgay, denNgay) {
        if (!tuNgay || !denNgay) {
            return {
                valid: false,
                message: 'Vui lòng chọn cả Từ ngày và Đến ngày'
            };
        }

        if (tuNgay > denNgay) {
            return {
                valid: false,
                message: 'Từ ngày không được lớn hơn Đến ngày'
            };
        }

        return { valid: true, message: '' };
    },

    slugify: function (text) {
        const from = "àáãảạăằắẳẵặâầấẩẫậèéẻẽẹêềếểễệđùúủũụưừứửữựòóỏõọôồốổỗộơờớởỡợìíỉĩịäëïîöüûñçýỳỹỵỷ";
        const to = "aaaaaaaaaaaaaaaaaeeeeeeeeeeeduuuuuuuuuuuoooooooooooooooooiiiiiaeiiouuncyyyyy";

        for (let i = 0, l = from.length; i < l; i++) {
            text = text.replace(new RegExp(from[i], "gi"), to[i]);
        }

        return text.toString().toLowerCase()
            .replace(/\s+/g, '')
            .replace(/[^\w\-]+/g, '')
            .replace(/\-\-+/g, '')
            .replace(/^-+/, '')
            .replace(/-+$/, '');
    },

    formatDate: function (date) {
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        return `${year}${month}${day}`;
    },

    formatDateTime: function (date) {
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        const hours = String(date.getHours()).padStart(2, '0');
        const minutes = String(date.getMinutes()).padStart(2, '0');
        const seconds = String(date.getSeconds()).padStart(2, '0');
        return `${year}${month}${day}_${hours}${minutes}${seconds}`;
    },

    initDateValidation: function () {
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
                    this.showToast('Đến ngày không được lớn hơn Từ ngày', 'warning');
                    exportDenNgay.value = '';
                }
                if (exportDenNgay.value) {
                    exportTuNgay.setAttribute('max', exportDenNgay.value);
                }
            });
        }

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
                    this.showToast('Đến ngày không được lớn hơn Đến ngày', 'warning');
                    pdfDenNgay.value = '';
                }
                if (pdfDenNgay.value) {
                    pdfTuNgay.setAttribute('max', pdfDenNgay.value);
                }
            });
        }

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
                    this.showToast('Đến ngày không được lớn hơn Từ ngày', 'warning');
                    denNgayInput.value = '';
                }
                if (denNgayInput.value) {
                    tuNgayInput.setAttribute('max', denNgayInput.value);
                }
            });
        }
    },

    initFilterForm: function () {
        const filterForm = document.querySelector('.digioi-filter-form');
        if (filterForm) {
            filterForm.addEventListener('submit', () => {
                this.showToast('Đang tải dữ liệu...', 'info');
            });
        }
    },

    showToast: function (message, type = 'info') {
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

        let toastContainer = document.querySelector('.toast-container');
        if (!toastContainer) {
            toastContainer = document.createElement('div');
            toastContainer.className = 'toast-container position-fixed top-0 end-0 p-3';
            toastContainer.style.zIndex = '9999';
            document.body.appendChild(toastContainer);
        }

        toastContainer.insertAdjacentHTML('beforeend', toastHtml);
        const toastElement = toastContainer.lastElementChild;

        const toast = new bootstrap.Toast(toastElement, {
            autohide: true,
            delay: 3000
        });
        toast.show();

        toastElement.addEventListener('hidden.bs.toast', () => {
            toastElement.remove();
        });
    }
};

document.addEventListener('DOMContentLoaded', function () {
    BaoCaoThongKe.init();
});