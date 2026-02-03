using System.ComponentModel.DataAnnotations;

namespace QuanLyRungPhongHo.Models
{
    public class SettingsViewModel
    {
        [Display(Name = "Họ tên")]
        public string? HoTen { get; set; }

        [Display(Name = "Tài khoản")]
        public string? TenDangNhap { get; set; }

        [Display(Name = "Chức vụ")]
        public string? ChucVu { get; set; }

        [Display(Name = "Quyền")]
        public string? Quyen { get; set; }

        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [Display(Name = "Đơn vị công tác")]
        public string? DonViCongTac { get; set; }

        [Display(Name = "Mã tài khoản")]
        public int? MaTaiKhoan { get; set; }

        [Display(Name = "Mã nhân viên")]
        public int? MaNhanVien { get; set; }
    }
}
