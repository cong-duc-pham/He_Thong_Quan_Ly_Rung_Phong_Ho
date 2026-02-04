using System.ComponentModel.DataAnnotations;

namespace QuanLyRungPhongHo.Models
{
    public class NhanSu
    {
        public int MaNV { get; set; } // IDENTITY
        public string HoTen { get; set; } = null!;
        public string? ChucVu { get; set; }
        public string? SDT { get; set; }
        public string? Email { get; set; }
        public string? MaXa { get; set; }

        public DanhMucXa? DanhMucXa { get; set; }
        public TaiKhoan? TaiKhoan { get; set; }
        public ICollection<NhatKyBaoVe> NhatKyBaoVes { get; set; } = new List<NhatKyBaoVe>();
    }
}