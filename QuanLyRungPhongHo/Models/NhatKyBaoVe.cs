using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyRungPhongHo.Models
{
    public class NhatKyBaoVe
    {
        public int MaNK { get; set; } // IDENTITY
        public DateTime NgayGhi { get; set; } = DateTime.Now;
        public string? LoaiSuViec { get; set; }
        public string? NoiDung { get; set; }

        [ForeignKey(nameof(LoRung))]
        public int? MaLo { get; set; }

        [ForeignKey(nameof(NhanSu))]
        public int? MaNV_GhiNhan { get; set; }
        public string? ToaDoGPS { get; set; }

        public LoRung? LoRung { get; set; }
        public NhanSu? NhanSu { get; set; }
    }
}