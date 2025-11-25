using DACN_Web_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace DACN_Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonHangController : ControllerBase
    {
        private readonly CsdlFinal1Context db = new CsdlFinal1Context();

        // ✅ 1. Lấy danh sách tất cả đơn hàng
        [HttpGet]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 500)]
        public IActionResult GetAllDonHang()
        {
            try
            {
                var donHangs = db.Donhangs
                    .Include(d => d.NguoiDung)
                    .Include(d => d.DiaChi)
                    .OrderByDescending(d => d.NgayDat)
                    .Select(d => new
                    {
                        d.Id,
                        d.NgayDat,
                        d.TrangThai,
                        d.GhiChu,
                        d.TongTien,
                        TenNguoiDung = d.NguoiDung != null ? d.NguoiDung.HoTen : null,
                        DiaChiGiaoHang = d.DiaChi != null ? d.DiaChi.DiaChiNhan : null
                    })
                    .ToList();

                return Ok(donHangs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách đơn hàng.", error = ex.Message });
            }
        }

        // ✅ 2. Xem chi tiết 1 đơn hàng
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public IActionResult GetDonHangById(int id)
        {
            try
            {
                var donHang = db.Donhangs
                    .Include(d => d.NguoiDung)
                    .Include(d => d.DiaChi)
                    .Include(d => d.DonhangChitiets)
                        .ThenInclude(ct => ct.SanPham)
                    .Include(d => d.DonhangChitiets)
                        .ThenInclude(ct => ct.KichThuoc)
                    .FirstOrDefault(d => d.Id == id);

                if (donHang == null)
                    return NotFound(new { message = "Không tìm thấy đơn hàng." });

                var result = new
                {
                    donHang.Id,
                    donHang.NgayDat,
                    donHang.TrangThai,
                    donHang.GhiChu,
                    donHang.TongTien,
                    TenNguoiDung = donHang.NguoiDung?.HoTen,
                    DiaChiGiaoHang = donHang.DiaChi?.DiaChiNhan,
                    ChiTiet = donHang.DonhangChitiets.Select(ct => new
                    {
                        SanPham = ct.SanPham?.TenSp,
                        KichThuoc = ct.KichThuoc?.SoLieu,
                        ct.SoLuong,
                        ct.Gia
                    })
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy thông tin đơn hàng.", error = ex.Message });
            }
        }

        // ✅ 3. Cập nhật trạng thái đơn hàng
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public IActionResult UpdateTrangThai(int id, [FromBody] string trangThaiMoi)
        {
            if (string.IsNullOrWhiteSpace(trangThaiMoi))
                return BadRequest(new { message = "Trạng thái mới không được để trống." });

            try
            {
                var donHang = db.Donhangs.FirstOrDefault(d => d.Id == id);
                if (donHang == null)
                    return NotFound(new { message = "Không tìm thấy đơn hàng để cập nhật." });

                donHang.TrangThai = trangThaiMoi;
                db.SaveChanges();

                // Load lại đơn hàng sau khi cập nhật
                var updated = db.Donhangs
                    .Include(d => d.NguoiDung)
                    .Include(d => d.DiaChi)
                    .Include(d => d.DonhangChitiets)
                        .ThenInclude(ct => ct.SanPham)
                    .Include(d => d.DonhangChitiets)
                        .ThenInclude(ct => ct.KichThuoc)
                    .FirstOrDefault(d => d.Id == id);

                return Ok(new
                {
                    message = "Cập nhật trạng thái thành công.",
                    DonHang = new
                    {
                        updated.Id,
                        updated.NgayDat,
                        updated.TrangThai,
                        updated.TongTien,
                        TenNguoiDung = updated.NguoiDung?.HoTen,
                        DiaChiGiaoHang = updated.DiaChi?.DiaChiNhan,
                        ChiTiet = updated.DonhangChitiets.Select(ct => new
                        {
                            SanPham = ct.SanPham?.TenSp,
                            KichThuoc = ct.KichThuoc?.SoLieu,
                            ct.SoLuong,
                            ct.Gia
                        })
                    }
                });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật dữ liệu vào database.", error = dbEx.InnerException?.Message ?? dbEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi không xác định khi cập nhật trạng thái.", error = ex.Message });
            }
        }
    }
}
