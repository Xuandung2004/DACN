using DACN_Web_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace DACN_Web_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {

        
        private CsdlFinal1Context _context=new CsdlFinal1Context();

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

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var user = await _context.Nguoidungs
                .FirstOrDefaultAsync(u => u.TenDn == req.TenDn);

            if (user == null)
                return Unauthorized(new { message = "Sai tên đăng nhập hoặc mật khẩu" });

            if (user.MatKhau != HashPassword(req.MatKhau))
                return Unauthorized(new { message = "Sai tên đăng nhập hoặc mật khẩu" });

            if (user.TrangThai == "ngừng hoạt động")
                return Forbid("Tài khoản đã bị khóa");

            return Ok(new
            {
                message = "Đăng nhập thành công",
                user = new
                {
                    user.Id,
                    user.TenDn,
                    user.Email,
                    user.HoTen,
                    user.ViTri
                }
            });
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
