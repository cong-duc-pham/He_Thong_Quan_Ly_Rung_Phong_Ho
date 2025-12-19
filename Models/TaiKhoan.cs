using System.ComponentModel.DataAnnotations;

namespace QuanLyRungPhongHo.Models
{
    public class TaiKhoan
    {
        public int MaTK { get; set; } // IDENTITY
        public string TenDangNhap { get; set; } = null!;
        public string MatKhau { get; set; } = null!; // Sau này hash
        public string Quyen { get; set; } = "NhanVien_Thon";

        public int? MaNV { get; set; }
        public NhanSu? NhanSu { get; set; }
    }
}