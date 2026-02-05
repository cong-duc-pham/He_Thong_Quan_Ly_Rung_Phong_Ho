using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyRungPhongHo.Data;
using QuanLyRungPhongHo.Models;
using QuanLyRungPhongHo.Models;
using QuanLyRungPhongHo.Attributes;

namespace QuanLyRungPhongHo.Controllers
{
    [Authorize]
    public class ChamCongController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChamCongController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper: Kiểm tra có đúng ca làm việc được phân công không
        private (bool isValid, string message) ValidateCaLamViec(CaLamViec caLamViec, DateTime currentTime, bool isCheckIn)
        {
            var currentTimeOnly = currentTime.TimeOfDay;
            var gioBatDau = caLamViec.GioBatDau;
            var gioKetThuc = caLamViec.GioKetThuc;

            // Khung giờ cho phép vào ca: 30 phút trước đến 2 giờ sau giờ bắt đầu
            var allowCheckInFrom = gioBatDau.Add(TimeSpan.FromMinutes(-30));
            var allowCheckInTo = gioBatDau.Add(TimeSpan.FromHours(2));

            // Khung giờ cho phép tan ca: 1 giờ trước đến 1 giờ sau giờ kết thúc
            var allowCheckOutFrom = gioKetThuc.Add(TimeSpan.FromHours(-1));
            var allowCheckOutTo = gioKetThuc.Add(TimeSpan.FromHours(1));

            // DEBUG LOG
            Console.WriteLine($"=== KIỂM TRA CA LÀM VIỆC ===");
            Console.WriteLine($"Ca: {caLamViec.TenCa}");
            Console.WriteLine($"Giờ ca: {gioBatDau:hh\\:mm} - {gioKetThuc:hh\\:mm}");
            Console.WriteLine($"Giờ hiện tại: {currentTimeOnly:hh\\:mm\\:ss}");
            Console.WriteLine($"Loại: {(isCheckIn ? "VÀO CA" : "TAN CA")}");

            if (isCheckIn)
            {
                Console.WriteLine($"Khung giờ cho phép vào ca: {allowCheckInFrom:hh\\:mm} - {allowCheckInTo:hh\\:mm}");
                
                // CHẶN nếu quá sớm
                if (currentTimeOnly < allowCheckInFrom)
                {
                    var minutesUntil = (int)(allowCheckInFrom - currentTimeOnly).TotalMinutes;
                    Console.WriteLine($"❌ Quá sớm - còn {minutesUntil} phút");
                    return (false, $"❌ Chưa đến giờ vào ca! Ca {caLamViec.TenCa} ({gioBatDau:hh\\:mm} - {gioKetThuc:hh\\:mm}). Bạn có thể vào ca từ {allowCheckInFrom:hh\\:mm}.");
                }
                
                // CHẶN nếu quá muộn
                if (currentTimeOnly > allowCheckInTo)
                {
                    Console.WriteLine($"❌ Quá muộn - đã qua {allowCheckInTo:hh\\:mm}");
                    return (false, $"❌ Đã quá giờ vào ca! Ca {caLamViec.TenCa} bắt đầu lúc {gioBatDau:hh\\:mm}. Thời gian vào ca tối đa đến {allowCheckInTo:hh\\:mm}.");
                }

                // Trong khung giờ hợp lệ - kiểm tra vào muộn
                if (currentTimeOnly > gioBatDau.Add(TimeSpan.FromMinutes(15)))
                {
                    var lateMinutes = (int)(currentTimeOnly - gioBatDau).TotalMinutes;
                    Console.WriteLine($"⚠️ Vào ca muộn {lateMinutes} phút (vẫn hợp lệ)");
                    return (true, $"⚠️ Bạn đang vào ca muộn {lateMinutes} phút so với giờ quy định ({gioBatDau:hh\\:mm}). Ca {caLamViec.TenCa}.");
                }

                Console.WriteLine($"✅ Vào ca đúng giờ");
                return (true, $"✓ Vào ca thành công - Ca {caLamViec.TenCa} ({gioBatDau:hh\\:mm} - {gioKetThuc:hh\\:mm})");
            }
            else
            {
                Console.WriteLine($"Khung giờ cho phép tan ca: {allowCheckOutFrom:hh\\:mm} - {allowCheckOutTo:hh\\:mm}");
                
                // CHẶN nếu quá sớm
                if (currentTimeOnly < allowCheckOutFrom)
                {
                    var minutesUntil = (int)(allowCheckOutFrom - currentTimeOnly).TotalMinutes;
                    Console.WriteLine($"❌ Quá sớm - còn {minutesUntil} phút");
                    return (false, $"❌ Chưa đến giờ tan ca! Ca {caLamViec.TenCa} kết thúc lúc {gioKetThuc:hh\\:mm}. Bạn có thể tan ca từ {allowCheckOutFrom:hh\\:mm}.");
                }
                
                // CHẶN nếu quá muộn
                if (currentTimeOnly > allowCheckOutTo)
                {
                    Console.WriteLine($"❌ Quá muộn - đã qua {allowCheckOutTo:hh\\:mm}");
                    return (false, $"❌ Đã quá giờ tan ca! Ca {caLamViec.TenCa} kết thúc lúc {gioKetThuc:hh\\:mm}. Thời gian tan ca tối đa đến {allowCheckOutTo:hh\\:mm}.");
                }

                // Trong khung giờ hợp lệ - kiểm tra tan sớm
                if (currentTimeOnly < gioKetThuc)
                {
                    var earlyMinutes = (int)(gioKetThuc - currentTimeOnly).TotalMinutes;
                    Console.WriteLine($"⚠️ Tan ca sớm {earlyMinutes} phút (vẫn hợp lệ)");
                    return (true, $"⚠️ Bạn đang tan ca sớm {earlyMinutes} phút so với giờ quy định ({gioKetThuc:hh\\:mm}). Ca {caLamViec.TenCa}.");
                }

                Console.WriteLine($"✅ Tan ca đúng giờ");
                return (true, $"✓ Tan ca thành công - Ca {caLamViec.TenCa} ({gioBatDau:hh\\:mm} - {gioKetThuc:hh\\:mm})");
            }
        }

