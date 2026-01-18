using System.ComponentModel.DataAnnotations;

namespace QuanLyRungPhongHo.Models
{
    public class LoRung
    {
        [Key]
        [Display(Name = "Mã Lô")]
        public int MaLo { get; set; } // IDENTITY

        [Display(Name = "Số Tiểu Khu")]
        [Range(1, 999, ErrorMessage = "Số tiểu khu phải từ 1 đến 999")]
        public int? SoTieuKhu { get; set; }

        [Display(Name = "Số Khoảnh")]
        [Range(1, 999, ErrorMessage = "Số khoảnh phải từ 1 đến 999")]
        public int? SoKhoanh { get; set; }

        [Display(Name = "Số Lô")]
        [Range(1, 999, ErrorMessage = "Số lô phải từ 1 đến 999")]
        public int? SoLo { get; set; }

        [StringLength(10)]
        [Display(Name = "Thôn/Bản")]
        public string? MaThon { get; set; }

        [Display(Name = "Diện Tích (ha)")]
        [Range(0.01, 9999.99, ErrorMessage = "Diện tích phải lớn hơn 0")]
        public decimal? DienTich { get; set; }

        [StringLength(100)]
        [Display(Name = "Loại Rừng")]
        public string? LoaiRung { get; set; }

        [StringLength(50)]
        [Display(Name = "Trạng Thái")]
        public string? TrangThai { get; set; }

        // Navigation properties
        public DanhMucThon? DanhMucThon { get; set; }
        public ICollection<SinhVat> SinhVats { get; set; } = new List<SinhVat>();
        public ICollection<NhatKyBaoVe> NhatKyBaoVes { get; set; } = new List<NhatKyBaoVe>();
    }
}