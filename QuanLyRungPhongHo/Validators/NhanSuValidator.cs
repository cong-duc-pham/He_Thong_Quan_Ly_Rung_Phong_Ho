using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;

namespace QuanLyRungPhongHo.Validators
{
    /// <summary>
    /// Lớp validator chuyên nghiệp cho nghiệp vụ Quản lý nhân sự
    /// Đảm bảo dữ liệu sạch, chính xác và phù hợp với thực tế nghiệp vụ
    /// </summary>
    public static class NhanSuValidator
    {
        #region Validation Rules Constants

        // Danh sách chức vụ hợp lệ trong hệ thống quản lý rừng
        private static readonly HashSet<string> ChucVuHopLe = new()
        {
            "Kiểm lâm",
            "Phó Kiểm lâm",
            "Trưởng trạm",
            "Phó trạm",
            "Nhân viên bảo vệ rừng",
            "Cán bộ kỹ thuật",
            "Hướng dẫn viên"
        };

        // Danh sách quyền hệ thống hợp lệ
        private static readonly HashSet<string> QuyenHopLe = new()
        {
            "Admin_Tinh",
            "QuanLy_Xa",
            "Kiem_Lam"
        };

        // Regex patterns
        private const string VietnamPhonePattern = @"^(0|\+84)(3[2-9]|5[6|8|9]|7[0|6-9]|8[1-9]|9[0-9])[0-9]{7}$";
        private const string UsernamePattern = @"^[a-zA-Z0-9_]{5,50}$";
        private const string VietnameseNamePattern = @"^[a-zA-ZÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚĂĐĨŨƠàáâãèéêìíòóôõùúăđĩũơƯĂẠẢẤẦẨẪẬẮẰẲẴẶẸẺẼỀỀỂưăạảấầẩẫậắằẳẵặẹẻẽềềểỄỆỈỊỌỎỐỒỔỖỘỚỜỞỠỢỤỦỨỪễệỉịọỏốồổỗộớờởỡợụủứừỬỮỰỲỴÝỶỸửữựỳỵýỷỹ\s]{2,100}$";

        // Tuổi hợp lệ cho nhân viên (18-65 tuổi)
        private const int MIN_AGE = 18;
        private const int MAX_AGE = 65;

        // Độ dài giới hạn
        private const int MIN_PASSWORD_LENGTH = 6;
        private const int MAX_PASSWORD_LENGTH = 100;
        private const int MIN_USERNAME_LENGTH = 5;
        private const int MAX_USERNAME_LENGTH = 50;

        #endregion

        #region Validation Methods

        /// <summary>
        /// Validate họ tên nhân sự
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateHoTen(string? hoTen)
        {
            if (string.IsNullOrWhiteSpace(hoTen))
                return (false, "Họ tên không được để trống!");

            hoTen = hoTen.Trim();

            if (hoTen.Length < 2)
                return (false, "Họ tên phải có ít nhất 2 ký tự!");

            if (hoTen.Length > 100)
                return (false, "Họ tên không được vượt quá 100 ký tự!");

            if (!Regex.IsMatch(hoTen, VietnameseNamePattern))
                return (false, "Họ tên chỉ được chứa chữ cái tiếng Việt và khoảng trắng!");

            // Kiểm tra không chứa số hoặc ký tự đặc biệt
            if (Regex.IsMatch(hoTen, @"[0-9!@#$%^&*()_+=\[\]{};:'""\\|,.<>?/~`]"))
                return (false, "Họ tên không được chứa số hoặc ký tự đặc biệt!");

            // Kiểm tra không có nhiều khoảng trắng liên tiếp
            if (Regex.IsMatch(hoTen, @"\s{2,}"))
                return (false, "Họ tên không được chứa nhiều khoảng trắng liên tiếp!");

            // Kiểm tra phải có ít nhất 2 từ (họ và tên)
            var words = hoTen.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length < 2)
                return (false, "Họ tên phải có ít nhất 2 từ (Họ và Tên)!");

