using DACN_Web_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography; 
using System.Text;

namespace DACN_Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NguoiDungController : ControllerBase
    {
        private readonly CsdlFinal1Context db = new CsdlFinal1Context();

        private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

        // ------------------ DTOs ------------------
        public class NguoiDungCreateModel
        {
            public string HoTen { get; set; }
            public string TenDn { get; set; }
            public string MatKhau { get; set; }
            public string Email { get; set; }
            public string Sdt { get; set; }
            public string ViTri { get; set; } = "khachhang";
        }

        public class NguoiDungUpdateModel
        {
            public string HoTen { get; set; }
            public string Email { get; set; }
            public string Sdt { get; set; }
            public string ViTri { get; set; }
            public string TrangThai { get; set; }
        }

        public class ChangePasswordModel
        {
            public string MatKhauMoi { get; set; }
        }

        // ------------------------------------------
        // --- 1. Lấy danh sách Người dùng ---
        [HttpGet]
        public async Task<IActionResult> GetDanhSachNguoiDung()
        {
            try
            {
                var nguoiDungs = await db.Nguoidungs
                    .OrderBy(nd => nd.Id)
                    .Select(nd => new
                    {
                        nd.Id,
                        nd.HoTen,
                        nd.TenDn,
                        nd.Email,
                        nd.Sdt,
                        nd.ViTri,
                        nd.NgayTao,
                        nd.TrangThai,
                    })
                    .ToListAsync();

                return Ok(nguoiDungs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tải danh sách người dùng.", error = ex.Message });
            }
        }
        // ✅ Thêm endpoint GET theo ID để nút Sửa
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNguoiDungById(int id)
        {
            var nguoiDung = await db.Nguoidungs.FindAsync(id);
            if (nguoiDung == null)
                return NotFound(new { message = "Không tìm thấy người dùng" });

            return Ok(new
            {
                nguoiDung.Id,
                nguoiDung.HoTen,
                nguoiDung.TenDn,
                nguoiDung.Email,
                nguoiDung.Sdt,
                nguoiDung.ViTri,
                nguoiDung.TrangThai
            });
        }

        // --- 2. Thêm Người dùng mới ---
        [HttpPost]
        public async Task<IActionResult> ThemNguoiDung([FromBody] NguoiDungCreateModel model)
        {
            if (string.IsNullOrWhiteSpace(model.TenDn) || string.IsNullOrWhiteSpace(model.MatKhau))
                return BadRequest(new { message = "Tên đăng nhập và Mật khẩu không được để trống." });

            try
            {
                if (await db.Nguoidungs.AnyAsync(nd => nd.TenDn == model.TenDn))
                    return BadRequest(new { message = "Tên đăng nhập đã tồn tại." });

                if (!string.IsNullOrWhiteSpace(model.Email) &&
                    await db.Nguoidungs.AnyAsync(nd => nd.Email == model.Email))
                    return BadRequest(new { message = "Email đã tồn tại." });

                var nguoiDungMoi = new Nguoidung
                {
                    HoTen = model.HoTen,
                    TenDn = model.TenDn,
                    MatKhau = HashPassword(model.MatKhau),
                    Email = model.Email,
                    Sdt = model.Sdt,
                    ViTri = model.ViTri ?? "khachhang",
                    NgayTao = DateTime.Now,
                    TrangThai = "đang hoạt động"
                };

                db.Nguoidungs.Add(nguoiDungMoi);
                await db.SaveChangesAsync();

                return CreatedAtAction(nameof(ThemNguoiDung), new { id = nguoiDungMoi.Id }, new
                {
                    message = "Thêm người dùng thành công.",
                    nguoiDungMoi.Id,
                    nguoiDungMoi.HoTen,
                    nguoiDungMoi.TenDn,
                    nguoiDungMoi.Email
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi thêm người dùng mới.", error = ex.Message });
            }
        }

        // --- 4. Cập nhật thông tin cơ bản Người dùng ---
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNguoiDung(int id, [FromBody] NguoiDungUpdateModel model)
        {
            try
            {
                var nguoiDung = await db.Nguoidungs.FindAsync(id);
                if (nguoiDung == null)
                    return NotFound(new { message = "Không tìm thấy người dùng để cập nhật." });

                if (!string.IsNullOrWhiteSpace(model.HoTen))
                    nguoiDung.HoTen = model.HoTen;

                if (!string.IsNullOrWhiteSpace(model.Email))
                    nguoiDung.Email = model.Email;

                if (!string.IsNullOrWhiteSpace(model.Sdt))
                    nguoiDung.Sdt = model.Sdt;

                if (model.ViTri != null)
                    nguoiDung.ViTri = model.ViTri;

                if (model.TrangThai != null)
                    nguoiDung.TrangThai = model.TrangThai;

                await db.SaveChangesAsync();

                return Ok(new
                {
                    message = "Cập nhật người dùng thành công.",
                    Id = nguoiDung.Id,
                    HoTenMoi = nguoiDung.HoTen,
                    ViTriMoi = nguoiDung.ViTri,
                    TrangThaiMoi = nguoiDung.TrangThai
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật người dùng. Vui lòng kiểm tra log server.", error = ex.Message });
            }
        }

        // --- 6. Đổi mật khẩu ---
        [HttpPut("{id}/doimatkhau")]
        public async Task<IActionResult> DoiMatKhau(int id, [FromBody] ChangePasswordModel model)
        {
            if (string.IsNullOrWhiteSpace(model.MatKhauMoi) || model.MatKhauMoi.Length < 6)
            {
                return BadRequest(new { message = "Mật khẩu mới phải có ít nhất 6 ký tự." });
            }

            try
            {
                var nguoiDung = await db.Nguoidungs.FindAsync(id);
                if (nguoiDung == null)
                    return NotFound(new { message = "Không tìm thấy người dùng để đổi mật khẩu." });

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
