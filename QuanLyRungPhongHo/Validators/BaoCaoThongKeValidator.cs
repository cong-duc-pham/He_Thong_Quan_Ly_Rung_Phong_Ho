namespace QuanLyRungPhongHo.Validators
{
    /// <summary>
    /// Lớp validator chuyên nghiệp cho nghiệp vụ Báo cáo thống kê
    /// Đảm bảo các tham số báo cáo hợp lệ và logic nghiệp vụ chính xác
    /// </summary>
    public static class BaoCaoThongKeValidator
    {
        #region Constants

        // Giới hạn khoảng thời gian báo cáo
        private const int MAX_REPORT_DAYS = 730; // Tối đa 2 năm
        private const int MIN_REPORT_DAYS = 1;   // Tối thiểu 1 ngày

        // Giới hạn thời gian so với hiện tại
        private const int MAX_FUTURE_DAYS = 0;   // Không cho phép ngày tương lai
        private const int MAX_PAST_YEARS = 10;   // Tối đa 10 năm về trước

        #endregion

        #region Date Validation Methods

        /// <summary>
        /// Validate khoảng thời gian báo cáo (Từ ngày - Đến ngày)
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateDateRange(DateTime? tuNgay, DateTime? denNgay)
        {
            // Nếu cả 2 đều null thì hợp lệ (sẽ dùng giá trị mặc định)
            if (!tuNgay.HasValue && !denNgay.HasValue)
                return (true, string.Empty);

            // Nếu chỉ có 1 giá trị
            if (tuNgay.HasValue && !denNgay.HasValue)
                return (false, "Vui lòng chọn 'Đến ngày'!");

            if (!tuNgay.HasValue && denNgay.HasValue)
                return (false, "Vui lòng chọn 'Từ ngày'!");

            // Validate từng ngày
            var tuNgayValidation = ValidateSingleDate(tuNgay.Value, "Từ ngày");
            if (!tuNgayValidation.IsValid)
                return tuNgayValidation;

            var denNgayValidation = ValidateSingleDate(denNgay.Value, "Đến ngày");
            if (!denNgayValidation.IsValid)
                return denNgayValidation;

            // Kiểm tra logic: Từ ngày <= Đến ngày
            if (tuNgay.Value > denNgay.Value)
                return (false, "'Từ ngày' không được lớn hơn 'Đến ngày'!");

            // Kiểm tra không được chọn cùng 1 ngày nếu muốn báo cáo chi tiết
            // (Tùy nghiệp vụ, có thể cho phép)

            // Kiểm tra khoảng thời gian không quá dài
            var daysDiff = (denNgay.Value - tuNgay.Value).Days;
            if (daysDiff > MAX_REPORT_DAYS)
                return (false, $"Khoảng thời gian báo cáo không được vượt quá {MAX_REPORT_DAYS} ngày ({MAX_REPORT_DAYS / 365} năm)!");

            // Kiểm tra khoảng thời gian tối thiểu
            if (daysDiff < MIN_REPORT_DAYS)
                return (false, $"Khoảng thời gian báo cáo phải ít nhất {MIN_REPORT_DAYS} ngày!");

            return (true, string.Empty);
        }

        /// <summary>
        /// Validate 1 ngày cụ thể
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateSingleDate(DateTime date, string fieldName = "Ngày")
        {
            var today = DateTime.Today;

            // Không cho phép ngày trong tương lai
            if (date > today.AddDays(MAX_FUTURE_DAYS))
                return (false, $"{fieldName} không được vượt quá ngày hiện tại!");

            // Không cho phép ngày quá xa trong quá khứ
            var minDate = today.AddYears(-MAX_PAST_YEARS);
            if (date < minDate)
                return (false, $"{fieldName} không được quá {MAX_PAST_YEARS} năm trước!");

            // Kiểm tra năm hợp lệ (tránh lỗi DateTime)
            if (date.Year < 1900 || date.Year > 2100)
                return (false, $"{fieldName} có năm không hợp lệ! (Phải từ 1900-2100)");

            return (true, string.Empty);
        }

        /// <summary>
        /// Validate tháng/năm cho báo cáo theo tháng
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateMonthYear(int month, int year)
        {
            // Validate tháng (1-12)
            if (month < 1 || month > 12)
                return (false, "Tháng phải từ 1 đến 12!");

            // Validate năm
            var currentYear = DateTime.Now.Year;
            if (year < currentYear - MAX_PAST_YEARS || year > currentYear)
                return (false, $"Năm không hợp lệ! (Phải từ {currentYear - MAX_PAST_YEARS} đến {currentYear})");

            // Kiểm tra không được chọn tháng trong tương lai
            if (year == currentYear && month > DateTime.Now.Month)
                return (false, "Không thể chọn tháng trong tương lai!");

            return (true, string.Empty);
        }

        /// <summary>
        /// Validate quý cho báo cáo theo quý
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateQuarterYear(int quarter, int year)
        {
            // Validate quý (1-4)
            if (quarter < 1 || quarter > 4)
                return (false, "Quý phải từ 1 đến 4!");

            // Validate năm
            var currentYear = DateTime.Now.Year;
            if (year < currentYear - MAX_PAST_YEARS || year > currentYear)
                return (false, $"Năm không hợp lệ! (Phải từ {currentYear - MAX_PAST_YEARS} đến {currentYear})");

            // Kiểm tra không được chọn quý trong tương lai
            if (year == currentYear)
            {
                var currentQuarter = (DateTime.Now.Month - 1) / 3 + 1;
                if (quarter > currentQuarter)
                    return (false, "Không thể chọn quý trong tương lai!");
            }

            return (true, string.Empty);
        }

        #endregion

        #region Filter Validation Methods

        /// <summary>
        /// Validate mã xã (filter)
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateMaXaFilter(string? maXa)
        {
            // Mã xã filter có thể null/empty (nghĩa là không filter)
            if (string.IsNullOrWhiteSpace(maXa))
                return (true, string.Empty);

            maXa = maXa.Trim();

            // Validate định dạng
            if (maXa.Length < 2 || maXa.Length > 20)
                return (false, "Mã xã không hợp lệ!");

            // Không chứa ký tự đặc biệt nguy hiểm (SQL Injection, XSS)
            if (maXa.Contains("'") || maXa.Contains("\"") || maXa.Contains("<") ||
                maXa.Contains(">") || maXa.Contains(";") || maXa.Contains("--"))
                return (false, "Mã xã chứa ký tự không hợp lệ!");

            return (true, string.Empty);
        }

        /// <summary>
        /// Validate loại báo cáo
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateReportType(string? reportType)
        {
            if (string.IsNullOrWhiteSpace(reportType))
                return (false, "Vui lòng chọn loại báo cáo!");

            var validTypes = new[] { "TongQuan", "LoaiRung", "TrangThai", "Xa", "SuKien", "SinhVat", "NhanSu" };

            if (!validTypes.Contains(reportType.Trim()))
                return (false, $"Loại báo cáo không hợp lệ! Các loại hợp lệ: {string.Join(", ", validTypes)}");

            return (true, string.Empty);
        }

        /// <summary>
        /// Validate định dạng xuất file (CSV, Excel, PDF)
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateExportFormat(string? format)
        {
            if (string.IsNullOrWhiteSpace(format))
                return (false, "Vui lòng chọn định dạng xuất file!");

            format = format.Trim().ToUpper();

            var validFormats = new[] { "CSV", "EXCEL", "XLSX", "PDF" };

            if (!validFormats.Contains(format))
                return (false, $"Định dạng xuất file không hợp lệ! Các định dạng hợp lệ: {string.Join(", ", validFormats)}");

            return (true, string.Empty);
        }

        #endregion

        #region Business Logic Validation

        /// <summary>
        /// Validate nghiệp vụ: Kiểm tra có dữ liệu trong khoảng thời gian không
        /// (Cần kết hợp với database context)
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateHasData(int recordCount, string entityName = "dữ liệu")
        {
            if (recordCount <= 0)
                return (false, $"Không có {entityName} trong khoảng thời gian này để tạo báo cáo!");

            return (true, string.Empty);
        }

        /// <summary>
        /// Validate số lượng bản ghi xuất (tránh quá tải)
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateExportRecordCount(int recordCount, int maxRecords = 100000)
        {
            if (recordCount <= 0)
                return (false, "Không có dữ liệu để xuất!");

            if (recordCount > maxRecords)
                return (false, $"Số lượng bản ghi vượt quá giới hạn ({recordCount:N0} > {maxRecords:N0})! Vui lòng thu hẹp phạm vi báo cáo.");

            return (true, string.Empty);
        }

        /// <summary>
        /// Validate top N (giới hạn số bản ghi hiển thị)
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateTopN(int? topN)
        {
            if (!topN.HasValue)
                return (true, string.Empty); // Null là hợp lệ (không giới hạn)

            if (topN.Value <= 0)
                return (false, "Số lượng bản ghi phải lớn hơn 0!");

            if (topN.Value > 1000)
                return (false, "Số lượng bản ghi tối đa là 1000!");

            return (true, string.Empty);
        }

        /// <summary>
        /// Validate page number và page size cho phân trang
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidatePagination(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                return (false, "Số trang phải lớn hơn 0!");

            if (pageSize < 1)
                return (false, "Kích thước trang phải lớn hơn 0!");

            if (pageSize > 1000)
                return (false, "Kích thước trang tối đa là 1000!");

            return (true, string.Empty);
        }

        #endregion

        #region Search/Filter Validation

        /// <summary>
        /// Validate chuỗi tìm kiếm (chống SQL Injection, XSS)
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateSearchString(string? searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return (true, string.Empty); // Empty search là hợp lệ

            searchString = searchString.Trim();

            // Kiểm tra độ dài
            if (searchString.Length > 200)
                return (false, "Chuỗi tìm kiếm không được vượt quá 200 ký tự!");

            // Kiểm tra ký tự nguy hiểm (SQL Injection)
            var dangerousPatterns = new[] { "'", "\"", ";", "--", "/*", "*/", "xp_", "sp_", "exec", "execute", "script", "<", ">" };

            foreach (var pattern in dangerousPatterns)
            {
                if (searchString.ToLower().Contains(pattern))
                    return (false, $"Chuỗi tìm kiếm chứa ký tự hoặc từ khóa không hợp lệ: '{pattern}'");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Validate danh sách mã xã (cho filter nhiều xã)
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateMaXaList(List<string>? maXaList)
        {
            if (maXaList == null || maXaList.Count == 0)
                return (true, string.Empty); // Empty list là hợp lệ

            // Giới hạn số lượng
            if (maXaList.Count > 100)
                return (false, "Không thể chọn quá 100 xã cùng lúc!");

            // Validate từng mã
            foreach (var maXa in maXaList)
            {
                var validation = ValidateMaXaFilter(maXa);
                if (!validation.IsValid)
                    return validation;
            }

            return (true, string.Empty);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Chuẩn hóa khoảng thời gian (set time về 00:00:00 và 23:59:59)
        /// </summary>
        public static (DateTime TuNgay, DateTime DenNgay) NormalizeDateRange(DateTime tuNgay, DateTime denNgay)
        {
            // Từ ngày: set về 00:00:00
            var normalizedTuNgay = tuNgay.Date;

            // Đến ngày: set về 23:59:59
            var normalizedDenNgay = denNgay.Date.AddDays(1).AddTicks(-1);

            return (normalizedTuNgay, normalizedDenNgay);
        }

        /// <summary>
        /// Lấy khoảng thời gian mặc định nếu không có input
        /// </summary>
        public static (DateTime TuNgay, DateTime DenNgay) GetDefaultDateRange(int defaultDays = 30)
        {
            var denNgay = DateTime.Today;
            var tuNgay = denNgay.AddDays(-defaultDays);

            return (tuNgay, denNgay);
        }

        /// <summary>
        /// Validate và chuẩn hóa khoảng thời gian (kết hợp)
        /// </summary>
        public static (bool IsValid, string ErrorMessage, DateTime TuNgay, DateTime DenNgay) ValidateAndNormalizeDateRange(
            DateTime? tuNgay, DateTime? denNgay, int defaultDays = 30)
        {
            // Sử dụng giá trị mặc định nếu null
            if (!tuNgay.HasValue || !denNgay.HasValue)
            {
                var defaultRange = GetDefaultDateRange(defaultDays);
                tuNgay = defaultRange.TuNgay;
                denNgay = defaultRange.DenNgay;
            }

            // Validate
            var validation = ValidateDateRange(tuNgay, denNgay);
            if (!validation.IsValid)
                return (false, validation.ErrorMessage, DateTime.MinValue, DateTime.MinValue);

            // Chuẩn hóa
            var normalized = NormalizeDateRange(tuNgay.Value, denNgay.Value);

            return (true, string.Empty, normalized.TuNgay, normalized.DenNgay);
        }

        #endregion

        #region Advanced Validation

        /// <summary>
        /// Validate tham số biểu đồ (chart type, dimensions)
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateChartParameters(string? chartType, int? width, int? height)
        {
            if (!string.IsNullOrWhiteSpace(chartType))
            {
                var validChartTypes = new[] { "bar", "line", "pie", "doughnut", "area", "column" };
                if (!validChartTypes.Contains(chartType.ToLower()))
                    return (false, $"Loại biểu đồ không hợp lệ! Các loại hợp lệ: {string.Join(", ", validChartTypes)}");
            }

            if (width.HasValue && (width.Value < 100 || width.Value > 5000))
                return (false, "Chiều rộng biểu đồ phải từ 100 đến 5000 pixels!");

            if (height.HasValue && (height.Value < 100 || height.Value > 5000))
                return (false, "Chiều cao biểu đồ phải từ 100 đến 5000 pixels!");

            return (true, string.Empty);
        }

        /// <summary>
        /// Validate tham số group by (nhóm theo)
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateGroupBy(string? groupBy)
        {
            if (string.IsNullOrWhiteSpace(groupBy))
                return (true, string.Empty);

            var validGroupBy = new[] { "Ngay", "Tuan", "Thang", "Quy", "Nam", "Xa", "Thon", "LoaiRung", "TrangThai" };

            if (!validGroupBy.Contains(groupBy.Trim()))
                return (false, $"Tiêu chí nhóm không hợp lệ! Các tiêu chí hợp lệ: {string.Join(", ", validGroupBy)}");

            return (true, string.Empty);
        }

        #endregion
    }
}
