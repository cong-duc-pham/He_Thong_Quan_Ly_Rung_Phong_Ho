namespace QuanLyRungPhongHo.Models
{
    public class HomeViewModel
    {
        // KPI Tổng Quan
        public decimal TongDienTich { get; set; }
        public int TongSoLoRung { get; set; }
        public int TongSoNhanSu { get; set; }
        public int SoCanhBao7Ngay { get; set; }

        // Thông tin người dùng đăng nhập
        public string? TenDangNhap { get; set; }
        public string? HoTen { get; set; }
        public string? ChucVu { get; set; }
        public string? Quyen { get; set; }
        public int? MaNV { get; set; }

        // Cảnh báo khẩn cấp (Top 10)
        public List<CanhBaoKhanCap> DanhSachCanhBao { get; set; } = new List<CanhBaoKhanCap>();

        // Công việc của tôi
        public int SoLoRungPhuTrach { get; set; }
        public decimal DienTichPhuTrach { get; set; }
        public int SoNhatKyDaGhi { get; set; }
        public List<NhatKyGanDay> NhatKyGanDay { get; set; } = new List<NhatKyGanDay>();
    }

    // DTO cho Cảnh báo
    public class CanhBaoKhanCap
    {
        public int MaNK { get; set; }
        public DateTime NgayGhi { get; set; }
        public string? LoaiSuViec { get; set; }
        public string? NoiDung { get; set; }
        public string? ViTri { get; set; } // TK 121 - Khoảnh 2 - Lô a
        public string? TenXa { get; set; }
        public string? NguoiGhiNhan { get; set; }
        public string MucDoClass { get; set; } = "warning"; // danger, warning, info
    }

    // DTO cho Nhật ký gần đây
    public class NhatKyGanDay
    {
        public DateTime NgayGhi { get; set; }
        public string? LoaiSuViec { get; set; }
        public string? NoiDung { get; set; }
        public string? ViTri { get; set; }
    }
}
