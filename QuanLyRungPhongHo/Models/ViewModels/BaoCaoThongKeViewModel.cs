using System.ComponentModel.DataAnnotations;

namespace QuanLyRungPhongHo.Models.ViewModels
{
    /// <summary>
    /// ViewModel cho trang Báo cáo Thống kê
    /// </summary>
    public class BaoCaoThongKeViewModel
    {
        // ===== THỐNG KÊ TỔNG QUAN =====
        public int TongSoXa { get; set; }
        public int TongSoThon { get; set; }
        public int TongSoLoRung { get; set; }
        public decimal TongDienTichRung { get; set; }
        public int TongSoNhanSu { get; set; }
        public int TongSoSinhVat { get; set; }
        public int TongSoSuKien { get; set; }

        // ===== THỐNG KÊ THEO LOẠI RỪNG =====
        public List<ThongKeLoaiRung> ThongKeTheoLoaiRung { get; set; } = new();

        // ===== THỐNG KÊ THEO TRẠNG THÁI RỪNG =====
        public List<ThongKeTrangThaiRung> ThongKeTheoTrangThai { get; set; } = new();

        // ===== THỐNG KÊ THEO XÃ =====
        public List<ThongKeTheoXa> ThongKeTheoXa { get; set; } = new();

        // ===== THỐNG KÊ SỰ KIỆN BẢO VỆ =====
        public List<ThongKeSuKien> ThongKeSuKienBaoVe { get; set; } = new();

        // ===== THỐNG KÊ SINH VẬT =====
        public int TongDongVat { get; set; }
        public int TongThucVat { get; set; }
        public int SoLoaiQuyHiem { get; set; }
        public int SoLoaiNguyCap { get; set; }
        public List<ThongKeSinhVat> Top10SinhVat { get; set; } = new();

        // ===== BỘ LỌC =====
        [Display(Name = "Xã")]
        public string? MaXaFilter { get; set; }

        [Display(Name = "Từ ngày")]
        [DataType(DataType.Date)]
        public DateTime? TuNgay { get; set; }

        [Display(Name = "Đến ngày")]
        [DataType(DataType.Date)]
        public DateTime? DenNgay { get; set; }

        // ===== DANH SÁCH CHO DROPDOWN =====
        public List<DanhMucXa> DanhSachXa { get; set; } = new();
    }

    /// <summary>
    /// Thống kê theo Loại Rừng
    /// </summary>
    public class ThongKeLoaiRung
    {
        public string LoaiRung { get; set; } = string.Empty;
        public int SoLuongLo { get; set; }
        public decimal TongDienTich { get; set; }
        public double TyLe { get; set; }
    }

    /// <summary>
    /// Thống kê theo Trạng Thái Rừng
    /// </summary>
    public class ThongKeTrangThaiRung
    {
        public string TrangThai { get; set; } = string.Empty;
        public int SoLuongLo { get; set; }
        public decimal TongDienTich { get; set; }
        public double TyLe { get; set; }
    }

    /// <summary>
    /// Thống kê theo Xã
    /// </summary>
    public class ThongKeTheoXa
    {
        public string MaXa { get; set; } = string.Empty;
        public string TenXa { get; set; } = string.Empty;
        public int SoThon { get; set; }
        public int SoLoRung { get; set; }
        public decimal TongDienTich { get; set; }
        public int SoNhanSu { get; set; }
        public int SoSuKien { get; set; }
    }

    /// <summary>
    /// Thống kê Sự kiện Bảo vệ
    /// </summary>
    public class ThongKeSuKien
    {
        public string LoaiSuViec { get; set; } = string.Empty;
        public int SoLuong { get; set; }
        public double TyLe { get; set; }
    }

    /// <summary>
    /// Thống kê Sinh vật
    /// </summary>
    public class ThongKeSinhVat
    {
        public string TenLoai { get; set; } = string.Empty;
        public string LoaiSV { get; set; } = string.Empty;
        public string MucDoQuyHiem { get; set; } = string.Empty;
        public int SoLuongGhiNhan { get; set; }
    }
}           