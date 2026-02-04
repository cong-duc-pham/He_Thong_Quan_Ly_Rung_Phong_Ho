using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyRungPhongHo.Data;
using QuanLyRungPhongHo.Models;
using QuanLyRungPhongHo.Validators;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyRungPhongHo.Controllers
{
    [Authorize]
    public class CaLamViecController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CaLamViecController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị trang quản lý ca làm việc
        public async Task<IActionResult> Index()
        {
            try
            {
                // Client-side sẽ load data qua API
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi tải trang: {ex.Message}";
                return View();
            }
        }

        // API lấy danh sách ca làm việc
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                Console.WriteLine("GetAll API called");

                var caLamViecs = await _context.CaLamViecs
                    .OrderBy(c => c.GioBatDau)
                    .ThenBy(c => c.TenCa)
                    .Select(c => new
                    {
                        maCa = c.MaCa,
                        tenCa = c.TenCa,
                        gioBatDau = c.GioBatDau.ToString(@"hh\:mm"),
                        gioKetThuc = c.GioKetThuc.ToString(@"hh\:mm"),
                        moTa = c.MoTa ?? "",
                        trangThai = c.TrangThai
                    })
                    .ToListAsync();

                Console.WriteLine($"Tìm thấy {caLamViecs.Count} ca làm việc");

                return Json(new { success = true, data = caLamViecs });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong GetAll: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Lỗi tải dữ liệu: {ex.Message}" });
            }
        }

        // Tạo mới ca làm việc
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] CaLamViecRequest request)
        {
            try
            {
                Console.WriteLine("Create API called");
                Console.WriteLine($"Request: {request.TenCa}, {request.GioBatDau} - {request.GioKetThuc}");

                // Kiểm tra null
                if (request == null || string.IsNullOrWhiteSpace(request.TenCa) ||
                    string.IsNullOrWhiteSpace(request.GioBatDau) || string.IsNullOrWhiteSpace(request.GioKetThuc))
                {
                    return Json(new { success = false, message = "Thông tin ca làm việc không được để trống!" });
                }

                // Validate dữ liệu
                var validationResult = CaLamViecValidator.ValidateCaLamViec(
                    request.TenCa,
                    request.GioBatDau,
                    request.GioKetThuc,
                    request.MoTa
                );

                if (!validationResult.IsValid)
                {
                    return Json(new { success = false, message = validationResult.ErrorMessage });
                }

                // Chuẩn hóa dữ liệu
                var tenCa = CaLamViecValidator.NormalizeTenCa(request.TenCa);
                var gioBatDau = CaLamViecValidator.ParseTime(request.GioBatDau);
                var gioKetThuc = CaLamViecValidator.ParseTime(request.GioKetThuc);

                // Kiểm tra trùng lặp tên ca (case-insensitive)
                var existingByName = await _context.CaLamViecs
                    .AnyAsync(c => c.TenCa.ToLower() == tenCa.ToLower());

                if (existingByName)
                {
                    return Json(new { success = false, message = $"Ca làm việc '{tenCa}' đã tồn tại!" });
                }

                // Kiểm tra trùng lặp giờ - Chỉ kiểm tra với ca đang hoạt động
                var overlappingShift = await _context.CaLamViecs
                    .Where(c => c.TrangThai)
                    .FirstOrDefaultAsync(c =>
                        (gioBatDau >= c.GioBatDau && gioBatDau < c.GioKetThuc) ||
                        (gioKetThuc > c.GioBatDau && gioKetThuc <= c.GioKetThuc) ||
                        (gioBatDau <= c.GioBatDau && gioKetThuc >= c.GioKetThuc)
                    );

                if (overlappingShift != null)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Khung giờ này trùng với ca '{overlappingShift.TenCa}' ({overlappingShift.GioBatDau:hh\\:mm} - {overlappingShift.GioKetThuc:hh\\:mm})!"
                    });
                }

                // Tạo mới ca làm việc
                var caLamViec = new CaLamViec
                {
                    TenCa = tenCa,
                    GioBatDau = gioBatDau,
                    GioKetThuc = gioKetThuc,
                    MoTa = string.IsNullOrWhiteSpace(request.MoTa) ? null : request.MoTa.Trim(),
                    TrangThai = true
                };

                _context.CaLamViecs.Add(caLamViec);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Đã tạo ca làm việc: ID={caLamViec.MaCa}");

                return Json(new
                {
                    success = true,
                    message = $"Đã thêm ca làm việc '{tenCa}' thành công!",
                    data = new
                    {
                        maCa = caLamViec.MaCa,
                        tenCa = caLamViec.TenCa,
                        gioBatDau = caLamViec.GioBatDau.ToString(@"hh\:mm"),
                        gioKetThuc = caLamViec.GioKetThuc.ToString(@"hh\:mm"),
                        moTa = caLamViec.MoTa ?? "",
                        trangThai = caLamViec.TrangThai
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong Create: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // Cập nhật ca làm việc
        [HttpPut]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([FromBody] CaLamViecRequest request)
        {
            try
            {
                Console.WriteLine("Update API called");
                Console.WriteLine($"Request: MaCa={request.MaCa}, TenCa={request.TenCa}");

                if (!request.MaCa.HasValue || request.MaCa.Value <= 0)
                {
                    return Json(new { success = false, message = "Thiếu mã ca hợp lệ!" });
                }

                var caLamViec = await _context.CaLamViecs.FindAsync(request.MaCa.Value);
                if (caLamViec == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy ca làm việc!" });
                }

                // Kiểm tra null
                if (string.IsNullOrWhiteSpace(request.TenCa) ||
                    string.IsNullOrWhiteSpace(request.GioBatDau) ||
                    string.IsNullOrWhiteSpace(request.GioKetThuc))
                {
                    return Json(new { success = false, message = "Thông tin ca làm việc không được để trống!" });
                }

                // Validate dữ liệu
                var validationResult = CaLamViecValidator.ValidateCaLamViec(
                    request.TenCa,
                    request.GioBatDau,
                    request.GioKetThuc,
                    request.MoTa
                );

                if (!validationResult.IsValid)
                {
                    return Json(new { success = false, message = validationResult.ErrorMessage });
                }

                // Chuẩn hóa dữ liệu
                var tenCa = CaLamViecValidator.NormalizeTenCa(request.TenCa);
                var gioBatDau = CaLamViecValidator.ParseTime(request.GioBatDau);
                var gioKetThuc = CaLamViecValidator.ParseTime(request.GioKetThuc);

                // Kiểm tra trùng tên (loại trừ bản thân)
                var existingByName = await _context.CaLamViecs
                    .AnyAsync(c => c.MaCa != request.MaCa.Value && c.TenCa.ToLower() == tenCa.ToLower());

                if (existingByName)
                {
                    return Json(new { success = false, message = $"Ca làm việc '{tenCa}' đã tồn tại!" });
                }

                // Kiểm tra trùng giờ (loại trừ bản thân, chỉ check với ca đang hoạt động)
                var overlappingShift = await _context.CaLamViecs
                    .Where(c => c.MaCa != request.MaCa.Value && c.TrangThai)
                    .FirstOrDefaultAsync(c =>
                        (gioBatDau >= c.GioBatDau && gioBatDau < c.GioKetThuc) ||
                        (gioKetThuc > c.GioBatDau && gioKetThuc <= c.GioKetThuc) ||
                        (gioBatDau <= c.GioBatDau && gioKetThuc >= c.GioKetThuc)
                    );

                if (overlappingShift != null)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Khung giờ này trùng với ca '{overlappingShift.TenCa}' ({overlappingShift.GioBatDau:hh\\:mm} - {overlappingShift.GioKetThuc:hh\\:mm})!"
                    });
                }

                // Cập nhật
                caLamViec.TenCa = tenCa;
                caLamViec.GioBatDau = gioBatDau;
                caLamViec.GioKetThuc = gioKetThuc;
                caLamViec.MoTa = string.IsNullOrWhiteSpace(request.MoTa) ? null : request.MoTa.Trim();

                _context.CaLamViecs.Update(caLamViec);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Đã cập nhật ca làm việc: ID={caLamViec.MaCa}");

                return Json(new
                {
                    success = true,
                    message = $"Đã cập nhật ca làm việc '{tenCa}' thành công!",
                    data = new
                    {
                        maCa = caLamViec.MaCa,
                        tenCa = caLamViec.TenCa,
                        gioBatDau = caLamViec.GioBatDau.ToString(@"hh\:mm"),
                        gioKetThuc = caLamViec.GioKetThuc.ToString(@"hh\:mm"),
                        moTa = caLamViec.MoTa ?? "",
                        trangThai = caLamViec.TrangThai
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong Update: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // Bật/tắt trạng thái ca làm việc
        [HttpPut]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus([FromBody] int maCa)
        {
            try
            {
                Console.WriteLine($"ToggleStatus API called for MaCa={maCa}");

                if (maCa <= 0)
                {
                    return Json(new { success = false, message = "Mã ca không hợp lệ!" });
                }

                var caLamViec = await _context.CaLamViecs.FindAsync(maCa);
                if (caLamViec == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy ca làm việc!" });
                }

                // Toggle trạng thái
                caLamViec.TrangThai = !caLamViec.TrangThai;
                _context.CaLamViecs.Update(caLamViec);
                await _context.SaveChangesAsync();

                var statusText = caLamViec.TrangThai ? "Đã kích hoạt" : "Đã vô hiệu hóa";
                Console.WriteLine($"Đã chuyển trạng thái: {statusText}");

                return Json(new
                {
                    success = true,
                    message = $"{statusText} ca làm việc '{caLamViec.TenCa}'!",
                    trangThai = caLamViec.TrangThai
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong ToggleStatus: {ex.Message}");
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // Xóa ca làm việc
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromBody] int maCa)
        {
            try
            {
                Console.WriteLine($"Delete API called for MaCa={maCa}");

                if (maCa <= 0)
                {
                    return Json(new { success = false, message = "Mã ca không hợp lệ!" });
                }

                var caLamViec = await _context.CaLamViecs.FindAsync(maCa);
                if (caLamViec == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy ca làm việc!" });
                }

                // Kiểm tra xem ca có đang được sử dụng trong lịch làm việc không
                var isUsed = await _context.LichLamViecs.AnyAsync(l => l.MaCa == maCa);
                if (isUsed)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không thể xóa ca làm việc này vì đã có lịch phân công! Vui lòng vô hiệu hóa thay vì xóa."
                    });
                }

                _context.CaLamViecs.Remove(caLamViec);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Đã xóa ca làm việc: ID={maCa}");

                return Json(new
                {
                    success = true,
                    message = $"Đã xóa ca làm việc '{caLamViec.TenCa}' thành công!"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong Delete: {ex.Message}");
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
    }

    // Request model
    public class CaLamViecRequest
    {
        public int? MaCa { get; set; }
        public string TenCa { get; set; }
        public string GioBatDau { get; set; }
        public string GioKetThuc { get; set; }
        public string MoTa { get; set; }
    }
}