using System.ComponentModel.DataAnnotations;

namespace QuanLyRungPhongHo.Models
{
    public class DanhMucThon
    {
        [Key]
        [Required(ErrorMessage = "Mã thôn không được để trống")]
        [StringLength(10, ErrorMessage = "Mã thôn không được vượt quá 10 ký tự")]
        [Display(Name = "Mã Thôn/Bản")]
        public string MaThon { get; set; } = null!;

        [Required(ErrorMessage = "Tên thôn không được để trống")]
        [StringLength(100, ErrorMessage = "Tên thôn không được vượt quá 100 ký tự")]
        [Display(Name = "Tên Thôn/Bản")]
        public string TenThon { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng chọn xã")]
        [StringLength(10)]
        [Display(Name = "Xã")]
        public string MaXa { get; set; } = null!;

        // Navigation properties
        public DanhMucXa DanhMucXa { get; set; } = null!;
        public ICollection<LoRung> LoRungs { get; set; } = new List<LoRung>();
    }
}