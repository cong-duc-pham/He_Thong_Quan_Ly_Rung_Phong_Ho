using System.ComponentModel.DataAnnotations;

namespace QuanLyRungPhongHo.Models
{
    public class SinhVat
    {
        public int MaSV { get; set; } // IDENTITY
        public string TenLoai { get; set; } = null!;
        public string? LoaiSV { get; set; } // Động vật / Thực vật
        public string? MucDoQuyHiem { get; set; }
        public int? MaLo { get; set; }

        public LoRung? LoRung { get; set; }
    }
}