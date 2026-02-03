using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyRungPhongHo.Data;
using QuanLyRungPhongHo.Models.ViewModels;
using System.Text;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace QuanLyRungPhongHo.Controllers
{
    /// <summary>
    /// Controller quản lý Báo cáo Thống kê
    /// Chuẩn nghiệp vụ quản lý rừng phòng hộ
    /// </summary>
    [Authorize]
    public class BaoCaoThongKeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BaoCaoThongKeController> _logger;

        public BaoCaoThongKeController(
            ApplicationDbContext context,
            ILogger<BaoCaoThongKeController> logger)
        {
            _context = context;
            _logger = logger;
            
            // Set EPPlus License Context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// Trang Báo cáo Thống kê chính
        /// GET: BaoCaoThongKe/Index
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(string? maXaFilter, DateTime? tuNgay, DateTime? denNgay)
        {
            try
            {
                // Validation ngày tháng
                if (tuNgay.HasValue && denNgay.HasValue && tuNgay > denNgay)
                {
                    TempData["ErrorMessage"] = "Từ ngày không được lớn hơn Đến ngày";
                    tuNgay = null;
                    denNgay = null;
                }

                // Nếu không có filter ngày, mặc định lấy 30 ngày gần nhất cho sự kiện
                DateTime defaultTuNgay = tuNgay ?? DateTime.Now.AddDays(-30);
                DateTime defaultDenNgay = denNgay ?? DateTime.Now;

                var viewModel = new BaoCaoThongKeViewModel
                {
                    MaXaFilter = maXaFilter,
                    TuNgay = tuNgay,
                    DenNgay = denNgay,
                    DanhSachXa = await _context.DanhMucXas
                        .OrderBy(x => x.TenXa)
                        .ToListAsync()
                };

                // Query cơ bản với filter
                var queryLoRung = _context.LoRungs
                    .Include(l => l.DanhMucThon)
                    .AsQueryable();

                var queryNhanSu = _context.NhanSus.AsQueryable();
                var querySuKien = _context.NhatKyBaoVes
                    .Include(nk => nk.LoRung)
                    .ThenInclude(l => l!.DanhMucThon)
                    .Where(nk => nk.NgayGhi >= defaultTuNgay && nk.NgayGhi <= defaultDenNgay);

                if (!string.IsNullOrEmpty(maXaFilter))
                {
                    queryLoRung = queryLoRung.Where(l => l.DanhMucThon!.MaXa == maXaFilter);
                    queryNhanSu = queryNhanSu.Where(n => n.MaXa == maXaFilter);
                    querySuKien = querySuKien.Where(nk => nk.LoRung!.DanhMucThon!.MaXa == maXaFilter);
                }

                // ===== 1. THỐNG KÊ TỔNG QUAN =====
                viewModel.TongSoXa = string.IsNullOrEmpty(maXaFilter)
                    ? await _context.DanhMucXas.CountAsync()
                    : 1;

                viewModel.TongSoThon = string.IsNullOrEmpty(maXaFilter)
                    ? await _context.DanhMucThons.CountAsync()
                    : await _context.DanhMucThons.CountAsync(t => t.MaXa == maXaFilter);

                viewModel.TongSoLoRung = await queryLoRung.CountAsync();
                viewModel.TongDienTichRung = await queryLoRung.SumAsync(l => l.DienTich ?? 0);
                viewModel.TongSoNhanSu = await queryNhanSu.CountAsync();
                
                var maLoRungList = await queryLoRung.Select(l => l.MaLo).ToListAsync();
                viewModel.TongSoSinhVat = await _context.SinhVats
                    .Where(sv => maLoRungList.Contains(sv.MaLo))
                    .CountAsync();

                viewModel.TongSoSuKien = await querySuKien.CountAsync();

                // ===== 2. THỐNG KÊ THEO LOẠI RỪNG =====
                var thongKeLoaiRung = await queryLoRung
                    .Where(l => !string.IsNullOrEmpty(l.LoaiRung))
                    .GroupBy(l => l.LoaiRung)
                    .Select(g => new ThongKeLoaiRung
                    {
                        LoaiRung = g.Key ?? "Chưa phân loại",
                        SoLuongLo = g.Count(),
                        TongDienTich = g.Sum(l => l.DienTich ?? 0)
                    })
                    .OrderByDescending(x => x.TongDienTich)
                    .ToListAsync();

                // Tính tỷ lệ %
                decimal tongDienTich = thongKeLoaiRung.Sum(x => x.TongDienTich);
                if (tongDienTich > 0)
                {
                    foreach (var item in thongKeLoaiRung)
                    {
                        item.TyLe = Math.Round((double)(item.TongDienTich / tongDienTich * 100), 2);
                    }
                }
                viewModel.ThongKeTheoLoaiRung = thongKeLoaiRung;

                // ===== 3. THỐNG KÊ THEO TRẠNG THÁI =====
                var thongKeTrangThai = await queryLoRung
                    .Where(l => !string.IsNullOrEmpty(l.TrangThai))
                    .GroupBy(l => l.TrangThai)
                    .Select(g => new ThongKeTrangThaiRung
                    {
                        TrangThai = g.Key ?? "Chưa xác định",
                        SoLuongLo = g.Count(),
                        TongDienTich = g.Sum(l => l.DienTich ?? 0)
                    })
                    .OrderByDescending(x => x.TongDienTich)
                    .ToListAsync();

                if (tongDienTich > 0)
                {
                    foreach (var item in thongKeTrangThai)
                    {
                        item.TyLe = Math.Round((double)(item.TongDienTich / tongDienTich * 100), 2);
                    }
                }
                viewModel.ThongKeTheoTrangThai = thongKeTrangThai;

                // ===== 4. THỐNG KÊ THEO XÃ =====
                var thongKeXa = await _context.DanhMucXas
                    .Where(x => string.IsNullOrEmpty(maXaFilter) || x.MaXa == maXaFilter)
                    .Select(x => new ThongKeTheoXa
                    {
                        MaXa = x.MaXa,
                        TenXa = x.TenXa,
                        SoThon = x.DanhMucThons.Count,
                        SoLoRung = x.DanhMucThons.SelectMany(t => t.LoRungs).Count(),
                        TongDienTich = x.DanhMucThons.SelectMany(t => t.LoRungs).Sum(l => l.DienTich ?? 0),
                        SoNhanSu = x.NhanSus.Count,
                        SoSuKien = x.DanhMucThons
                            .SelectMany(t => t.LoRungs)
                            .SelectMany(l => l.NhatKyBaoVes)
                            .Count(nk => nk.NgayGhi >= defaultTuNgay && nk.NgayGhi <= defaultDenNgay)
                    })
                    .OrderByDescending(x => x.TongDienTich)
                    .ToListAsync();

                viewModel.ThongKeTheoXa = thongKeXa;

                // ===== 5. THỐNG KÊ SỰ KIỆN BẢO VỆ =====
                var thongKeSuKien = await querySuKien
                    .Where(nk => !string.IsNullOrEmpty(nk.LoaiSuViec))
                    .GroupBy(nk => nk.LoaiSuViec)
                    .Select(g => new ThongKeSuKien
                    {
                        LoaiSuViec = g.Key ?? "Khác",
                        SoLuong = g.Count()
                    })
                    .OrderByDescending(x => x.SoLuong)
                    .ToListAsync();

                int tongSuKien = thongKeSuKien.Sum(x => x.SoLuong);
                if (tongSuKien > 0)
                {
                    foreach (var item in thongKeSuKien)
                    {
                        item.TyLe = Math.Round((double)item.SoLuong / tongSuKien * 100, 2);
                    }
                }
                viewModel.ThongKeSuKienBaoVe = thongKeSuKien;

                // ===== 6. THỐNG KÊ SINH VẬT =====
                var querySinhVat = _context.SinhVats
                    .Where(sv => maLoRungList.Contains(sv.MaLo));

                viewModel.TongDongVat = await querySinhVat.CountAsync(sv => sv.LoaiSV == "Động vật");
                viewModel.TongThucVat = await querySinhVat.CountAsync(sv => sv.LoaiSV == "Thực vật");
                viewModel.SoLoaiQuyHiem = await querySinhVat.CountAsync(sv => 
                    sv.MucDoQuyHiem == "Cực kỳ nguy cấp" || 
                    sv.MucDoQuyHiem == "Nguy cấp" ||
                    sv.MucDoQuyHiem == "Sắp nguy cấp");
                viewModel.SoLoaiNguyCap = await querySinhVat.CountAsync(sv => 
                    sv.MucDoQuyHiem == "Cực kỳ nguy cấp" || 
                    sv.MucDoQuyHiem == "Nguy cấp");

                viewModel.Top10SinhVat = await querySinhVat
                    .GroupBy(sv => new { sv.TenLoai, sv.LoaiSV, sv.MucDoQuyHiem })
                    .Select(g => new ThongKeSinhVat
                    {
                        TenLoai = g.Key.TenLoai,
                        LoaiSV = g.Key.LoaiSV ?? "Chưa xác định",
                        MucDoQuyHiem = g.Key.MucDoQuyHiem ?? "Chưa đánh giá",
                        SoLuongGhiNhan = g.Count()
                    })
                    .OrderByDescending(x => x.SoLuongGhiNhan)
                    .Take(10)
                    .ToListAsync();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải báo cáo thống kê");
                TempData["ErrorMessage"] = $"Lỗi tải dữ liệu: {ex.Message}";
                return View(new BaoCaoThongKeViewModel { DanhSachXa = new List<Models.DanhMucXa>() });
            }
        }

        /// <summary>
        /// Export báo cáo ra Excel (.xlsx) với format đẹp và tiếng Việt chính xác
        /// GET: BaoCaoThongKe/ExportCsv
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportCsv(string? maXaFilter, DateTime? tuNgay, DateTime? denNgay)
        {
            try
            {
                _logger.LogInformation("Bắt đầu xuất báo cáo Excel");

                DateTime defaultTuNgay = tuNgay ?? DateTime.Now.AddDays(-30);
                DateTime defaultDenNgay = denNgay ?? DateTime.Now;

                // Query data
                var queryLoRung = _context.LoRungs
                    .Include(l => l.DanhMucThon)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(maXaFilter))
                {
                    queryLoRung = queryLoRung.Where(l => l.DanhMucThon!.MaXa == maXaFilter);
                }

                var tongSoXa = string.IsNullOrEmpty(maXaFilter) 
                    ? await _context.DanhMucXas.CountAsync() 
                    : 1;
                var tongSoThon = string.IsNullOrEmpty(maXaFilter)
                    ? await _context.DanhMucThons.CountAsync()
                    : await _context.DanhMucThons.CountAsync(t => t.MaXa == maXaFilter);
                var tongSoLoRung = await queryLoRung.CountAsync();
                var tongDienTich = await queryLoRung.SumAsync(l => l.DienTich ?? 0);
                var tongNhanSu = string.IsNullOrEmpty(maXaFilter)
                    ? await _context.NhanSus.CountAsync()
                    : await _context.NhanSus.CountAsync(n => n.MaXa == maXaFilter);

                var thongKeLoaiRung = await queryLoRung
                    .Where(l => !string.IsNullOrEmpty(l.LoaiRung))
                    .GroupBy(l => l.LoaiRung)
                    .Select(g => new { LoaiRung = g.Key, SoLuong = g.Count(), DienTich = g.Sum(l => l.DienTich ?? 0) })
                    .OrderByDescending(x => x.DienTich)
                    .ToListAsync();

                var thongKeTrangThai = await queryLoRung
                    .Where(l => !string.IsNullOrEmpty(l.TrangThai))
                    .GroupBy(l => l.TrangThai)
                    .Select(g => new { TrangThai = g.Key, SoLuong = g.Count(), DienTich = g.Sum(l => l.DienTich ?? 0) })
                    .OrderByDescending(x => x.DienTich)
                    .ToListAsync();

                var thongKeXa = await _context.DanhMucXas
                    .Where(x => string.IsNullOrEmpty(maXaFilter) || x.MaXa == maXaFilter)
                    .Include(x => x.DanhMucThons).ThenInclude(t => t.LoRungs)
                    .Include(x => x.NhanSus)
                    .ToListAsync();

                var querySuKien = _context.NhatKyBaoVes
                    .Include(nk => nk.LoRung)
                    .ThenInclude(l => l!.DanhMucThon)
                    .Where(nk => nk.NgayGhi >= defaultTuNgay && nk.NgayGhi <= defaultDenNgay);

                if (!string.IsNullOrEmpty(maXaFilter))
                {
                    querySuKien = querySuKien.Where(nk => nk.LoRung!.DanhMucThon!.MaXa == maXaFilter);
                }

                var thongKeSuKien = await querySuKien
                    .Where(nk => !string.IsNullOrEmpty(nk.LoaiSuViec))
                    .GroupBy(nk => nk.LoaiSuViec)
                    .Select(g => new { LoaiSuViec = g.Key, SoLuong = g.Count() })
                    .OrderByDescending(x => x.SoLuong)
                    .ToListAsync();

                // Tạo Excel package
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Báo cáo thống kê");

                    // Set default font cho toàn bộ worksheet
                    worksheet.Cells.Style.Font.Name = "Times New Roman";
                    worksheet.Cells.Style.Font.Size = 11;

                    int row = 1;

                    // ===== HEADER =====
                    worksheet.Cells[row, 1].Value = "BÁO CÁO THỐNG KÊ QUẢN LÝ RỪNG PHÒNG HỘ";
                    worksheet.Cells[row, 1, row, 6].Merge = true;
                    worksheet.Cells[row, 1].Style.Font.Size = 16;
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[row, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    worksheet.Row(row).Height = 30;
                    row += 2;

                    // Thông tin báo cáo
                    if (!string.IsNullOrEmpty(maXaFilter))
                    {
                        var tenXa = await _context.DanhMucXas
                            .Where(x => x.MaXa == maXaFilter)
                            .Select(x => x.TenXa)
                            .FirstOrDefaultAsync();
                        worksheet.Cells[row, 1].Value = $"Phạm vi: Xã {tenXa ?? maXaFilter}";
                        worksheet.Cells[row, 1].Style.Font.Bold = true;
                        row++;
                    }
                    else
                    {
                        worksheet.Cells[row, 1].Value = "Phạm vi: Toàn tỉnh";
                        worksheet.Cells[row, 1].Style.Font.Bold = true;
                        row++;
                    }
                    
                    worksheet.Cells[row, 1].Value = $"Thời gian: {defaultTuNgay:dd/MM/yyyy} - {defaultDenNgay:dd/MM/yyyy}";
                    row++;
                    worksheet.Cells[row, 1].Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
                    row += 2;

                    // ===== I. THỐNG KÊ TỔNG QUAN =====
                    worksheet.Cells[row, 1].Value = "I. THỐNG KÊ TỔNG QUAN";
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    worksheet.Cells[row, 1].Style.Font.Size = 12;
                    row++;

                    // Header
                    var headerCells = new[] { "STT", "Chỉ tiêu", "Đơn vị tính", "Số lượng" };
                    for (int i = 0; i < headerCells.Length; i++)
                    {
                        var cell = worksheet.Cells[row, i + 1];
                        cell.Value = headerCells[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                        cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    }
                    row++;

                    // Data rows
                    var dataRows = new[]
                    {
                        new { STT = 1, ChiTieu = "Số xã được quản lý", DonVi = "xã", SoLuong = tongSoXa.ToString() },
                        new { STT = 2, ChiTieu = "Số thôn/bản", DonVi = "thôn", SoLuong = tongSoThon.ToString() },
                        new { STT = 3, ChiTieu = "Số lô rừng", DonVi = "lô", SoLuong = tongSoLoRung.ToString() },
                        new { STT = 4, ChiTieu = "Tổng diện tích rừng", DonVi = "ha", SoLuong = tongDienTich.ToString("N2") },
                        new { STT = 5, ChiTieu = "Số cán bộ quản lý", DonVi = "người", SoLuong = tongNhanSu.ToString() }
                    };

                    foreach (var dataRow in dataRows)
                    {
                        worksheet.Cells[row, 1].Value = dataRow.STT;
                        worksheet.Cells[row, 2].Value = dataRow.ChiTieu;
                        worksheet.Cells[row, 3].Value = dataRow.DonVi;
                        worksheet.Cells[row, 4].Value = dataRow.SoLuong;
                        
                        // Center align STT
                        worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        worksheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        
                        // Border
                        for (int i = 1; i <= 4; i++)
                        {
                            worksheet.Cells[row, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        }
                        row++;
                    }
                    row++;

                    // ===== II. THỐNG KÊ THEO LOẠI RỪNG =====
                    worksheet.Cells[row, 1].Value = "II. THỐNG KÊ THEO LOẠI RỪNG";
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    worksheet.Cells[row, 1].Style.Font.Size = 12;
                    row++;

                    // Header
                    headerCells = new[] { "STT", "Loại rừng", "Số lô", "Diện tích (ha)", "Tỷ lệ (%)" };
                    for (int i = 0; i < headerCells.Length; i++)
                    {
                        var cell = worksheet.Cells[row, i + 1];
                        cell.Value = headerCells[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                        cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                    row++;

                    decimal tongDT = thongKeLoaiRung.Sum(x => x.DienTich);
                    for (int i = 0; i < thongKeLoaiRung.Count; i++)
                    {
                        var item = thongKeLoaiRung[i];
                        var tyLe = tongDT > 0 ? Math.Round((double)(item.DienTich / tongDT * 100), 2) : 0;
                        
                        worksheet.Cells[row, 1].Value = i + 1;
                        worksheet.Cells[row, 2].Value = item.LoaiRung;
                        worksheet.Cells[row, 3].Value = item.SoLuong;
                        worksheet.Cells[row, 4].Value = item.DienTich.ToString("N2");
                        worksheet.Cells[row, 5].Value = tyLe.ToString("N2");
                        
                        worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        worksheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        worksheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        worksheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        
                        for (int j = 1; j <= 5; j++)
                        {
                            worksheet.Cells[row, j].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        }
                        row++;
                    }

                    // Tổng cộng
                    worksheet.Cells[row, 1].Value = "TỔNG CỘNG";
                    worksheet.Cells[row, 1, row, 2].Merge = true;
                    worksheet.Cells[row, 3].Value = thongKeLoaiRung.Sum(x => x.SoLuong);
                    worksheet.Cells[row, 4].Value = tongDT.ToString("N2");
                    worksheet.Cells[row, 5].Value = "100.00";
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    worksheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    
                    for (int j = 1; j <= 5; j++)
                    {
                        worksheet.Cells[row, j].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                    row += 2;

                    // ===== III. THỐNG KÊ THEO TRẠNG THÁI =====
                    worksheet.Cells[row, 1].Value = "III. THỐNG KÊ THEO CHẤT LƯỢNG RỪNG";
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    worksheet.Cells[row, 1].Style.Font.Size = 12;
                    row++;

                    // Header
                    headerCells = new[] { "STT", "Trạng thái rừng", "Số lô", "Diện tích (ha)", "Tỷ lệ (%)" };
                    for (int i = 0; i < headerCells.Length; i++)
                    {
                        var cell = worksheet.Cells[row, i + 1];
                        cell.Value = headerCells[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                        cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                    row++;

                    for (int i = 0; i < thongKeTrangThai.Count; i++)
                    {
                        var item = thongKeTrangThai[i];
                        var tyLe = tongDT > 0 ? Math.Round((double)(item.DienTich / tongDT * 100), 2) : 0;
                        
                        worksheet.Cells[row, 1].Value = i + 1;
                        worksheet.Cells[row, 2].Value = item.TrangThai;
                        worksheet.Cells[row, 3].Value = item.SoLuong;
                        worksheet.Cells[row, 4].Value = item.DienTich.ToString("N2");
                        worksheet.Cells[row, 5].Value = tyLe.ToString("N2");
                        
                        worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        worksheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        worksheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        worksheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        
                        for (int j = 1; j <= 5; j++)
                        {
                            worksheet.Cells[row, j].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        }
                        row++;
                    }

                    // Tổng cộng
                    worksheet.Cells[row, 1].Value = "TỔNG CỘNG";
                    worksheet.Cells[row, 1, row, 2].Merge = true;
                    worksheet.Cells[row, 3].Value = thongKeTrangThai.Sum(x => x.SoLuong);
                    worksheet.Cells[row, 4].Value = tongDT.ToString("N2");
                    worksheet.Cells[row, 5].Value = "100.00";
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    worksheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    
                    for (int j = 1; j <= 5; j++)
                    {
                        worksheet.Cells[row, j].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                    row += 2;

                    // ===== IV. THỐNG KÊ THEO XÃ =====
                    worksheet.Cells[row, 1].Value = "IV. THỐNG KÊ THEO ĐƠN VỊ HÀNH CHÍNH";
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    worksheet.Cells[row, 1].Style.Font.Size = 12;
                    row++;

                    // Header
                    headerCells = new[] { "STT", "Tên xã", "Số thôn/bản", "Số lô rừng", "Diện tích (ha)", "Số cán bộ" };
                    for (int i = 0; i < headerCells.Length; i++)
                    {
                        var cell = worksheet.Cells[row, i + 1];
                        cell.Value = headerCells[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                        cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                    row++;

                    for (int i = 0; i < thongKeXa.Count; i++)
                    {
                        var xa = thongKeXa[i];
                        var soThon = xa.DanhMucThons.Count;
                        var soLoRung = xa.DanhMucThons.SelectMany(t => t.LoRungs).Count();
                        var dienTich = xa.DanhMucThons.SelectMany(t => t.LoRungs).Sum(l => l.DienTich ?? 0);
                        var soNhanSu = xa.NhanSus.Count;
                        
                        worksheet.Cells[row, 1].Value = i + 1;
                        worksheet.Cells[row, 2].Value = xa.TenXa;
                        worksheet.Cells[row, 3].Value = soThon;
                        worksheet.Cells[row, 4].Value = soLoRung;
                        worksheet.Cells[row, 5].Value = dienTich.ToString("N2");
                        worksheet.Cells[row, 6].Value = soNhanSu;
                        
                        worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        worksheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        worksheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        worksheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        worksheet.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        
                        for (int j = 1; j <= 6; j++)
                        {
                            worksheet.Cells[row, j].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        }
                        row++;
                    }
                    
                    // Tổng cộng
                    var tongSoThonSum = thongKeXa.Sum(x => x.DanhMucThons.Count);
                    var tongSoLoSum = thongKeXa.SelectMany(x => x.DanhMucThons).SelectMany(t => t.LoRungs).Count();
                    var tongDienTichSum = thongKeXa.SelectMany(x => x.DanhMucThons).SelectMany(t => t.LoRungs).Sum(l => l.DienTich ?? 0);
                    var tongNhanSuSum = thongKeXa.Sum(x => x.NhanSus.Count);
                    
                    worksheet.Cells[row, 1].Value = "TỔNG CỘNG";
                    worksheet.Cells[row, 1, row, 2].Merge = true;
                    worksheet.Cells[row, 3].Value = tongSoThonSum;
                    worksheet.Cells[row, 4].Value = tongSoLoSum;
                    worksheet.Cells[row, 5].Value = tongDienTichSum.ToString("N2");
                    worksheet.Cells[row, 6].Value = tongNhanSuSum;
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    worksheet.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    
                    for (int j = 1; j <= 6; j++)
                    {
                        worksheet.Cells[row, j].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                    row += 2;

                    // ===== V. SỰ KIỆN BẢO VỆ =====
                    worksheet.Cells[row, 1].Value = "V. THỐNG KÊ SỰ KIỆN BẢO VỆ RỪNG";
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    worksheet.Cells[row, 1].Style.Font.Size = 12;
                    row++;

                    // Header
                    headerCells = new[] { "STT", "Loại sự việc", "Số vụ", "Tỷ lệ (%)" };
                    for (int i = 0; i < headerCells.Length; i++)
                    {
                        var cell = worksheet.Cells[row, i + 1];
                        cell.Value = headerCells[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                        cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                    row++;

                    int tongSuKien = thongKeSuKien.Sum(x => x.SoLuong);
                    for (int i = 0; i < thongKeSuKien.Count; i++)
                    {
                        var item = thongKeSuKien[i];
                        var tyLe = tongSuKien > 0 ? Math.Round((double)item.SoLuong / tongSuKien * 100, 2) : 0;
                        
                        worksheet.Cells[row, 1].Value = i + 1;
                        worksheet.Cells[row, 2].Value = item.LoaiSuViec;
                        worksheet.Cells[row, 3].Value = item.SoLuong;
                        worksheet.Cells[row, 4].Value = tyLe.ToString("N2");
                        
                        worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        worksheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        worksheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        
                        for (int j = 1; j <= 4; j++)
                        {
                            worksheet.Cells[row, j].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        }
                        row++;
                    }

                    // Tổng cộng
                    worksheet.Cells[row, 1].Value = "TỔNG CỘNG";
                    worksheet.Cells[row, 1, row, 2].Merge = true;
                    worksheet.Cells[row, 3].Value = tongSuKien;
                    worksheet.Cells[row, 4].Value = "100.00";
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    
                    for (int j = 1; j <= 4; j++)
                    {
                        worksheet.Cells[row, j].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                    row += 2;

                    // ===== FOOTER =====
                    worksheet.Cells[row, 1].Value = "CHÚ THÍCH:";
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    row++;
                    worksheet.Cells[row, 1].Value = "- Báo cáo được tạo tự động từ Hệ thống Quản lý Rừng Phòng Hộ";
                    row++;
                    worksheet.Cells[row, 1].Value = $"- Dữ liệu được cập nhật đến: {DateTime.Now:dd/MM/yyyy HH:mm}";
                    row++;
                    worksheet.Cells[row, 1].Value = "- Đơn vị: Chi cục Kiểm lâm tỉnh";

                    // Auto-fit columns
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    // Set minimum column widths
                    worksheet.Column(1).Width = Math.Max(worksheet.Column(1).Width, 8);
                    worksheet.Column(2).Width = Math.Max(worksheet.Column(2).Width, 25);
                    worksheet.Column(3).Width = Math.Max(worksheet.Column(3).Width, 12);
                    worksheet.Column(4).Width = Math.Max(worksheet.Column(4).Width, 15);
                    worksheet.Column(5).Width = Math.Max(worksheet.Column(5).Width, 12);
                    worksheet.Column(6).Width = Math.Max(worksheet.Column(6).Width, 12);

                    // Generate filename
                    var tenXaSlug = string.IsNullOrEmpty(maXaFilter) 
                        ? "ToanTinh" 
                        : thongKeXa.FirstOrDefault()?.TenXa.Replace(" ", "") ?? "KhongXacDinh";
                    
                    var fileName = $"BaoCao_{tenXaSlug}_{defaultTuNgay:yyyyMMdd}_{defaultDenNgay:yyyyMMdd}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                    _logger.LogInformation($"Xuất Excel thành công: {fileName}");

                    var fileBytes = package.GetAsByteArray();
                    return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xuất Excel");
                TempData["ErrorMessage"] = $"Lỗi khi xuất báo cáo: {ex.Message}";
                return RedirectToAction(nameof(Index), new { maXaFilter, tuNgay, denNgay });
            }
        }

        /// <summary>
        /// Export báo cáo ra PDF - Tải xuống file HTML với tên tự động
        /// GET: BaoCaoThongKe/ExportPdf
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportPdf(string? maXaFilter, DateTime? tuNgay, DateTime? denNgay)
        {
            try
            {
                _logger.LogInformation("Bắt đầu xuất báo cáo PDF");

                // Lấy dữ liệu thống kê
                var viewModel = await GetBaoCaoData(maXaFilter, tuNgay, denNgay);

                // Trả về View template - browser sẽ tự động mở Print dialog
                return View("PdfTemplate", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xuất PDF");
                TempData["ErrorMessage"] = $"Lỗi khi xuất báo cáo: {ex.Message}";
                return RedirectToAction(nameof(Index), new { maXaFilter, tuNgay, denNgay });
            }
        }

        /// <summary>
        /// API: Lấy dữ liệu thống kê theo thời gian (cho biểu đồ)
        /// GET: BaoCaoThongKe/GetChartData
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetChartData(string? maXaFilter, DateTime? tuNgay, DateTime? denNgay)
        {
            try
            {
                DateTime fromDate = tuNgay ?? DateTime.Now.AddMonths(-6);
                DateTime toDate = denNgay ?? DateTime.Now;

                var querySuKien = _context.NhatKyBaoVes
                    .Include(nk => nk.LoRung)
                    .ThenInclude(l => l!.DanhMucThon)
                    .Where(nk => nk.NgayGhi >= fromDate && nk.NgayGhi <= toDate);

                if (!string.IsNullOrEmpty(maXaFilter))
                {
                    querySuKien = querySuKien.Where(nk => nk.LoRung!.DanhMucThon!.MaXa == maXaFilter);
                }

                var thongKeTheoThang = await querySuKien
                    .GroupBy(nk => new { nk.NgayGhi.Year, nk.NgayGhi.Month })
                    .Select(g => new
                    {
                        Nam = g.Key.Year,
                        Thang = g.Key.Month,
                        SoLuong = g.Count()
                    })
                    .OrderBy(x => x.Nam)
                    .ThenBy(x => x.Thang)
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    data = thongKeTheoThang.Select(x => new
                    {
                        label = $"{x.Thang:00}/{x.Nam}",
                        value = x.SoLuong
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy dữ liệu biểu đồ");
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// API: Kiểm tra trạng thái hệ thống
        /// GET: BaoCaoThongKe/HealthCheck
        /// </summary>
        [HttpGet]
        public JsonResult HealthCheck()
        {
            try
            {
                return Json(new { 
                    success = true, 
                    message = "Hệ thống hoạt động bình thường",
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi health check");
                return Json(new { 
                    success = false, 
                    message = $"Lỗi hệ thống: {ex.Message}",
                    timestamp = DateTime.Now
                });
            }
        }

        #region Private Methods

        /// <summary>
        /// Lấy dữ liệu báo cáo đầy đủ (tái sử dụng cho cả Index và Export)
        /// </summary>
        private async Task<BaoCaoThongKeViewModel> GetBaoCaoData(string? maXaFilter, DateTime? tuNgay, DateTime? denNgay)
        {
            DateTime defaultTuNgay = tuNgay ?? DateTime.Now.AddDays(-30);
            DateTime defaultDenNgay = denNgay ?? DateTime.Now;

            var viewModel = new BaoCaoThongKeViewModel
            {
                MaXaFilter = maXaFilter,
                TuNgay = tuNgay,
                DenNgay = denNgay,
                DanhSachXa = await _context.DanhMucXas.OrderBy(x => x.TenXa).ToListAsync()
            };

            // Query cơ bản với filter
            var queryLoRung = _context.LoRungs
                .Include(l => l.DanhMucThon)
                .AsQueryable();
            if (!string.IsNullOrEmpty(maXaFilter))
            {
                queryLoRung = queryLoRung.Where(l => l.DanhMucThon!.MaXa == maXaFilter);
            }

            // Thống kê tổng quan
            viewModel.TongSoXa = string.IsNullOrEmpty(maXaFilter) ? await _context.DanhMucXas.CountAsync() : 1;
            viewModel.TongSoThon = string.IsNullOrEmpty(maXaFilter) 
                ? await _context.DanhMucThons.CountAsync()
                : await _context.DanhMucThons.CountAsync(t => t.MaXa == maXaFilter);
            viewModel.TongSoLoRung = await queryLoRung.CountAsync();
            viewModel.TongDienTichRung = await queryLoRung.SumAsync(l => l.DienTich ?? 0);
            
            var queryNhanSu = _context.NhanSus.AsQueryable();
            if (!string.IsNullOrEmpty(maXaFilter))
            {
                queryNhanSu = queryNhanSu.Where(n => n.MaXa == maXaFilter);
            }
            viewModel.TongSoNhanSu = await queryNhanSu.CountAsync();

            var maLoRungList = await queryLoRung.Select(l => l.MaLo).ToListAsync();
            viewModel.TongSoSinhVat = await _context.SinhVats
                .Where(sv => maLoRungList.Contains(sv.MaLo))
                .CountAsync();

            var querySuKien = _context.NhatKyBaoVes
                .Include(nk => nk.LoRung)
                .ThenInclude(l => l!.DanhMucThon)
                .Where(nk => nk.NgayGhi >= defaultTuNgay && nk.NgayGhi <= defaultDenNgay);
            if (!string.IsNullOrEmpty(maXaFilter))
            {
                querySuKien = querySuKien.Where(nk => nk.LoRung!.DanhMucThon!.MaXa == maXaFilter);
            }
            viewModel.TongSoSuKien = await querySuKien.CountAsync();

            // Thống kê theo loại rừng
            var thongKeLoaiRung = await queryLoRung
                .Where(l => !string.IsNullOrEmpty(l.LoaiRung))
                .GroupBy(l => l.LoaiRung)
                .Select(g => new ThongKeLoaiRung
                {
                    LoaiRung = g.Key ?? "Chưa phân loại",
                    SoLuongLo = g.Count(),
                    TongDienTich = g.Sum(l => l.DienTich ?? 0)
                })
                .OrderByDescending(x => x.TongDienTich)
                .ToListAsync();

            // Tính tỷ lệ %
            decimal tongDienTich = thongKeLoaiRung.Sum(x => x.TongDienTich);
            if (tongDienTich > 0)
            {
                foreach (var item in thongKeLoaiRung)
                {
                    item.TyLe = Math.Round((double)(item.TongDienTich / tongDienTich * 100), 2);
                }
            }
            viewModel.ThongKeTheoLoaiRung = thongKeLoaiRung;

            // Thống kê theo trạng thái
            var thongKeTrangThai = await queryLoRung
                .Where(l => !string.IsNullOrEmpty(l.TrangThai))
                .GroupBy(l => l.TrangThai)
                .Select(g => new ThongKeTrangThaiRung
                {
                    TrangThai = g.Key ?? "Chưa xác định",
                    SoLuongLo = g.Count(),
                    TongDienTich = g.Sum(l => l.DienTich ?? 0)
                })
                .OrderByDescending(x => x.TongDienTich)
                .ToListAsync();

            if (tongDienTich > 0)
            {
                foreach (var item in thongKeTrangThai)
                {
                    item.TyLe = Math.Round((double)(item.TongDienTich / tongDienTich * 100), 2);
                }
            }
            viewModel.ThongKeTheoTrangThai = thongKeTrangThai;

            // Thống kê theo xã
            viewModel.ThongKeTheoXa = await _context.DanhMucXas
                .Where(x => string.IsNullOrEmpty(maXaFilter) || x.MaXa == maXaFilter)
                .Select(x => new ThongKeTheoXa
                {
                    MaXa = x.MaXa,
                    TenXa = x.TenXa,
                    SoThon = x.DanhMucThons.Count,
                    SoLoRung = x.DanhMucThons.SelectMany(t => t.LoRungs).Count(),
                    TongDienTich = x.DanhMucThons.SelectMany(t => t.LoRungs).Sum(l => l.DienTich ?? 0),
                    SoNhanSu = x.NhanSus.Count,
                    SoSuKien = x.DanhMucThons
                        .SelectMany(t => t.LoRungs)
                        .SelectMany(l => l.NhatKyBaoVes)
                        .Count(nk => nk.NgayGhi >= defaultTuNgay && nk.NgayGhi <= defaultDenNgay)
                })
                .OrderByDescending(x => x.TongDienTich)
                .ToListAsync();

            // Thống kê sự kiện bảo vệ
            var thongKeSuKien = await querySuKien
                .Where(nk => !string.IsNullOrEmpty(nk.LoaiSuViec))
                .GroupBy(nk => nk.LoaiSuViec)
                .Select(g => new ThongKeSuKien
                {
                    LoaiSuViec = g.Key ?? "Khác",
                    SoLuong = g.Count()
                })
                .OrderByDescending(x => x.SoLuong)
                .ToListAsync();

            int tongSuKien = thongKeSuKien.Sum(x => x.SoLuong);
            if (tongSuKien > 0)
            {
                foreach (var item in thongKeSuKien)
                {
                    item.TyLe = Math.Round((double)item.SoLuong / tongSuKien * 100, 2);
                }
            }
            viewModel.ThongKeSuKienBaoVe = thongKeSuKien;

            // Thống kê sinh vật (không cần thiết cho PDF nhưng giữ lại cho consistency)
            var querySinhVat = _context.SinhVats
                .Where(sv => maLoRungList.Contains(sv.MaLo));

            viewModel.TongDongVat = await querySinhVat.CountAsync(sv => sv.LoaiSV == "Động vật");
            viewModel.TongThucVat = await querySinhVat.CountAsync(sv => sv.LoaiSV == "Thực vật");
            viewModel.SoLoaiQuyHiem = await querySinhVat.CountAsync(sv => 
                sv.MucDoQuyHiem == "Cực kỳ nguy cấp" || 
                sv.MucDoQuyHiem == "Nguy cấp" ||
                sv.MucDoQuyHiem == "Sắp nguy cấp");
            viewModel.SoLoaiNguyCap = await querySinhVat.CountAsync(sv => 
                sv.MucDoQuyHiem == "Cực kỳ nguy cấp" || 
                sv.MucDoQuyHiem == "Nguy cấp");

            viewModel.Top10SinhVat = await querySinhVat
                .GroupBy(sv => new { sv.TenLoai, sv.LoaiSV, sv.MucDoQuyHiem })
                .Select(g => new ThongKeSinhVat
                {
                    TenLoai = g.Key.TenLoai,
                    LoaiSV = g.Key.LoaiSV ?? "Chưa xác định",
                    MucDoQuyHiem = g.Key.MucDoQuyHiem ?? "Chưa đánh giá",
                    SoLuongGhiNhan = g.Count()
                })
                .OrderByDescending(x => x.SoLuongGhiNhan)
                .Take(10)
                .ToListAsync();

            return viewModel;
        }

        #endregion
    }
}