            return (true, string.Empty);
        }

        /// <summary>
        /// Validate chức vụ
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateChucVu(string? chucVu)
        {
            if (string.IsNullOrWhiteSpace(chucVu))
                return (false, "Vui lòng chọn chức vụ!");

            chucVu = chucVu.Trim();

            if (!ChucVuHopLe.Contains(chucVu))
                return (false, $"Chức vụ không hợp lệ! Các chức vụ hợp lệ: {string.Join(", ", ChucVuHopLe)}");

            return (true, string.Empty);
        }

        /// <summary>
        /// Validate số điện thoại Việt Nam (rất nghiêm ngặt)
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateSDT(string? sdt)
        {
            if (string.IsNullOrWhiteSpace(sdt))
                return (false, "Số điện thoại không được để trống!");

            // Loại bỏ khoảng trắng, dấu gạch ngang, dấu chấm
            sdt = Regex.Replace(sdt, @"[\s\-\.]", "");

            // Kiểm tra chỉ chứa số và dấu +
            if (!Regex.IsMatch(sdt, @"^[\+0-9]+$"))
                return (false, "Số điện thoại chỉ được chứa số và dấu +!");

            // Chuẩn hóa về định dạng Việt Nam
            if (sdt.StartsWith("+84"))
                sdt = "0" + sdt.Substring(3);
            else if (sdt.StartsWith("84"))
                sdt = "0" + sdt.Substring(2);

            // Validate theo pattern Việt Nam
            if (!Regex.IsMatch(sdt, VietnamPhonePattern))
                return (false, "Số điện thoại không đúng định dạng Việt Nam! (VD: 0912345678, 0987654321)");

            // Kiểm tra độ dài chính xác (10 số)
            if (sdt.Length != 10)
                return (false, "Số điện thoại Việt Nam phải có đúng 10 chữ số!");

            // Kiểm tra đầu số hợp lệ
            var dauSo = sdt.Substring(0, 3);
            var danhSachDauSoHopLe = new[] { "032", "033", "034", "035", "036", "037", "038", "039",
                                              "056", "058", "059",
                                              "070", "076", "077", "078", "079",
                                              "081", "082", "083", "084", "085", "086", "087", "088", "089",
                                              "090", "091", "092", "093", "094", "096", "097", "098", "099" };

            if (!danhSachDauSoHopLe.Contains(dauSo))
                return (false, $"Đầu số '{dauSo}' không hợp lệ! Vui lòng kiểm tra lại.");

            return (true, string.Empty);
        }

        /// <summary>
        /// Validate email (nếu có nhập)
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateEmail(string? email)
        {
            // Email không bắt buộc
            if (string.IsNullOrWhiteSpace(email))
                return (true, string.Empty);

            email = email.Trim().ToLower();

            // Kiểm tra độ dài
            if (email.Length > 100)
                return (false, "Email không được vượt quá 100 ký tự!");

            // Kiểm tra định dạng cơ bản
            var emailAttribute = new EmailAddressAttribute();
            if (!emailAttribute.IsValid(email))
                return (false, "Email không đúng định dạng! (VD: example@domain.com)");

            // Kiểm tra regex chi tiết hơn
            var emailPattern = @"^[a-zA-Z0-9]([a-zA-Z0-9._-]*[a-zA-Z0-9])?@[a-zA-Z0-9]([a-zA-Z0-9-]*[a-zA-Z0-9])?(\.[a-zA-Z]{2,})+$";
            if (!Regex.IsMatch(email, emailPattern))
                return (false, "Email không hợp lệ! Vui lòng kiểm tra lại.");

            // Kiểm tra domain phổ biến (chống email spam/giả)
            var domain = email.Split('@')[1];
            if (domain.Length < 3)
                return (false, "Domain email không hợp lệ!");

            // Kiểm tra không chứa ký tự đặc biệt không hợp lệ
            if (email.Contains("..") || email.StartsWith(".") || email.EndsWith("."))
                return (false, "Email không được chứa dấu chấm liên tiếp hoặc ở đầu/cuối!");

            return (true, string.Empty);
        }

