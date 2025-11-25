using DACN_Web_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography; 
using System.Text;
using System.Threading.Tasks;

namespace DACN_Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaiKhoanController : ControllerBase
    {
        
        private readonly CsdlFinal1Context db = new CsdlFinal1Context();

        public class TaiKhoanUpdateModel
        {
            public string HoTen { get; set; }
            public string Email { get; set; }
            public string Sdt { get; set; }
        }

        public class TaiKhoanChangePasswordModel
        {
            public string MatKhauCu { get; set; }
            public string MatKhauMoi { get; set; }
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        // ------------------------------------------

        // --- 1. Lấy thông tin chi tiết tài khoản ---
        [HttpGet("{id}")]
        public async Task<IActionResult> GetThongTinCaNhan(int id)
        {
            try
            {
                var nguoiDung = await db.Nguoidungs.FindAsync(id);
                
                if (nguoiDung == null)
                {
                    return NotFound(new { message = "Không tìm thấy tài khoản." });
                }

                // Kiểm tra vai trò: Chỉ cho phép khách hàng
                if (nguoiDung.ViTri != "khachhang")
                {
                    return NotFound(new { message = "Không tìm thấy tài khoản khách hàng với ID này." });
                }

                return Ok(new
                {
                    nguoiDung.Id,
                    nguoiDung.HoTen,
                    nguoiDung.TenDn,
                    nguoiDung.Email,
                    nguoiDung.Sdt,
                    nguoiDung.ViTri,
                    nguoiDung.NgayTao,
                    nguoiDung.TrangThai
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi kết nối cơ sở dữ liệu.", error = ex.Message });
            }
        }

        // --- 2. Cập nhật thông tin cá nhân ---
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateThongTinCaNhan(int id, [FromBody] TaiKhoanUpdateModel model)
        {
            if (string.IsNullOrWhiteSpace(model.HoTen) || 
                string.IsNullOrWhiteSpace(model.Email) || 
                string.IsNullOrWhiteSpace(model.Sdt))
            {
                return BadRequest(new { message = "Họ tên, Email và SĐT không được để trống." });
            }

            try
            {
                var nguoiDung = await db.Nguoidungs.FindAsync(id);
                
                if (nguoiDung == null)
                {
                    return NotFound(new { message = "Không tìm thấy tài khoản để cập nhật." });
                }
                
                // Kiểm tra vai trò: Chỉ cho phép khách hàng
                if (nguoiDung.ViTri != "khachhang")
                {
                    return NotFound(new { message = "Không thể cập nhật tài khoản này." });
                }

                // Kiểm tra Email trùng lặp
                if (model.Email != nguoiDung.Email &&
                    await db.Nguoidungs.AnyAsync(nd => nd.Email == model.Email && nd.Id != id))
                {
                    return BadRequest(new { message = "Email này đã được sử dụng bởi một tài khoản khác." });
                }

                if (model.Sdt != nguoiDung.Sdt &&
                    await db.Nguoidungs.AnyAsync(nd => nd.Sdt == model.Sdt && nd.Id != id))
                {
                    return BadRequest(new { message = "Số điện thoại này đã được sử dụng bởi một tài khoản khác." });
                }

                // Cập nhật các trường được phép
                nguoiDung.HoTen = model.HoTen;
                nguoiDung.Email = model.Email;
                nguoiDung.Sdt = model.Sdt;

                await db.SaveChangesAsync();
                
                return Ok(new { message = "Cập nhật thông tin thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật thông tin.", error = ex.Message });
            }
        }

        // --- 3. Người dùng tự đổi mật khẩu ---
        [HttpPut("{id}/doimatkhau")]
        public async Task<IActionResult> DoiMatKhauCaNhan(int id, [FromBody] TaiKhoanChangePasswordModel model)
        {
            // (Giữ nguyên các validation ban đầu)
            if (string.IsNullOrWhiteSpace(model.MatKhauMoi) || model.MatKhauMoi.Length < 6)
            {
                return BadRequest(new { message = "Mật khẩu mới phải có ít nhất 6 ký tự." });
            }
            if (string.IsNullOrWhiteSpace(model.MatKhauCu))
            {
                return BadRequest(new { message = "Vui lòng nhập mật khẩu cũ của bạn." });
            }

            try
            {
                var nguoiDung = await db.Nguoidungs.FindAsync(id);
                
                if (nguoiDung == null)
                {
                    return NotFound(new { message = "Không tìm thấy tài khoản." });
                }

                // Kiểm tra vai trò: Chỉ cho phép khách hàng
                if (nguoiDung.ViTri != "khachhang")
                {
                    return NotFound(new { message = "Không thể đổi mật khẩu cho tài khoản này." });
                }

                // LỖI ĐÃ XẢY RA Ở ĐÂY: Phải hash MatKhauCu trước khi so sánh
                string hashedMatKhauCu = HashPassword(model.MatKhauCu);

                // Kiểm tra mật khẩu cũ
                if (nguoiDung.MatKhau != hashedMatKhauCu)
                {
                    // Đã sửa: Thông báo sẽ đúng nếu mật khẩu không khớp.
                    return BadRequest(new { message = "Mật khẩu cũ không chính xác." });
                }

                // Cập nhật mật khẩu mới (Phải hash mật khẩu mới trước khi lưu)
                // Đã sửa: Hash mật khẩu mới
                nguoiDung.MatKhau = HashPassword(model.MatKhauMoi);
                await db.SaveChangesAsync();

                return Ok(new { message = "Đổi mật khẩu thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi đổi mật khẩu.", error = ex.Message });
            }
        }
    }
}