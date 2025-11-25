using DACN_Web_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DACN_Web_API.Controllers
{
    // DTO cho POST
    public class DiaChiCreateModel
    {
        public required int IdNguoiDung { get; set; } 
        public required string TenNguoiNhan { get; set; }
        public required string Sdt { get; set; }
        public required string DiaChiCuThe { get; set; }
    }
    
    public class DiaChiResponseModel
    {
        public int Id { get; set; }
        public string? TenNguoiNhan { get; set; }
        public string? Sdt { get; set; }
        public string? DiaChiCuThe { get; set; }
        public int? NguoiDungId { get; set; } 
    }

    [Route("api/[controller]")]
    [ApiController]
    public class DiaChiController : ControllerBase
    {
        private readonly CsdlFinal1Context db = new CsdlFinal1Context(); 
        
        // --- Hàm kiểm tra và trả về lỗi chung ---
        private IActionResult? ValidateDiaChiModel(DiaChiCreateModel model)
        {
            if (model.IdNguoiDung <= 0)
            {
                return BadRequest(new { message = "ID người dùng không hợp lệ." });
            }
            if (string.IsNullOrWhiteSpace(model.TenNguoiNhan) || 
                string.IsNullOrWhiteSpace(model.Sdt) ||
                string.IsNullOrWhiteSpace(model.DiaChiCuThe))
            {
                return BadRequest(new { message = "Vui lòng điền đầy đủ Tên người nhận, SĐT và Địa chỉ cụ thể." });
            }
            return null;
        }

        // --------------------------------------------------------
        // --- 1. Thêm địa chỉ nhận hàng mới (POST) ---
        // --------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> ThemDiaChi([FromBody] DiaChiCreateModel model)
        {
            var validationError = ValidateDiaChiModel(model);
            if (validationError != null) return validationError;

            try
            {
                if (!await db.Nguoidungs.AnyAsync(nd => nd.Id == model.IdNguoiDung && nd.ViTri == "khachhang"))
                {
                    return NotFound(new { message = $"Không tìm thấy tài khoản khách hàng với ID: {model.IdNguoiDung}." });
                }
                
                var newDiaChi = new Thongtinnhan
                {
                    TenNguoiNhan = model.TenNguoiNhan,
                    Sdtnn = model.Sdt,
                    DiaChiNhan = model.DiaChiCuThe,
                    NguoiDungId = model.IdNguoiDung,
                };

                db.Thongtinnhans.Add(newDiaChi);
                await db.SaveChangesAsync();

                // Tối ưu hóa trả về 
                return CreatedAtAction(nameof(GetDiaChiById), new { id = newDiaChi.Id }, new 
                { 
                    message = "Thêm địa chỉ giao hàng thành công.", 
                    DiaChi = new 
                    {
                        Id = newDiaChi.Id,
                        TenNguoiNhan = newDiaChi.TenNguoiNhan,
                        Sdt = newDiaChi.Sdtnn,
                        DiaChiCuThe = newDiaChi.DiaChiNhan,
                        NguoiDungId = newDiaChi.NguoiDungId,
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống khi thêm địa chỉ.", error = ex.Message });
            }
        }
        
        // --------------------------------------------------------
        // --- 2. Lấy danh sách địa chỉ theo ID người dùng (GET) ---
        // --------------------------------------------------------
        [HttpGet("NguoiDung/{nguoiDungId}")]
        public async Task<ActionResult<IEnumerable<DiaChiResponseModel>>> GetDiaChiByNguoiDung(int nguoiDungId)
        {
            if (nguoiDungId <= 0)
            {
                return BadRequest(new { message = "ID người dùng không hợp lệ." });
            }

            try
            {
                if (!await db.Nguoidungs.AnyAsync(nd => nd.Id == nguoiDungId && nd.ViTri == "khachhang"))
                {
                    return NotFound(new { message = $"Không tìm thấy tài khoản khách hàng với ID: {nguoiDungId}." });
                }

                var danhSachDiaChi = await db.Thongtinnhans
                    .Where(t => t.NguoiDungId == nguoiDungId)
                    .Select(t => new DiaChiResponseModel 
                    {
                        Id = t.Id,
                        TenNguoiNhan = t.TenNguoiNhan,
                        Sdt = t.Sdtnn,
                        DiaChiCuThe = t.DiaChiNhan,
                        NguoiDungId = t.NguoiDungId,
                    })
                    .OrderByDescending(t => t.Id) // Chỉ sắp xếp theo ID (mới nhất lên đầu)
                    .ToListAsync();

                if (!danhSachDiaChi.Any())
                {
                    return NotFound(new { message = "Người dùng này chưa có địa chỉ nào." });
                }

                return Ok(danhSachDiaChi);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tải danh sách địa chỉ.", error = ex.Message });
            }
        }
        
        // --------------------------------------------------------
        // --- 3. Hàm Hỗ trợ cho CreatedAtAction (GET 1 bản ghi) ---
        // --------------------------------------------------------
        [HttpGet("{id}", Name = "GetDiaChiById")]
        public async Task<IActionResult> GetDiaChiById(int id)
        {
            var diaChi = await db.Thongtinnhans
                .Where(t => t.Id == id)
                .Select(t => new 
                {
                    t.Id,
                    t.TenNguoiNhan,
                    Sdt = t.Sdtnn,
                    DiaChiCuThe = t.DiaChiNhan,
                    t.NguoiDungId,
                })
                .FirstOrDefaultAsync();

            if (diaChi == null)
            {
                return NotFound(new { message = "Không tìm thấy địa chỉ." });
            }

            return Ok(diaChi);
        }
    }
}