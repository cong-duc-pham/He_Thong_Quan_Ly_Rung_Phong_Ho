using System.ComponentModel.DataAnnotations;

namespace QuanLyRungPhongHo.Models
{
    public class NhatKyBaoVe
    {
        public int MaNK { get; set; } // IDENTITY
        public DateTime NgayGhi { get; set; } = DateTime.Now;
        public string? LoaiSuViec { get; set; }
        public string? NoiDung { get; set; }
        public int? MaLo { get; set; }
        public int? MaNV_GhiNhan { get; set; }
        public string? ToaDoGPS { get; set; }

        public LoRung? LoRung { get; set; }
        public NhanSu? NhanSu { get; set; }
    }
}