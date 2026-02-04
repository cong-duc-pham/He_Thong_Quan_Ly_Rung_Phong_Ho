using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyRungPhongHo.Models
{
    public class TaiKhoan
    {
        [Key]
        public int MaTK { get; set; } // IDENTITY

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Tên đăng nhập từ 5-50 ký tự")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", 
            ErrorMessage = "Tên đăng nhập chỉ chứa chữ, số và gạch dưới")]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; } = null!;

        [Required(ErrorMessage = "Quyền không được để trống")]
        [StringLength(50)]
        [RegularExpression(@"^(Admin_Tinh|QuanLy_Xa|Kiem_Lam)$", 
            ErrorMessage = "Quyền không hợp lệ")]
        [Display(Name = "Quyền hạn")]
        public string Quyen { get; set; } = "Kiem_Lam";

        [ForeignKey(nameof(NhanSu))]
        public int? MaNV { get; set; }

        [Required]
        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true; // true = Kích hoạt, false = Khóa

        // Navigation property
        public NhanSu? NhanSu { get; set; }
    }
}