        /// <summary>
        /// Validate tên đăng nhập
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateTenDangNhap(string? tenDangNhap)
        {
            if (string.IsNullOrWhiteSpace(tenDangNhap))
                return (false, "Tên đăng nhập không được để trống!");

            tenDangNhap = tenDangNhap.Trim();

            // Kiểm tra độ dài
            if (tenDangNhap.Length < MIN_USERNAME_LENGTH)
                return (false, $"Tên đăng nhập phải có ít nhất {MIN_USERNAME_LENGTH} ký tự!");

            if (tenDangNhap.Length > MAX_USERNAME_LENGTH)
                return (false, $"Tên đăng nhập không được vượt quá {MAX_USERNAME_LENGTH} ký tự!");

            // Kiểm tra pattern
            if (!Regex.IsMatch(tenDangNhap, UsernamePattern))
                return (false, "Tên đăng nhập chỉ được chứa chữ cái (a-z, A-Z), số (0-9) và dấu gạch dưới (_)!");

            // Không được bắt đầu hoặc kết thúc bằng dấu gạch dưới
            if (tenDangNhap.StartsWith("_") || tenDangNhap.EndsWith("_"))
                return (false, "Tên đăng nhập không được bắt đầu hoặc kết thúc bằng dấu gạch dưới!");

            // Không được chứa nhiều dấu gạch dưới liên tiếp
            if (tenDangNhap.Contains("__"))
                return (false, "Tên đăng nhập không được chứa nhiều dấu gạch dưới liên tiếp!");

            // Không được là số thuần
            if (Regex.IsMatch(tenDangNhap, @"^\d+$"))
                return (false, "Tên đăng nhập không được chỉ toàn số!");

            // Blacklist tên đăng nhập hệ thống
            var blacklist = new[] { "admin", "root", "system", "administrator", "superuser", "test", "guest" };
            if (blacklist.Contains(tenDangNhap.ToLower()))
                return (false, $"Tên đăng nhập '{tenDangNhap}' không được phép sử dụng!");

            return (true, string.Empty);
        }

        /// <summary>
        /// Validate mật khẩu (khi thêm mới hoặc thay đổi)
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateMatKhau(string? matKhau, bool isRequired = true)
        {
            if (string.IsNullOrWhiteSpace(matKhau))
            {
                if (isRequired)
                    return (false, "Mật khẩu không được để trống!");
                else
                    return (true, string.Empty); // Không bắt buộc khi cập nhật
            }

            // Kiểm tra độ dài
            if (matKhau.Length < MIN_PASSWORD_LENGTH)
                return (false, $"Mật khẩu phải có ít nhất {MIN_PASSWORD_LENGTH} ký tự!");

            if (matKhau.Length > MAX_PASSWORD_LENGTH)
                return (false, $"Mật khẩu không được vượt quá {MAX_PASSWORD_LENGTH} ký tự!");

            // Kiểm tra độ mạnh mật khẩu
            bool hasUpper = Regex.IsMatch(matKhau, @"[A-Z]");
            bool hasLower = Regex.IsMatch(matKhau, @"[a-z]");
            bool hasDigit = Regex.IsMatch(matKhau, @"[0-9]");
            bool hasSpecial = Regex.IsMatch(matKhau, @"[!@#$%^&*()_+=\[\]{};:'""\\|,.<>?/~`-]");

            int criteriaCount = (hasUpper ? 1 : 0) + (hasLower ? 1 : 0) + (hasDigit ? 1 : 0) + (hasSpecial ? 1 : 0);

            if (criteriaCount < 3)
                return (false, "Mật khẩu phải chứa ít nhất 3 trong 4: chữ hoa, chữ thường, số, ký tự đặc biệt!");

            // Kiểm tra không chứa khoảng trắng
            if (matKhau.Contains(" "))
                return (false, "Mật khẩu không được chứa khoảng trắng!");

            // Kiểm tra mật khẩu phổ biến (weak passwords)
            var weakPasswords = new[] { "123456", "password", "12345678", "qwerty", "abc123", "111111", "123123" };
            if (weakPasswords.Any(wp => matKhau.ToLower().Contains(wp)))
                return (false, "Mật khẩu quá phổ biến, vui lòng chọn mật khẩu khác!");

            return (true, string.Empty);
        }

