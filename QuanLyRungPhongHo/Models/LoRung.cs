using System.ComponentModel.DataAnnotations;

namespace QuanLyRungPhongHo.Models
{
    public class LoRung
    {
        public int MaLo { get; set; } // IDENTITY
        public int? SoTieuKhu { get; set; }
        public int? SoKhoanh { get; set; }
        public int? SoLo { get; set; }
        public string? MaThon { get; set; }
        public decimal? DienTich { get; set; }
        public string? LoaiRung { get; set; }
        public string? TrangThai { get; set; }

        public DanhMucThon? DanhMucThon { get; set; }
        public ICollection<SinhVat> SinhVats { get; set; } = new List<SinhVat>();
        public ICollection<NhatKyBaoVe> NhatKyBaoVes { get; set; } = new List<NhatKyBaoVe>();
    }
}