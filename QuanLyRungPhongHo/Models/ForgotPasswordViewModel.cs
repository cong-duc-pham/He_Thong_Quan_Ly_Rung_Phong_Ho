using System.ComponentModel.DataAnnotations;

namespace QuanLyRungPhongHo.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ email")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
        [Display(Name = "Địa Chỉ Email")]
        public string Email { get; set; } = null!;
    }

    public class OtpVerificationViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mã OTP")]
        [StringLength(6, MinimumLength = 6, 
            ErrorMessage = "Mã OTP phải có 6 chữ số")]
        [Display(Name = "Mã OTP")]
        public string OtpCode { get; set; } = null!;

        [Required]
        public string Email { get; set; } = null!;
    }

    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [StringLength(100, MinimumLength = 8, 
            ErrorMessage = "Mật khẩu phải từ 8-100 ký tự")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Mật khẩu phải chứa: chữ hoa, chữ thường, số, ký tự đặc biệt (@$!%*?&)")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật Khẩu Mới")]
        public string MatKhauMoi { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác Nhận Mật Khẩu")]
        [Compare("MatKhauMoi", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string XacNhanMatKhau { get; set; } = null!;

        [Required]
        public string Email { get; set; } = null!;
    }
}