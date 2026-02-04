using System.ComponentModel.DataAnnotations;

namespace QuanLyRungPhongHo.Models
{
    public class NhanSuViewModel
    {
        public int MaNV { get; set; }

        // --- VALIDATION CHO BẢNG NHÂN SỰ ---
        [Display(Name = "Họ và tên")]
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ tên không quá 100 ký tự")]
        public string HoTen { get; set; } = null!;

        [Display(Name = "Chức vụ")]
        [Required(ErrorMessage = "Vui lòng chọn chức vụ")]
        public string? ChucVu { get; set; }

        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "SĐT không được để trống")]
        [RegularExpression(@"^(03|05|07|08|09|01[2|6|8|9])+([0-9]{8})$", ErrorMessage = "Số điện thoại không đúng định dạng VN")]
        public string? SDT { get; set; }

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string? Email { get; set; }

        [Display(Name = "Địa bàn (Xã)")]
        [Required(ErrorMessage = "Vui lòng chọn xã")]
        public string? MaXa { get; set; }

        // Trường này chỉ để hiển thị tên xã ra bảng (không cần validate)
        public string? TenXa { get; set; }

        // Trạng thái tài khoản
        public bool TrangThai { get; set; } = true;

        // --- validate bảng tài khaonr
        [Display(Name = "Tên đăng nhập")]
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Tên đăng nhập từ 5-50 ký tự")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Không chứa ký tự đặc biệt")]
        public string? TenDangNhap { get; set; }

        [Display(Name = "Mật khẩu")]
        // Chỉ bắt buộc nhập pass khi Thêm Mới (MaNV = 0)
        // Logic này sẽ xử lý thêm trong Controller hoặc custom validation
        public string? MatKhau { get; set; }

        [Display(Name = "Quyền hạn")]
        public string? Quyen { get; set; }
    }
}