        // GET: ChamCong/Index - Hiển thị lịch làm việc và chấm công
        [CheckPermission("ChamCong.View")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Lấy thông tin nhân viên từ username
                var taikhoan = await _context.TaiKhoans
                    .Include(t => t.NhanSu)
                    .FirstOrDefaultAsync(t => t.TenDangNhap == username);

                if (taikhoan == null || taikhoan.NhanSu == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin nhân viên!";
                    return RedirectToAction("Index", "Home");
                }

                ViewBag.MaNV = taikhoan.MaNV;
                ViewBag.TenNV = taikhoan.NhanSu.HoTen;
                
                // Lấy lịch làm việc hôm nay
                var today = DateTime.Today;
                var lichLamViec = await _context.LichLamViecs
                    .Include(l => l.CaLamViec)
                    .Include(l => l.LoRung)
                    .Include(l => l.ChamCongs)
                    .Where(l => l.MaNV == taikhoan.MaNV && l.NgayLamViec == today)
                    .OrderBy(l => l.CaLamViec!.GioBatDau)
                    .FirstOrDefaultAsync();

                if (lichLamViec != null)
                {
                    var chamCong = lichLamViec.ChamCongs?.FirstOrDefault();
                    ViewBag.Schedule = new
                    {
                        MaLich = lichLamViec.MaLich,
                        TenCa = lichLamViec.CaLamViec?.TenCa,
                        GioBatDau = lichLamViec.CaLamViec?.GioBatDau.ToString(@"hh\:mm"),
                        GioKetThuc = lichLamViec.CaLamViec?.GioKetThuc.ToString(@"hh\:mm"),
                        TenLo = lichLamViec.LoRung != null 
                            ? $"TK{lichLamViec.LoRung.SoTieuKhu}/K{lichLamViec.LoRung.SoKhoanh}/L{lichLamViec.LoRung.SoLo}" 
                            : "Chưa xác định",
                        DaCheckIn = chamCong != null && chamCong.GioVao.HasValue,
                        DaCheckOut = chamCong != null && chamCong.GioRa.HasValue,
                        GioVao = chamCong?.GioVao?.ToString("HH:mm:ss"),
                        GioRa = chamCong?.GioRa?.ToString("HH:mm:ss"),
                        SoGioLam = chamCong?.SoGioLam
                    };
                }

                // Lấy lịch sử chấm công 7 ngày gần nhất
                var fromDate = DateTime.Today.AddDays(-7);
                var history = await _context.LichLamViecs
                    .Include(l => l.CaLamViec)
                    .Include(l => l.LoRung)
                    .Include(l => l.ChamCongs)
                    .Where(l => l.MaNV == taikhoan.MaNV 
                            && l.NgayLamViec >= fromDate 
                            && l.NgayLamViec <= DateTime.Today)
                    .OrderByDescending(l => l.NgayLamViec)
                    .ThenBy(l => l.CaLamViec!.GioBatDau)
                    .Select(l => new
                    {
                        MaNV = l.MaNV,
                        NgayLamViec = l.NgayLamViec.ToString("dd/MM/yyyy"),
                        TenCa = l.CaLamViec!.TenCa,
                        TenLo = l.LoRung != null 
                            ? $"TK{l.LoRung.SoTieuKhu}/K{l.LoRung.SoKhoanh}/L{l.LoRung.SoLo}" 
                            : "Chưa xác định",
                        chamCong = l.ChamCongs.FirstOrDefault(),
                        GioVao = l.ChamCongs.FirstOrDefault() != null && l.ChamCongs.FirstOrDefault()!.GioVao.HasValue 
                            ? l.ChamCongs.FirstOrDefault()!.GioVao.Value.ToString("HH:mm") : (string?)null,
                        GioRa = l.ChamCongs.FirstOrDefault() != null && l.ChamCongs.FirstOrDefault()!.GioRa.HasValue 
                            ? l.ChamCongs.FirstOrDefault()!.GioRa.Value.ToString("HH:mm") : (string?)null,
                        SoGioLam = l.ChamCongs.FirstOrDefault() != null && l.ChamCongs.FirstOrDefault()!.SoGioLam.HasValue 
                            ? l.ChamCongs.FirstOrDefault()!.SoGioLam.Value.ToString("0.0") + "h" : (string?)null,
                        TrangThai = l.ChamCongs.FirstOrDefault() != null && l.ChamCongs.FirstOrDefault()!.GioRa.HasValue 
                            ? "Hoàn thành" 
                            : (l.ChamCongs.FirstOrDefault() != null && l.ChamCongs.FirstOrDefault()!.GioVao.HasValue 
                                ? "Đang làm" 
                                : "Chưa chấm công")
                    })
                    .ToListAsync();

                ViewBag.History = history;
                
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Vào ca đơn giản
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckPermission("ChamCong.Create")]
        public async Task<IActionResult> CheckInSimple(int maLich)
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                {
                    return RedirectToAction("Login", "Account");
                }

