using System.ComponentModel.DataAnnotations;

namespace QuanLyRungPhongHo.Models
{
    public class DanhMucXa
    {
        [Key]
        [Required(ErrorMessage = "Mã xã không được để trống")]
        [StringLength(10, ErrorMessage = "Mã xã không được vượt quá 10 ký tự")]
        [Display(Name = "Mã Xã")]
        public string MaXa { get; set; } = null!;

        [Required(ErrorMessage = "Tên xã không được để trống")]
        [StringLength(100, ErrorMessage = "Tên xã không được vượt quá 100 ký tự")]
        [Display(Name = "Tên Xã")]
        public string TenXa { get; set; } = null!;

        // Navigation properties
        public ICollection<DanhMucThon> DanhMucThons { get; set; } = new List<DanhMucThon>();
        public ICollection<NhanSu> NhanSus { get; set; } = new List<NhanSu>();
    }
}