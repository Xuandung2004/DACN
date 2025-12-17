using DACN_Web_API.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DACN_Web_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly CsdlFinal1Context _context = new CsdlFinal1Context();

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            if (await _context.Nguoidungs.AnyAsync(u => u.TenDn == req.TenDn))
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại" });

            var user = new Nguoidung
            {
                TenDn = req.TenDn,
                MatKhau = HashPassword(req.MatKhau),
                Email = req.Email,
                Sdt = req.Sdt,
                HoTen = req.HoTen,
                ViTri = "khachhang",
                NgayTao = DateTime.Now,
                TrangThai = "đang hoạt động"
            };

            _context.Nguoidungs.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng ký thành công" });
        }

        //// POST: api/auth/login
        //[HttpPost("login")]
        //public async Task<IActionResult> Login([FromBody] LoginRequest req)
        //{
        //    var user = await _context.Nguoidungs
        //        .FirstOrDefaultAsync(u => u.TenDn == req.TenDn);

        //    if (user == null)
        //        return Unauthorized(new { message = "Sai tên đăng nhập hoặc mật khẩu" });

        //    if (user.MatKhau != HashPassword(req.MatKhau))
        //        return Unauthorized(new { message = "Sai tên đăng nhập hoặc mật khẩu" });

        //    if (user.TrangThai == "ngừng hoạt động")
        //        return Forbid("Tài khoản đã bị khóa");

        //    return Ok(new
        //    {
        //        message = "Đăng nhập thành công",
        //        user = new
        //        {
        //            user.Id,
        //            user.TenDn,
        //            user.Email,
        //            user.HoTen,
        //            user.ViTri
        //        }
        //    });
        //}



        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var user = await _context.Nguoidungs
                .FirstOrDefaultAsync(u => u.TenDn == req.TenDn);

            if (user == null)
                return Unauthorized(new { message = "Sai tên đăng nhập hoặc mật khẩu" });

            //kiểm tra mật khẩu(sha256)
            if (user.MatKhau != req.MatKhau && user.MatKhau != HashPassword(req.MatKhau))
                return Unauthorized(new { message = "Sai tên đăng nhập hoặc mật khẩu" });

            if (user.TrangThai == "ngừng hoạt động")
                return Forbid("Tài khoản đã bị khóa");

            // chuẩn hóa role (viTri) để tránh trường hợp viết khác
            var role = (user.ViTri ?? "").Trim().ToLower(); // ví dụ "admin", "khachhang", "nhanvien"

            // chọn redirectUrl tùy role
            string redirectUrl;
            var isAdmin = role.Contains("admin") || role.Contains("administrator") || role.Contains("quantri");
            var isEmployee = role.Contains("nhan") || role.Contains("nhân") || role.Contains("nv") || role.Contains("nhan_vien") || role.Contains("nhân_viên") || role.Contains("nhanvien");

            if (isAdmin)
            {
                redirectUrl = "/FrontendWeb/admin/index.html"; // admin dashboard
            }
            else if (isEmployee)
            {
                // nhân viên -> employee dashboard (no user management)
                redirectUrl = "/FrontendWeb/admin/indexnv.html";
            }
            else
            {
                redirectUrl = "/FrontendWeb/user/index.html"; // khách hàng
            }





            return Ok(new
            {
                message = "Đăng nhập thành công",
                user = new
                {
                    user.Id,
                    user.TenDn,
                    user.Email,
                    user.HoTen,
                    role = role
                },
                redirectUrl = redirectUrl

            });
        }



        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Xóa cookie
            Response.Cookies.Delete("DACN_Web_Auth");
            return Ok(new { message = "Đã đăng xuất" });
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }


        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int id))
                return Unauthorized();

            var user = _context.Nguoidungs.Find(id);
            if (user == null)
                return NotFound();

            return Ok(new { id = user.Id, hoTen = user.HoTen, tenDn = user.TenDn, role = user.ViTri?.Trim().ToLower() });
        }
    }
}