                var taikhoan = await _context.TaiKhoans
                    .FirstOrDefaultAsync(t => t.TenDangNhap == username);

                if (taikhoan == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy tài khoản!";
                    return RedirectToAction("Index");
                }

                var lichLamViec = await _context.LichLamViecs
                    .Include(l => l.ChamCongs)
                    .Include(l => l.CaLamViec)
                    .FirstOrDefaultAsync(l => l.MaLich == maLich && l.MaNV == taikhoan.MaNV);

                if (lichLamViec == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy lịch làm việc!";
                    return RedirectToAction("Index");
                }

                // Kiểm tra ca làm việc
                if (lichLamViec.CaLamViec == null)
                {
                    TempData["ErrorMessage"] = "Lịch làm việc không có thông tin ca!";
                    return RedirectToAction("Index");
                }

                var chamCong = lichLamViec.ChamCongs?.FirstOrDefault();
                if (chamCong != null && chamCong.GioVao.HasValue)
                {
                    TempData["ErrorMessage"] = $"Bạn đã vào ca lúc {chamCong.GioVao.Value:HH:mm:ss}!";
                    return RedirectToAction("Index");
                }

                var currentTime = DateTime.Now;

                // Kiểm tra ca làm việc - CHẶN nếu ngoài khung giờ cho phép
                var validation = ValidateCaLamViec(lichLamViec.CaLamViec, currentTime, isCheckIn: true);
                if (!validation.isValid)
                {
                    TempData["ErrorMessage"] = validation.message;
                    return RedirectToAction("Index");
                }

