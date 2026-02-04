using System;
using System.Text.RegularExpressions;

namespace QuanLyRungPhongHo.Validators
{
    /// <summary>
    /// Validator cho Qu?n lý Ca Làm Vi?c
    /// ??m b?o d? li?u ca làm vi?c chính xác và không xung ??t
    /// </summary>
    public static class CaLamViecValidator
    {
        // Constants
        private const int MIN_SHIFT_NAME_LENGTH = 3;
        private const int MAX_SHIFT_NAME_LENGTH = 100;
        private const int MAX_DESCRIPTION_LENGTH = 500;

        // Pattern cho th?i gian HH:mm
        private const string TimePattern = @"^([0-1][0-9]|2[0-3]):([0-5][0-9])$";

        // Tên ca ph? bi?n
        private static readonly string[] CommonShiftNames = 
        {
            "Ca Sáng", "Ca Chi?u", "Ca T?i", "Ca ?êm",
            "Ca Hành Chính", "Ca Tr?c", "Ca Tu?n Tra"
        };

        /// <summary>
        /// Validate toàn b? thông tin ca làm vi?c
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateCaLamViec(
            string tenCa,
            string gioBatDau,
            string gioKetThuc,
            string moTa = null)
        {
            // Validate tên ca
            var tenCaResult = ValidateTenCa(tenCa);
            if (!tenCaResult.IsValid)
                return tenCaResult;

            // Validate gi? b?t ??u
            var gioBatDauResult = ValidateGio(gioBatDau, "Gi? b?t ??u");
            if (!gioBatDauResult.IsValid)
                return gioBatDauResult;

            // Validate gi? k?t thúc
            var gioKetThucResult = ValidateGio(gioKetThuc, "Gi? k?t thúc");
            if (!gioKetThucResult.IsValid)
                return gioKetThucResult;

            // Validate logic gi? b?t ??u và k?t thúc
            var timeLogicResult = ValidateTimeLogic(gioBatDau, gioKetThuc);
            if (!timeLogicResult.IsValid)
                return timeLogicResult;

            // Validate mô t? (n?u có)
            if (!string.IsNullOrWhiteSpace(moTa))
            {
                var moTaResult = ValidateMoTa(moTa);
                if (!moTaResult.IsValid)
                    return moTaResult;
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Validate tên ca làm vi?c
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateTenCa(string tenCa)
        {
            if (string.IsNullOrWhiteSpace(tenCa))
                return (false, "Tên ca không ???c ?? tr?ng!");

            tenCa = tenCa.Trim();

            // Ki?m tra ?? dài
            if (tenCa.Length < MIN_SHIFT_NAME_LENGTH)
                return (false, $"Tên ca ph?i có ít nh?t {MIN_SHIFT_NAME_LENGTH} ký t?!");

            if (tenCa.Length > MAX_SHIFT_NAME_LENGTH)
                return (false, $"Tên ca không ???c v??t quá {MAX_SHIFT_NAME_LENGTH} ký t?!");

            // Ki?m tra không ch?a ký t? ??c bi?t nguy hi?m
            if (Regex.IsMatch(tenCa, @"[<>""'%;()&+]"))
                return (false, "Tên ca không ???c ch?a các ký t? ??c bi?t: < > \" ' % ; ( ) & +");

            // Ki?m tra không ch?a nhi?u kho?ng tr?ng liên ti?p
            if (Regex.IsMatch(tenCa, @"\s{2,}"))
                return (false, "Tên ca không ???c ch?a nhi?u kho?ng tr?ng liên ti?p!");

            return (true, string.Empty);
        }

        /// <summary>
        /// Validate gi? (??nh d?ng HH:mm)
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateGio(string gio, string fieldName = "Gi?")
        {
            if (string.IsNullOrWhiteSpace(gio))
                return (false, $"{fieldName} không ???c ?? tr?ng!");

            gio = gio.Trim();

            // Ki?m tra ??nh d?ng HH:mm
            if (!Regex.IsMatch(gio, TimePattern))
                return (false, $"{fieldName} ph?i ?úng ??nh d?ng HH:mm (VD: 08:00, 14:30)!");

            // Parse ?? ki?m tra h?p l?
            try
            {
                var parts = gio.Split(':');
                var hour = int.Parse(parts[0]);
                var minute = int.Parse(parts[1]);

                if (hour < 0 || hour > 23)
                    return (false, $"{fieldName} không h?p l?! Gi? ph?i t? 00 ??n 23.");

                if (minute < 0 || minute > 59)
                    return (false, $"{fieldName} không h?p l?! Phút ph?i t? 00 ??n 59.");
            }
            catch
            {
                return (false, $"{fieldName} không h?p l?!");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Validate logic gi? b?t ??u và k?t thúc
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateTimeLogic(string gioBatDau, string gioKetThuc)
        {
            try
            {
                var startTime = ParseTime(gioBatDau);
                var endTime = ParseTime(gioKetThuc);

                // Ca trong cùng ngày
                if (endTime <= startTime)
                {
                    // Ki?m tra n?u là ca qua ?êm (VD: 22:00 - 06:00)
                    if (endTime < startTime)
                    {
                        // Cho phép ca qua ?êm, nh?ng c?nh báo n?u quá dài
                        var duration = (TimeSpan.FromHours(24) - startTime) + endTime;
                        if (duration.TotalHours > 12)
                        {
                            return (false, "Ca làm vi?c không ???c v??t quá 12 ti?ng!");
                        }
                    }
                    else
                    {
                        // Gi? b?t ??u = Gi? k?t thúc
                        return (false, "Gi? k?t thúc ph?i sau gi? b?t ??u!");
                    }
                }
                else
                {
                    // Ca trong cùng ngày
                    var duration = endTime - startTime;

                    // Ki?m tra ?? dài ca t?i thi?u (ít nh?t 30 phút)
                    if (duration.TotalMinutes < 30)
                        return (false, "Ca làm vi?c ph?i có ít nh?t 30 phút!");

                    // Ki?m tra ?? dài ca t?i ?a (không quá 12 ti?ng)
                    if (duration.TotalHours > 12)
                        return (false, "Ca làm vi?c không ???c v??t quá 12 ti?ng!");
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"L?i ki?m tra th?i gian: {ex.Message}");
            }
        }

        /// <summary>
        /// Validate mô t? ca làm vi?c
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateMoTa(string moTa)
        {
            if (string.IsNullOrWhiteSpace(moTa))
                return (true, string.Empty); // Mô t? không b?t bu?c

            moTa = moTa.Trim();

            if (moTa.Length > MAX_DESCRIPTION_LENGTH)
                return (false, $"Mô t? không ???c v??t quá {MAX_DESCRIPTION_LENGTH} ký t?!");

            // Ki?m tra không ch?a ký t? nguy hi?m
            if (Regex.IsMatch(moTa, @"[<>""']"))
                return (false, "Mô t? không ???c ch?a các ký t?: < > \" '");

            return (true, string.Empty);
        }

        /// <summary>
        /// Parse chu?i th?i gian thành TimeSpan
        /// </summary>
        public static TimeSpan ParseTime(string time)
        {
            if (string.IsNullOrWhiteSpace(time))
                throw new ArgumentException("Th?i gian không ???c ?? tr?ng!");

            var parts = time.Trim().Split(':');
            if (parts.Length != 2)
                throw new ArgumentException("??nh d?ng th?i gian không h?p l?!");

            var hour = int.Parse(parts[0]);
            var minute = int.Parse(parts[1]);

            return new TimeSpan(hour, minute, 0);
        }

        /// <summary>
        /// Format TimeSpan thành chu?i HH:mm
        /// </summary>
        public static string FormatTime(TimeSpan time)
        {
            return time.ToString(@"hh\:mm");
        }

        /// <summary>
        /// Chu?n hóa tên ca (Vi?t hoa ch? cái ??u)
        /// </summary>
        public static string NormalizeTenCa(string tenCa)
        {
            if (string.IsNullOrWhiteSpace(tenCa))
                return string.Empty;

            // Lo?i b? kho?ng tr?ng th?a
            tenCa = Regex.Replace(tenCa.Trim(), @"\s+", " ");

            // Vi?t hoa ch? cái ??u m?i t?
            var words = tenCa.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (!string.IsNullOrEmpty(words[i]))
                {
                    words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
                }
            }

            return string.Join(" ", words);
        }

        /// <summary>
        /// Tính ?? dài ca làm vi?c (gi?)
        /// </summary>
        public static double CalculateDuration(string gioBatDau, string gioKetThuc)
        {
            try
            {
                var startTime = ParseTime(gioBatDau);
                var endTime = ParseTime(gioKetThuc);

                if (endTime < startTime)
                {
                    // Ca qua ?êm
                    var duration = (TimeSpan.FromHours(24) - startTime) + endTime;
                    return duration.TotalHours;
                }
                else
                {
                    // Ca trong ngày
                    var duration = endTime - startTime;
                    return duration.TotalHours;
                }
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// G?i ý tên ca d?a trên gi? b?t ??u
        /// </summary>
        public static string SuggestShiftName(string gioBatDau)
        {
            try
            {
                var time = ParseTime(gioBatDau);
                var hour = time.Hours;

                if (hour >= 6 && hour < 12)
                    return "Ca Sáng";
                else if (hour >= 12 && hour < 18)
                    return "Ca Chi?u";
                else if (hour >= 18 && hour < 22)
                    return "Ca T?i";
                else
                    return "Ca ?êm";
            }
            catch
            {
                return "Ca Làm Vi?c";
            }
        }
    }
}
