using DACN_Web_API.DTO;
using DACN_Web_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DACN_Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NgayhenController : ControllerBase
    {
        private readonly CsdlFinal1Context _context = new CsdlFinal1Context();

        // Try to obtain the current user id from JWT/claims first, then fallback to query string (userId) for testing.
        private int? TryGetCurrentUserId()
        {
            // 1) From claims (NameIdentifier or "sub")
            var idClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User?.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(idClaim) && int.TryParse(idClaim, out int idFromClaim))
                return idFromClaim;

            // 2) From query string for quick testing (e.g. ?userId=3)
            if (int.TryParse(HttpContext.Request.Query["userId"], out int idQuery))
                return idQuery;

            return null;
        }

        // POST: api/Ngayhen/datlich  (tạo lịch)
        [HttpPost("datlich")]
        public IActionResult DatLich([FromBody] DatLichDto request)
        {
            // 1. Kiểm tra dữ liệu đầu vào và User
            if (request == null || request.Ngay == default)
                return BadRequest(new { message = "Dữ liệu lịch không hợp lệ. Vui lòng chọn ngày và giờ." });

            var userId = TryGetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Vui lòng đăng nhập" });

            var user = _context.Nguoidungs.Find(userId.Value);
            if (user == null)
                return NotFound(new { message = "Người dùng không tồn tại" });

            // Kiểm tra trùng lịch
            var sameDate = _context.Ngayhens.Any(x =>
                x.NguoiDungId == userId.Value &&
                x.Ngay.HasValue &&
                x.Ngay.Value.Date == request.Ngay.Date
            );

            if (sameDate)
                return Conflict(new { message = "Bạn đã có lịch hẹn vào ngày này. Vui lòng chọn ngày khác." });

            // KHAI BÁO BIẾN LICH BÊN NGOÀI KHỐI TRY ĐỂ TRÁNH LỖI "DOES NOT EXIST"
            var lich = new Ngayhen
            {
                NguoiDungId = userId.Value,
                Ngay = request.Ngay
            };

            // 2. Lưu vào Database VÀ XỬ LÝ LỖI
            try
            {
                _context.Ngayhens.Add(lich);
                _context.SaveChanges();

                // Lỗi Tham chiếu Vòng lặp (Circular Reference) xảy ra ở đây.
                // Giải pháp 1: Cấu hình global trong Program.cs/Startup.cs
                // Giải pháp 2: Trả về đối tượng ẩn danh (Anonymous object) đã được tinh gọn
                return CreatedAtAction(nameof(LayLichTheoId), new { id = lich.Id }, new
                {
                    message = "Đặt lịch thành công!",
                    // Chỉ trả về các trường cơ bản để tránh vòng lặp JSON
                    lich = new
                    {
                        lich.Id,
                        lich.Ngay,
                        lich.NguoiDungId
                    }
                });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                // ... (Giữ nguyên logic bắt lỗi DB)
                Console.WriteLine($"\n***** LỖI DB CHI TIẾT (NGUYÊN NHÂN CUỐI CÙNG) *****\n{dbEx.InnerException?.Message}\n*************************\n");
                return StatusCode(500, new { error = true, message = "Lỗi Database: Thiếu trường bắt buộc hoặc lỗi kết nối. Vui lòng kiểm tra log backend." });
            }
            catch (Exception ex)
            {
                // ... (Giữ nguyên logic bắt lỗi chung)
                Console.WriteLine($"\n***** LỖI SERVER KHÔNG XÁC ĐỊNH *****\n{ex.Message}\n********************************\n");
                return StatusCode(500, new { error = true, message = "Lỗi máy chủ không xác định. Vui lòng kiểm tra log backend." });
            }
        }

        // Alternate create endpoint (thêm lịch) - alias for datlich
        [HttpPost("them")]
        public IActionResult ThemLich([FromBody] DatLichDto request) => DatLich(request);

        [HttpGet("cua-toi")]
        public IActionResult LayLichCuaToi()
        {
            var userId = TryGetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Vui lòng đăng nhập" });

            // 1. Lấy tất cả lịch hẹn của người dùng (bao gồm cả lịch đã qua)
            var tatCaLich = _context.Ngayhens
                .Where(x => x.NguoiDungId == userId.Value)
                .ToList(); // Tải dữ liệu vào bộ nhớ để xử lý

            // Lấy thời điểm hiện tại
            var now = DateTime.Now;

            // 2. Xác định các lịch đã quá hạn
            // Lịch quá hạn là lịch đã qua ngày hiện tại, kể cả giờ
            var lichDaQua = tatCaLich.Where(x => x.Ngay.HasValue && x.Ngay.Value < now).ToList();

            // 3. Xóa các lịch hẹn quá hạn
            if (lichDaQua.Any())
            {
                _context.Ngayhens.RemoveRange(lichDaQua);

                try
                {
                    _context.SaveChanges();
                    // Optional: Log ra console để biết đã xóa bao nhiêu lịch
                    Console.WriteLine($"Đã tự động xóa {lichDaQua.Count} lịch hẹn cũ cho User ID: {userId.Value}");
                }
                catch (Exception ex)
                {
                    // Xử lý nếu việc xóa thất bại (ví dụ: do ràng buộc khóa ngoại)
                    Console.WriteLine($"LỖI khi tự động xóa lịch cũ: {ex.Message}");
                    // Ta vẫn tiếp tục trả về danh sách lịch hợp lệ
                }
            }

            // 4. Trả về danh sách lịch còn hiệu lực (những lịch không nằm trong danh sách đã xóa)
            var lichConHieuLuc = tatCaLich.Except(lichDaQua).ToList();

            if (lichConHieuLuc.Any())
            {
                // Trả về danh sách lịch còn lại
                return Ok(lichConHieuLuc);
            }

            return NotFound(new { message = "Bạn chưa có lịch hẹn nào còn hiệu lực." });
        }

        // GET: api/Ngayhen/{id}  (lấy 1 lịch theo id)
        [HttpGet("{id:int}")]
        public IActionResult LayLichTheoId(int id)
        {
            var userId = TryGetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Vui lòng đăng nhập" });

            var lich = _context.Ngayhens.FirstOrDefault(x => x.Id == id && x.NguoiDungId == userId.Value);
            if (lich == null)
                return NotFound(new { message = "Không tìm thấy lịch hẹn" });

            return Ok(lich);
        }

        // PUT: api/Ngayhen/update/{id}  (cập nhật ngày của lịch)
        [HttpPut("update/{id:int}")]
        public IActionResult CapNhatLich(int id, [FromBody] DatLichDto request)
        {
            if (request == null || request.Ngay == default)
                return BadRequest(new { message = "Dữ liệu lịch không hợp lệ" });

            var userId = TryGetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Vui lòng đăng nhập" });

            var lich = _context.Ngayhens.FirstOrDefault(x => x.Id == id && x.NguoiDungId == userId.Value);
            if (lich == null)
                return NotFound(new { message = "Không tìm thấy lịch hẹn" });

            // Kiểm tra trùng lịch với các lịch khác của người dùng
            var conflict = _context.Ngayhens.Any(x => x.NguoiDungId == userId.Value && x.Id != id && x.Ngay.HasValue && x.Ngay.Value.Date == request.Ngay.Date);
            if (conflict)
                return Conflict(new { message = "Đã tồn tại lịch hẹn vào ngày này" });

            lich.Ngay = request.Ngay;

            try
            {
                _context.SaveChanges();
                return Ok(new { message = "Cập nhật thành công", lich });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi cập nhật
                Console.WriteLine($"\n***** LỖI CẬP NHẬT DB *****\n{ex.Message}\n*************************\n");
                return StatusCode(500, new { error = true, message = "Lỗi Database khi cập nhật." });
            }
        }

        // DELETE: api/Ngayhen/huy/{id}  (hủy 1 lịch theo id thuộc user hiện tại)
        [HttpDelete("huy/{id:int}")]
        public IActionResult HuyLich(int id)
        {
            var userId = TryGetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Vui lòng đăng nhập" });

            var lich = _context.Ngayhens.FirstOrDefault(x => x.Id == id && x.NguoiDungId == userId.Value);
            if (lich == null)
                return NotFound(new { message = "Không tìm thấy lịch hẹn" });

            _context.Ngayhens.Remove(lich);
            _context.SaveChanges();

            return Ok(new { message = "Đã hủy lịch hẹn" });
        }
    }
}