                if (chamCong == null)
                {
                    chamCong = new ChamCong
                    {
                        MaLich = maLich,
                        GioVao = currentTime
                    };
                    _context.ChamCongs.Add(chamCong);
                }
                else
                {
                    chamCong.GioVao = currentTime;
                    _context.ChamCongs.Update(chamCong);
                }

                await _context.SaveChangesAsync();
                
                // Hiển thị thông báo
                if (validation.message.Contains("⚠️"))
                {
                    TempData["WarningMessage"] = validation.message + $" Đã ghi nhận vào ca lúc {currentTime:HH:mm:ss}.";
                }
                else
                {
                    TempData["SuccessMessage"] = validation.message + $" lúc {currentTime:HH:mm:ss}";
                }
                
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // POST: Tan ca đơn giản
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckPermission("ChamCong.Edit")]
        public async Task<IActionResult> CheckOutSimple(int maLich)
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                {
                    return RedirectToAction("Login", "Account");
                }

                var taikhoan = await _context.TaiKhoans
                    .FirstOrDefaultAsync(t => t.TenDangNhap == username);

                if (taikhoan == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy tài khoản!";
                    return RedirectToAction("Index");
                }

                var lichLamViec = await _context.LichLamViecs
                    .Include(l => l.ChamCongs)
                    .Include(l => l.CaLamViec)
                    .FirstOrDefaultAsync(l => l.MaLich == maLich && l.MaNV == taikhoan.MaNV);

                if (lichLamViec == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy lịch làm việc!";
                    return RedirectToAction("Index");
                }

                // Kiểm tra ca làm việc
                if (lichLamViec.CaLamViec == null)
                {
                    TempData["ErrorMessage"] = "Lịch làm việc không có thông tin ca!";
                    return RedirectToAction("Index");
                }

                var chamCong = lichLamViec.ChamCongs?.FirstOrDefault();
                if (chamCong == null || !chamCong.GioVao.HasValue)
                {
                    TempData["ErrorMessage"] = "Bạn chưa vào ca! Vui lòng vào ca trước.";
                    return RedirectToAction("Index");
                }

                if (chamCong.GioRa.HasValue)
                {
                    TempData["ErrorMessage"] = $"Bạn đã tan ca lúc {chamCong.GioRa.Value:HH:mm:ss}!";
                    return RedirectToAction("Index");
                }

                var currentTime = DateTime.Now;

                // Kiểm tra ca làm việc - CHẶN nếu ngoài khung giờ cho phép
                var validation = ValidateCaLamViec(lichLamViec.CaLamViec, currentTime, isCheckIn: false);
                if (!validation.isValid)
                {
                    TempData["ErrorMessage"] = validation.message;
                    return RedirectToAction("Index");
                }
                chamCong.GioRa = currentTime;

                var soGioLam = (currentTime - chamCong.GioVao.Value).TotalHours;
                chamCong.SoGioLam = (decimal)Math.Round(soGioLam, 2);

                _context.ChamCongs.Update(chamCong);
                await _context.SaveChangesAsync();

                // Hiển thị thông báo
                if (validation.message.Contains("⚠️"))
                {
                    TempData["WarningMessage"] = validation.message + $" Đã ghi nhận tan ca lúc {currentTime:HH:mm:ss} | Tổng: {chamCong.SoGioLam}h";
                }
                else
                {
                    TempData["SuccessMessage"] = validation.message + $" lúc {currentTime:HH:mm:ss} | Tổng: {chamCong.SoGioLam}h";
                }
                
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // Tạo lịch test cho hôm nay (chỉ dùng để test)
        [HttpGet]
        public async Task<IActionResult> CreateTestSchedule()
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                {
                    return RedirectToAction("Login", "Account");
                }

                var taikhoan = await _context.TaiKhoans
                    .FirstOrDefaultAsync(t => t.TenDangNhap == username);

                if (taikhoan == null || !taikhoan.MaNV.HasValue)
                {
                    TempData["ErrorMessage"] = "Tài khoản chưa liên kết với nhân viên!";
                    return RedirectToAction("Index");
                }

                var today = DateTime.Today;

                // Kiểm tra đã có lịch chưa
                var existingSchedule = await _context.LichLamViecs
                    .FirstOrDefaultAsync(l => l.MaNV == taikhoan.MaNV.Value && l.NgayLamViec == today);

                if (existingSchedule != null)
                {
                    TempData["ErrorMessage"] = "Bạn đã có lịch làm việc hôm nay!";
                    return RedirectToAction("Index");
                }

                // Lấy ca làm việc đầu tiên
                var caLamViec = await _context.CaLamViecs.FirstOrDefaultAsync();
                if (caLamViec == null)
                {
                    TempData["ErrorMessage"] = "Chưa có ca làm việc trong hệ thống!";
                    return RedirectToAction("Index");
                }

                // Lấy lô rừng đầu tiên (optional)
                var loRung = await _context.LoRungs.FirstOrDefaultAsync();

                // Tạo lịch mới
                var lichMoi = new LichLamViec
                {
                    MaNV = taikhoan.MaNV.Value,
                    MaCa = caLamViec.MaCa,
                    NgayLamViec = today,
                    MaLo = loRung?.MaLo,
                    TrangThai = "Đã phân công",
                    GhiChu = "Lịch test tự động",
                    NgayTao = DateTime.Now,
                    NguoiTao = taikhoan.MaNV.Value
                };

                _context.LichLamViecs.Add(lichMoi);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "✓ Đã tạo lịch test thành công cho hôm nay!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // API: Lấy lịch làm việc của nhân viên hôm nay
        [HttpGet]
        [CheckPermission("ChamCong.View")]
        public async Task<JsonResult> GetMyScheduleToday()
        {
            try
            {
                var username = User.Identity?.Name;
                
                if (string.IsNullOrEmpty(username))
                {
                    return Json(new { success = false, message = "Chưa đăng nhập! User.Identity.Name is null" });
                }

                var taikhoan = await _context.TaiKhoans
                    .FirstOrDefaultAsync(t => t.TenDangNhap == username);

                if (taikhoan == null)
                {
                    return Json(new { success = false, message = $"Không tìm thấy tài khoản với username: {username}" });
                }

                var today = DateTime.Today;
                
                var lichLamViec = await _context.LichLamViecs
                    .Include(l => l.CaLamViec)
                    .Include(l => l.LoRung)
                    .Include(l => l.ChamCongs)
                    .Where(l => l.MaNV == taikhoan.MaNV && l.NgayLamViec == today)
                    .OrderBy(l => l.CaLamViec!.GioBatDau)
                    .Select(l => new
                    {
                        chamCong = l.ChamCongs.FirstOrDefault(),
                        MaLich = l.MaLich,
                        TenCa = l.CaLamViec!.TenCa,
                        GioBatDau = l.CaLamViec.GioBatDau.ToString(@"hh\:mm"),
                        GioKetThuc = l.CaLamViec.GioKetThuc.ToString(@"hh\:mm"),
                        TenLo = l.LoRung != null 
                            ? $"TK{l.LoRung.SoTieuKhu}/K{l.LoRung.SoKhoanh}/L{l.LoRung.SoLo}" 
                            : "Chưa xác định",
                        TrangThai = l.TrangThai,
                        GhiChu = l.GhiChu
                    })
                    .Select(x => new
                    {
                        x.MaLich,
                        x.TenCa,
                        x.GioBatDau,
                        x.GioKetThuc,
                        x.TenLo,
                        x.TrangThai,
                        x.GhiChu,
                        // Thông tin chấm công
                        DaChamCong = x.chamCong != null,
                        MaChamCong = x.chamCong != null ? x.chamCong.MaChamCong : (int?)null,
                        GioVao = x.chamCong != null && x.chamCong.GioVao.HasValue 
                            ? x.chamCong.GioVao.Value.ToString("HH:mm:ss") : null,
                        GioRa = x.chamCong != null && x.chamCong.GioRa.HasValue 
                            ? x.chamCong.GioRa.Value.ToString("HH:mm:ss") : null,
                        SoGioLam = x.chamCong != null ? x.chamCong.SoGioLam : null,
                        DaCheckIn = x.chamCong != null && x.chamCong.GioVao.HasValue,
                        DaCheckOut = x.chamCong != null && x.chamCong.GioRa.HasValue
                    })
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    data = lichLamViec,
                    count = lichLamViec.Count,
                    message = lichLamViec.Count == 0 ? "Bạn không có lịch làm việc hôm nay" : "Lấy dữ liệu thành công",
                    debug = new { username = username, maNV = taikhoan.MaNV, today = today.ToString("yyyy-MM-dd") }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}", stackTrace = ex.StackTrace });
            }
        }

        // API: Check-in (Vào ca)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckPermission("ChamCong.Create")]
        public async Task<JsonResult> CheckIn(int maLich, string? toaDoGPS, string? ghiChu)
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                {
                    return Json(new { success = false, message = "Chưa đăng nhập!" });
                }

                var taikhoan = await _context.TaiKhoans
                    .FirstOrDefaultAsync(t => t.TenDangNhap == username);

                if (taikhoan == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy tài khoản!" });
                }

                // Kiểm tra lịch làm việc có thuộc về nhân viên này không
                var lichLamViec = await _context.LichLamViecs
                    .Include(l => l.CaLamViec)
                    .Include(l => l.ChamCongs)
                    .FirstOrDefaultAsync(l => l.MaLich == maLich && l.MaNV == taikhoan.MaNV);

                if (lichLamViec == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy lịch làm việc!" });
                }

                var chamCong = lichLamViec.ChamCongs?.FirstOrDefault();
                
                // Kiểm tra đã check-in chưa
                if (chamCong != null && chamCong.GioVao.HasValue)
                {
                    return Json(new 
                    { 
                        success = false, 
                        message = $"Bạn đã check-in lúc {chamCong.GioVao.Value:HH:mm:ss}!" 
                    });
                }

                var currentTime = DateTime.Now;

                // Nếu chưa có bản ghi chấm công, tạo mới
                if (chamCong == null)
                {
                    var newChamCong = new ChamCong
                    {
                        MaLich = maLich,
                        GioVao = currentTime,
                        ToaDoGPS_Vao = toaDoGPS,
                        GhiChu = ghiChu
                    };

                    _context.ChamCongs.Add(newChamCong);
                }
                else
                {
                    // Nếu đã có bản ghi nhưng chưa có GioVao (trường hợp đã checkout trước)
                    chamCong.GioVao = currentTime;
                    chamCong.ToaDoGPS_Vao = toaDoGPS;
                    if (!string.IsNullOrEmpty(ghiChu))
                    {
                        chamCong.GhiChu = ghiChu;
                    }
                    _context.ChamCongs.Update(chamCong);
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Check-in thành công!",
                    gioVao = currentTime.ToString("HH:mm:ss"),
                    ngay = currentTime.ToString("dd/MM/yyyy")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // API: Check-out (Tan ca)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckPermission("ChamCong.Edit")]
        public async Task<JsonResult> CheckOut(int maLich, string? toaDoGPS, string? ghiChu)
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                {
                    return Json(new { success = false, message = "Chưa đăng nhập!" });
                }

                var taikhoan = await _context.TaiKhoans
                    .FirstOrDefaultAsync(t => t.TenDangNhap == username);

                if (taikhoan == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy tài khoản!" });
                }

                // Kiểm tra lịch làm việc
                var lichLamViec = await _context.LichLamViecs
                    .Include(l => l.ChamCongs)
                    .FirstOrDefaultAsync(l => l.MaLich == maLich && l.MaNV == taikhoan.MaNV);

                if (lichLamViec == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy lịch làm việc!" });
                }

                var chamCong = lichLamViec.ChamCongs?.FirstOrDefault();
                
                // Kiểm tra đã check-in chưa
                if (chamCong == null || !chamCong.GioVao.HasValue)
                {
                    return Json(new { success = false, message = "Bạn chưa check-in! Vui lòng check-in trước." });
                }

                // Kiểm tra đã check-out chưa
                if (chamCong.GioRa.HasValue)
                {
                    return Json(new 
                    { 
                        success = false, 
                        message = $"Bạn đã check-out lúc {chamCong.GioRa.Value:HH:mm:ss}!" 
                    });
                }

                var currentTime = DateTime.Now;
                
                // Cập nhật giờ ra
                chamCong.GioRa = currentTime;
                chamCong.ToaDoGPS_Ra = toaDoGPS;
                
                // Cập nhật ghi chú nếu có
                if (!string.IsNullOrEmpty(ghiChu))
                {
                    chamCong.GhiChu = string.IsNullOrEmpty(chamCong.GhiChu) 
                        ? ghiChu 
                        : $"{chamCong.GhiChu}; {ghiChu}";
                }

                // Tính số giờ làm việc
                var soGioLam = (currentTime - chamCong.GioVao.Value).TotalHours;
                chamCong.SoGioLam = (decimal)Math.Round(soGioLam, 2);

                _context.ChamCongs.Update(chamCong);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Check-out thành công!",
                    gioRa = currentTime.ToString("HH:mm:ss"),
                    soGioLam = chamCong.SoGioLam,
                    ngay = currentTime.ToString("dd/MM/yyyy")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // API: Lấy lịch sử chấm công (7 ngày gần nhất)
        [HttpGet]
        [CheckPermission("ChamCong.View")]
        public async Task<JsonResult> GetHistory(int days = 7)
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                {
                    return Json(new { success = false, message = "Chưa đăng nhập!" });
                }

                var taikhoan = await _context.TaiKhoans
                    .FirstOrDefaultAsync(t => t.TenDangNhap == username);

                if (taikhoan == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy tài khoản!" });
                }

                var fromDate = DateTime.Today.AddDays(-days);
                
                var history = await _context.LichLamViecs
                    .Include(l => l.CaLamViec)
                    .Include(l => l.LoRung)
                    .Include(l => l.ChamCongs)
                    .Where(l => l.MaNV == taikhoan.MaNV 
                            && l.NgayLamViec >= fromDate 
                            && l.NgayLamViec <= DateTime.Today)
                    .OrderByDescending(l => l.NgayLamViec)
                    .ThenBy(l => l.CaLamViec!.GioBatDau)
                    .Select(l => new
                    {
                        chamCong = l.ChamCongs.FirstOrDefault(),
                        NgayLamViec = l.NgayLamViec.ToString("dd/MM/yyyy"),
                        TenCa = l.CaLamViec!.TenCa,
                        TenLo = l.LoRung != null 
                            ? $"TK{l.LoRung.SoTieuKhu}/K{l.LoRung.SoKhoanh}/L{l.LoRung.SoLo}" 
                            : "Chưa xác định"
                    })
                    .Select(x => new
                    {
                        x.NgayLamViec,
                        x.TenCa,
                        x.TenLo,
                        GioVao = x.chamCong != null && x.chamCong.GioVao.HasValue 
                            ? x.chamCong.GioVao.Value.ToString("HH:mm") : "---",
                        GioRa = x.chamCong != null && x.chamCong.GioRa.HasValue 
                            ? x.chamCong.GioRa.Value.ToString("HH:mm") : "---",
                        SoGioLam = x.chamCong != null && x.chamCong.SoGioLam.HasValue 
                            ? x.chamCong.SoGioLam.Value.ToString("0.0") + "h" : "---",
                        TrangThai = x.chamCong != null && x.chamCong.GioRa.HasValue 
                            ? "Hoàn thành" 
                            : (x.chamCong != null && x.chamCong.GioVao.HasValue 
                                ? "Đang làm" 
                                : "Chưa chấm công")
                    })
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    data = history,
                    count = history.Count
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // Trang quản lý chấm công (quyền được kiểm tra bởi CheckPermission)
        [CheckPermission("ChamCong.View")]
        public async Task<IActionResult> QuanLy()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        // API: Lấy tất cả lịch sử chấm công (quyền được kiểm tra bởi CheckPermission)
        [HttpGet]
        [CheckPermission("ChamCong.View")]
        public async Task<JsonResult> GetAllAttendance(DateTime? fromDate, DateTime? toDate, int? maNV, string? trangThai)
        {
            try
            {
                var from = fromDate ?? DateTime.Today.AddDays(-7);
                var to = toDate ?? DateTime.Today;
                
                var query = _context.LichLamViecs
                    .Include(l => l.CaLamViec)
                    .Include(l => l.LoRung)
                    .Include(l => l.ChamCongs)
                    .Include(l => l.NhanVien)
                    .Where(l => l.NgayLamViec >= from && l.NgayLamViec <= to);

                // Filter by nhân viên
                if (maNV.HasValue)
                {
                    query = query.Where(l => l.MaNV == maNV.Value);
                }

                // Filter by trạng thái
                if (!string.IsNullOrEmpty(trangThai))
                {
                    if (trangThai == "HoanThanh")
                    {
                        query = query.Where(l => l.ChamCongs.Any(c => c.GioRa.HasValue));
                    }
                    else if (trangThai == "DangLam")
                    {
                        query = query.Where(l => l.ChamCongs.Any(c => c.GioVao.HasValue && !c.GioRa.HasValue));
                    }
                    else if (trangThai == "ChuaCham")
                    {
                        query = query.Where(l => !l.ChamCongs.Any() || !l.ChamCongs.Any(c => c.GioVao.HasValue));
                    }
                }

                var data = await query
                    .OrderByDescending(l => l.NgayLamViec)
                    .ThenBy(l => l.CaLamViec!.GioBatDau)
                    .ThenBy(l => l.NhanVien!.HoTen)
                    .Select(l => new
                    {
                        chamCong = l.ChamCongs.FirstOrDefault(),
                        MaLich = l.MaLich,
                        MaNV = l.MaNV,
                        TenNV = l.NhanVien!.HoTen,
                        NgayLamViec = l.NgayLamViec.ToString("dd/MM/yyyy"),
                        TenCa = l.CaLamViec!.TenCa,
                        GioBatDau = l.CaLamViec.GioBatDau.ToString(@"hh\:mm"),
                        GioKetThuc = l.CaLamViec.GioKetThuc.ToString(@"hh\:mm"),
                        TenLo = l.LoRung != null 
                            ? $"TK{l.LoRung.SoTieuKhu}/K{l.LoRung.SoKhoanh}/L{l.LoRung.SoLo}" 
                            : "Chưa xác định"
                    })
                    .Select(x => new
                    {
                        x.MaLich,
                        x.MaNV,
                        x.TenNV,
                        x.NgayLamViec,
                        x.TenCa,
                        x.GioBatDau,
                        x.GioKetThuc,
                        x.TenLo,
                        GioVao = x.chamCong != null && x.chamCong.GioVao.HasValue 
                            ? x.chamCong.GioVao.Value.ToString("HH:mm") : null,
                        GioRa = x.chamCong != null && x.chamCong.GioRa.HasValue 
                            ? x.chamCong.GioRa.Value.ToString("HH:mm") : null,
                        SoGioLam = x.chamCong != null && x.chamCong.SoGioLam.HasValue 
                            ? x.chamCong.SoGioLam.Value : (decimal?)null,
                        TrangThai = x.chamCong != null && x.chamCong.GioRa.HasValue 
                            ? "Hoàn thành" 
                            : (x.chamCong != null && x.chamCong.GioVao.HasValue 
                                ? "Đang làm" 
                                : "Chưa chấm công"),
                        GhiChu = x.chamCong != null ? x.chamCong.GhiChu : null
                    })
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    data = data,
                    count = data.Count
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // API: Lấy danh sách nhân viên (quyền được kiểm tra bởi CheckPermission)
        [HttpGet]
        [CheckPermission("ChamCong.View")]
        public async Task<JsonResult> GetNhanVienList()
        {
            try
            {
                var nhanviens = await _context.NhanSus
                    .OrderBy(n => n.HoTen)
                    .Select(n => new
                    {
                        MaNV = n.MaNV,
                        HoTen = n.HoTen,
                        ChucVu = n.ChucVu
                    })
                    .ToListAsync();

                return Json(new { success = true, data = nhanviens });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
    }
}
