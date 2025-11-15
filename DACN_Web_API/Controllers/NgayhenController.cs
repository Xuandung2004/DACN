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
            if (request == null || request.Ngay == default)
                return BadRequest(new { message = "Dữ liệu lịch không hợp lệ" });

            var userId = TryGetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Vui lòng đăng nhập" });

            var user = _context.Nguoidungs.Find(userId.Value);
            if (user == null)
                return NotFound(new { message = "Người dùng không tồn tại" });

            // Prevent duplicate appointment on same date for the same user
            var sameDate = _context.Ngayhens.Any(x => x.NguoiDungId == userId.Value && x.Ngay.HasValue && x.Ngay.Value.Date == request.Ngay.Date);
            if (sameDate)
                return Conflict(new { message = "Bạn đã có lịch hẹn vào ngày này" });

            var lich = new Ngayhen
            {
                NguoiDungId = userId.Value,
                Ngay = request.Ngay
            };

            _context.Ngayhens.Add(lich);
            _context.SaveChanges();

            return CreatedAtAction(nameof(LayLichTheoId), new { id = lich.Id }, lich);
        }

        // Alternate create endpoint (thêm lịch) - alias for datlich
        [HttpPost("them")]
        public IActionResult ThemLich([FromBody] DatLichDto request) => DatLich(request);

        // GET: api/Ngayhen/cua-toi  (lấy tất cả lịch của user hiện tại)
        [HttpGet("cua-toi")]
        public IActionResult LayLichCuaToi()
        {
            var userId = TryGetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Vui lòng đăng nhập" });

            var lichs = _context.Ngayhens
                .Where(x => x.NguoiDungId == userId.Value)
                .OrderBy(x => x.Ngay)
                .ToList();

            return Ok(lichs);
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
            _context.SaveChanges();

            return Ok(new { message = "Cập nhật thành công", lich });
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
