// Script xử lý phân quyền
(function () {
    'use strict';

    console.log('=== PhanQuyen.js loaded ===');

    function initPermissionToggles() {
        console.log('Initializing permission toggles...');
        const checkboxes = document.querySelectorAll('.permission-toggle');
        console.log('Found checkboxes:', checkboxes.length);

        checkboxes.forEach(function (checkbox) {
            checkbox.addEventListener('change', function () {
                const roleName = this.getAttribute('data-role');
                const permissionId = this.getAttribute('data-permission-id');
                const permissionName = this.getAttribute('data-permission-name');
                const isGranted = this.checked;

                console.log('=== Toggle permission ===');
                console.log('Role:', roleName);
                console.log('PermissionId:', permissionId);
                console.log('IsGranted:', isGranted);

                // Disable checkbox khi đang xử lý
                this.disabled = true;

                // Lấy anti-forgery token
                const tokenInput = document.querySelector('#hiddenForm input[name="__RequestVerificationToken"]');
                const token = tokenInput ? tokenInput.value : null;

                console.log('Token found:', token ? 'Yes' : 'No');

                if (!token) {
                    console.error('CSRF Token not found!');
                    showToast('error', 'Lỗi bảo mật: Không tìm thấy token');
                    this.disabled = false;
                    this.checked = !isGranted;
                    return;
                }

                // Gửi AJAX request
                const formData = new FormData();
                formData.append('roleName', roleName);
                formData.append('permissionId', permissionId);
                formData.append('isGranted', isGranted);
                formData.append('__RequestVerificationToken', token);

                fetch('/PhanQuyen/UpdatePermission', {
                    method: 'POST',
                    body: formData
                })
                    .then(response => response.json())
                    .then(data => {
                        console.log('=== AJAX Success ===');
                        console.log('Response:', data);

                        if (data.success) {
                            showToast('success', data.message);
                        } else {
                            // Rollback checkbox nếu lỗi
                            checkbox.checked = !isGranted;
                            showToast('error', data.message);
                        }
                    })
                    .catch(error => {
                        console.error('=== AJAX Error ===');
                        console.error('Error:', error);
                        // Rollback checkbox nếu lỗi
                        checkbox.checked = !isGranted;
                        showToast('error', 'Lỗi khi cập nhật quyền: ' + error.message);
                    })
                    .finally(() => {
                        // Enable lại checkbox
                        checkbox.disabled = false;
                    });
            });
        });
    }

    // Hàm hiển thị toast notification
    function showToast(type, message) {
        const bgColor = type === 'success' ? 'bg-success' : 'bg-danger';
        const icon = type === 'success' ? 'fa-check-circle' : 'fa-exclamation-circle';

        const toastHtml = `
            <div class="toast align-items-center text-white ${bgColor} border-0" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="d-flex">
                    <div class="toast-body">
                        <i class="fas ${icon} me-2"></i>${message}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
            </div>
        `;

        // Tạo container nếu chưa có
        let container = document.getElementById('toast-container');
        if (!container) {
            container = document.createElement('div');
            container.id = 'toast-container';
            container.className = 'position-fixed top-0 end-0 p-3';
            container.style.zIndex = '11000';
            document.body.appendChild(container);
        }

        // Thêm toast
        const tempDiv = document.createElement('div');
        tempDiv.innerHTML = toastHtml.trim();
        const toastElement = tempDiv.firstChild;
        container.appendChild(toastElement);

        // Show toast
        const bsToast = new bootstrap.Toast(toastElement);
        bsToast.show();

        // Xóa toast sau khi ẩn
        toastElement.addEventListener('hidden.bs.toast', function () {
            toastElement.remove();
        });
    }

    // Chạy khi DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initPermissionToggles);
    } else {
        initPermissionToggles();
    }
})();