        /// <summary>
        /// Validate quyền hạn
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateQuyen(string? quyen)
        {
            if (string.IsNullOrWhiteSpace(quyen))
                return (false, "Vui lòng chọn quyền hạn!");

            quyen = quyen.Trim();

            if (!QuyenHopLe.Contains(quyen))
                return (false, $"Quyền không hợp lệ! Các quyền hợp lệ: {string.Join(", ", QuyenHopLe)}");

            return (true, string.Empty);
        }

        /// <summary>
        /// Validate mã xã (địa bàn)
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateMaXa(string? maXa)
        {
            if (string.IsNullOrWhiteSpace(maXa))
                return (false, "Vui lòng chọn địa bàn (xã)!");

            maXa = maXa.Trim();

            // Kiểm tra định dạng mã xã (thường là 5-10 ký tự)
            if (maXa.Length < 2 || maXa.Length > 20)
                return (false, "Mã xã không hợp lệ!");

            return (true, string.Empty);
        }

        /// <summary>
        /// Validate logic nghiệp vụ: Quyền phù hợp với chức vụ
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateQuyenVsChucVu(string? quyen, string? chucVu)
        {
            if (string.IsNullOrWhiteSpace(quyen) || string.IsNullOrWhiteSpace(chucVu))
                return (true, string.Empty); // Bỏ qua nếu chưa có đủ dữ liệu

            // Logic nghiệp vụ: Chức vụ cao phải có quyền phù hợp
            if (chucVu == "Trưởng trạm" || chucVu == "Phó trạm")
            {
                if (quyen == "Kiem_Lam")
                    return (false, "Chức vụ Trưởng trạm/Phó trạm phải có quyền QuanLy_Xa trở lên!");
            }

            if (chucVu == "Kiểm lâm" && quyen == "Admin_Tinh")
            {
                return (false, "Chức vụ Kiểm lâm không phù hợp với quyền Admin_Tinh!");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Chuẩn hóa số điện thoại về định dạng chuẩn
        /// </summary>
        public static string NormalizeSDT(string sdt)
        {
            if (string.IsNullOrWhiteSpace(sdt))
                return string.Empty;

            // Loại bỏ khoảng trắng, dấu gạch ngang, dấu chấm
            sdt = Regex.Replace(sdt, @"[\s\-\.]", "");

            // Chuẩn hóa về định dạng 0xxxxxxxxx
            if (sdt.StartsWith("+84"))
                sdt = "0" + sdt.Substring(3);
            else if (sdt.StartsWith("84"))
                sdt = "0" + sdt.Substring(2);

            return sdt;
        }

        /// <summary>
        /// Chuẩn hóa email về chữ thường
        /// </summary>
        public static string NormalizeEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return string.Empty;

            return email.Trim().ToLower();
        }

        /// <summary>
        /// Chuẩn hóa họ tên (viết hoa chữ cái đầu mỗi từ)
        /// </summary>
        public static string NormalizeHoTen(string hoTen)
        {
            if (string.IsNullOrWhiteSpace(hoTen))
                return string.Empty;

            // Loại bỏ khoảng trắng thừa
            hoTen = Regex.Replace(hoTen.Trim(), @"\s+", " ");

            // Viết hoa chữ cái đầu mỗi từ
            var words = hoTen.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (!string.IsNullOrEmpty(words[i]))
                {
                    words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
                }
            }

            return string.Join(" ", words);
        }

        #endregion
    }
}
