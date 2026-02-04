using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyRungPhongHo.Data;
using QuanLyRungPhongHo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyRungPhongHo.Controllers
{
    public class LichLamViecController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LichLamViecController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Load danh sách nhân viên sẵn vào View
            var employees = await _context.NhanSus
                .Select(n => new
                {
                    id = n.MaNV,
                    name = n.HoTen,
                    role = n.ChucVu
                })
                .ToListAsync();
            
            ViewBag.Employees = employees;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployees()
        {
            try
            {
                var employees = await _context.NhanSus
                    .Select(n => new
                    {
                        id = n.MaNV,
                        name = n.HoTen,
                        role = n.ChucVu
                    })
                    .ToListAsync();

                return Json(new { success = true, data = employees });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetShifts()
        {
            try
            {
                var shifts = await _context.CaLamViecs
                    .Where(c => c.TrangThai == true)
                    .Select(c => new
                    {
                        id = c.MaCa,
                        name = c.TenCa,
                        startTime = c.GioBatDau.ToString(@"hh\:mm"),
                        endTime = c.GioKetThuc.ToString(@"hh\:mm"),
                        description = c.MoTa
                    })
                    .ToListAsync();

                return Json(new { success = true, data = shifts });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSchedule(DateTime weekStart)
        {
            try
            {
                // ✅ Đảm bảo weekStart là Thứ Hai
                weekStart = GetMondayOfWeek(weekStart);
                var weekEnd = weekStart.AddDays(6);

                Console.WriteLine($"=== GetSchedule ===");
                Console.WriteLine($"WeekStart (Monday): {weekStart:yyyy-MM-dd ddd}");
                Console.WriteLine($"WeekEnd (Sunday): {weekEnd:yyyy-MM-dd ddd}");

                var schedules = await (
                    from l in _context.LichLamViecs
                    join n in _context.NhanSus on l.MaNV equals n.MaNV
                    join c in _context.CaLamViecs on l.MaCa equals c.MaCa
                    where l.NgayLamViec >= weekStart && l.NgayLamViec <= weekEnd
                    select new
                    {
                        MaLich = l.MaLich,
                        MaNV = l.MaNV,
                        MaCa = l.MaCa,
                        NgayLamViec = l.NgayLamViec,
                        HoTen = n.HoTen ?? "Unknown",
                        TenCa = c.TenCa ?? "Ca làm việc"
                    }
                ).ToListAsync();

                Console.WriteLine($"Found {schedules.Count} schedules:");
                foreach (var s in schedules)
                {
                    Console.WriteLine($"  - {s.NgayLamViec:yyyy-MM-dd ddd} | Ca {s.MaCa} | {s.HoTen}");
                }

                var groupedSchedule = schedules
                    .GroupBy(s => new { DayOfWeek = GetDayOfWeekString(s.NgayLamViec), s.MaCa })
                    .Select(g => new
                    {
                        day = g.Key.DayOfWeek,
                        shiftId = g.Key.MaCa,
                        employees = g.Select(s => new
                        {
                            id = s.MaNV,
                            name = s.HoTen,
                            scheduleId = s.MaLich
                        }).ToList()
                    })
                    .ToList();

                return Json(new { success = true, schedule = groupedSchedule });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR in GetSchedule: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveSchedule([FromBody] ScheduleRequest request)
        {
            try
            {
                if (request == null || request.Schedule == null)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
                }

                // ✅ Đảm bảo weekStart là Thứ Hai
                var weekStart = GetMondayOfWeek(request.WeekStart.Date);
                var weekEnd = weekStart.AddDays(6);
                var currentUserId = GetCurrentUserId();

                Console.WriteLine($"=== SaveSchedule ===");
                Console.WriteLine($"Received WeekStart: {request.WeekStart:yyyy-MM-dd ddd}");
                Console.WriteLine($"Adjusted to Monday: {weekStart:yyyy-MM-dd ddd}");
                Console.WriteLine($"WeekEnd (Sunday): {weekEnd:yyyy-MM-dd ddd}");

                int addedCount = 0;
                int deletedCount = 0;

                var existingSchedules = await _context.LichLamViecs
                    .Where(l => l.NgayLamViec >= weekStart && l.NgayLamViec <= weekEnd)
                    .ToListAsync();

                Console.WriteLine($"Existing schedules in DB: {existingSchedules.Count}");
                Console.WriteLine($"Received cells from client: {request.Schedule.Count}");

                foreach (var item in request.Schedule)
                {
                    if (item == null || item.Employees == null)
                    {
                        continue;
                    }

                    // ✅ Tính ngày chính xác từ weekStart (Monday)
                    var date = GetDateFromDayOfWeek(weekStart, item.Day);

                    Console.WriteLine($"Processing: {item.Day.ToUpper()} → {date:yyyy-MM-dd ddd} | Ca {item.ShiftId} | {item.Employees.Count} employees");

                    var existingInCell = existingSchedules
                        .Where(s => s.NgayLamViec == date && s.MaCa == item.ShiftId)
                        .ToList();

                    var existingEmployeeIds = existingInCell.Select(s => s.MaNV).ToHashSet();
                    var newEmployeeIds = item.Employees.Select(e => e.Id).ToHashSet();

                    // Xóa nhân viên không còn trong ô
                    var toDelete = existingInCell.Where(s => !newEmployeeIds.Contains(s.MaNV)).ToList();
                    if (toDelete.Any())
                    {
                        _context.LichLamViecs.RemoveRange(toDelete);
                        deletedCount += toDelete.Count;
                        Console.WriteLine($"  ❌ Deleting {toDelete.Count} employees");
                    }

                    // Thêm nhân viên mới
                    foreach (var employee in item.Employees)
                    {
                        if (!existingEmployeeIds.Contains(employee.Id))
                        {
                            var newSchedule = new LichLamViec
                            {
                                MaNV = employee.Id,
                                MaCa = item.ShiftId,
                                NgayLamViec = date,
                                TrangThai = "Đã phân công",
                                GhiChu = "",
                                NgayTao = DateTime.Now,
                                NguoiTao = currentUserId,
                                MaLo = null
                            };

                            _context.LichLamViecs.Add(newSchedule);
                            addedCount++;
                            Console.WriteLine($"  ✅ Adding: NV{employee.Id} on {date:yyyy-MM-dd ddd}");
                        }
                    }
                }

                await _context.SaveChangesAsync();

                Console.WriteLine($"💾 SAVED: +{addedCount} new, -{deletedCount} deleted");

                return Json(new
                {
                    success = true,
                    message = $"Đã lưu thành công! (Thêm: {addedCount}, Xóa: {deletedCount})",
                    added = addedCount,
                    deleted = deletedCount
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                
                // Log inner exception if exists
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"❌ INNER ERROR: {ex.InnerException.Message}");
                    if (ex.InnerException.InnerException != null)
                    {
                        Console.WriteLine($"❌ INNER INNER ERROR: {ex.InnerException.InnerException.Message}");
                    }
                }
                
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ClearWeekSchedule([FromBody] ClearWeekRequest request)
        {
            try
            {
                var weekStart = GetMondayOfWeek(request.WeekStart.Date);
                var weekEnd = weekStart.AddDays(6);

                var schedules = await _context.LichLamViecs
                    .Where(l => l.NgayLamViec >= weekStart && l.NgayLamViec <= weekEnd)
                    .ToListAsync();

                if (schedules.Any())
                {
                    _context.LichLamViecs.RemoveRange(schedules);
                    await _context.SaveChangesAsync();
                }

                return Json(new
                {
                    success = true,
                    message = $"Đã xóa {schedules.Count} ca làm việc"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteScheduleItem([FromBody] DeleteScheduleItemRequest request)
        {
            try
            {
                var schedule = await _context.LichLamViecs.FindAsync(request.ScheduleId);

                if (schedule == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy lịch làm việc" });
                }

                _context.LichLamViecs.Remove(schedule);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Đã xóa nhân viên khỏi ca làm việc"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ✅ Helper: Lấy ngày Thứ Hai của tuần
        private DateTime GetMondayOfWeek(DateTime date)
        {
            date = date.Date; // Reset time to 00:00:00

            while (date.DayOfWeek != DayOfWeek.Monday)
            {
                date = date.AddDays(-1);
            }

            return date;
        }

        // ✅ Helper: Convert DateTime → "mon", "tue", ...
        private string GetDayOfWeekString(DateTime date)
        {
            return date.DayOfWeek switch
            {
                DayOfWeek.Monday => "mon",
                DayOfWeek.Tuesday => "tue",
                DayOfWeek.Wednesday => "wed",
                DayOfWeek.Thursday => "thu",
                DayOfWeek.Friday => "fri",
                DayOfWeek.Saturday => "sat",
                DayOfWeek.Sunday => "sun",
                _ => "mon"
            };
        }

        // ✅ Helper: Convert "mon" → DateTime (based on weekStart = Monday)
        private DateTime GetDateFromDayOfWeek(DateTime monday, string day)
        {
            // Đảm bảo monday thực sự là Thứ Hai
            if (monday.DayOfWeek != DayOfWeek.Monday)
            {
                monday = GetMondayOfWeek(monday);
            }

            var offset = day.ToLower() switch
            {
                "mon" => 0,
                "tue" => 1,
                "wed" => 2,
                "thu" => 3,
                "fri" => 4,
                "sat" => 5,
                "sun" => 6,
                _ => 0
            };

            return monday.AddDays(offset);
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("MaNV")?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            return null;
        }
    }

    // Request models
    public class ScheduleRequest
    {
        public DateTime WeekStart { get; set; }
        public int WeekNumber { get; set; }
        public List<ScheduleItem> Schedule { get; set; }
    }

    public class ScheduleItem
    {
        public string Day { get; set; }
        public int ShiftId { get; set; }
        public List<EmployeeItem> Employees { get; set; } = new List<EmployeeItem>();
    }

    public class EmployeeItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ClearWeekRequest
    {
        public DateTime WeekStart { get; set; }
    }

    public class DeleteScheduleItemRequest
    {
        public int ScheduleId { get; set; }
    }
}