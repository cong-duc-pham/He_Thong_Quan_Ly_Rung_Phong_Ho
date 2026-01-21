// JS hỗ trợ Nhật Ký Bảo Vệ
(function ($) {
    function setNowIfEmpty() {
        var input = $('#NgayGhi');
        if (input.length && !input.val()) {
            var now = new Date();
            var iso = new Date(now.getTime() - now.getTimezoneOffset() * 60000)
                .toISOString()
                .slice(0, 16);
            input.val(iso);
        }
    }

    function bindGpsButton() {
        var btn = $('#btnGetGps');
        var target = $('#ToaDoGPS');
        if (!btn.length || !target.length) return;

        btn.on('click', function (e) {
            e.preventDefault();
            if (!navigator.geolocation) {
                alert('Trình duyệt không hỗ trợ GPS');
                return;
            }
            btn.prop('disabled', true).text('Đang lấy...');
            navigator.geolocation.getCurrentPosition(function (pos) {
                var lat = pos.coords.latitude.toFixed(6);
                var lng = pos.coords.longitude.toFixed(6);
                target.val(lat + ',' + lng);
                btn.prop('disabled', false).html('<i class="bi bi-crosshair"></i>');
            }, function () {
                alert('Không thể lấy tọa độ. Vui lòng bật GPS.');
                btn.prop('disabled', false).html('<i class="bi bi-crosshair"></i>');
            });
        });
    }

    function markActiveFilters() {
        var fields = ['fromDate', 'toDate', 'maLo', 'maNV', 'loaiSuViec', 'keyword'];
        fields.forEach(function (name) {
            var el = $('[name="' + name + '"]');
            if (el.length && el.val()) {
                el.addClass('border-success border-2');
            }
        });
    }

    $(document).ready(function () {
        var path = window.location.pathname.toLowerCase();
        if (path.includes('/nhatkybaove/create') || path.includes('/nhatkybaove/edit')) {
            setNowIfEmpty();
            bindGpsButton();
        }
        if (path.includes('/nhatkybaove/index') || path.endsWith('/nhatkybaove')) {
            markActiveFilters();
        }
    });
})(jQuery);
