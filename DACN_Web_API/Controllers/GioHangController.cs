using DACN_Web_API.DTO;
using DACN_Web_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace DACN_Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GioHangController : ControllerBase
    {
        private CsdlFinal1Context db = new CsdlFinal1Context();
        [HttpGet]
        [Route("ChiTiet/{nguoiDungId}")]
        public IActionResult GetChiTiet(int nguoiDungId)
        {
            var items = (from g in db.Giohangs
                         join s in db.Sanphams on g.SanPhamId equals s.Id
                         join k in db.Kichthuocs on g.KichThuocId equals k.Id
                         join a in db.Anhs on s.Id equals a.SanPhamId into anhGroup
                         from a in anhGroup.DefaultIfEmpty()
                         where g.NguoiDungId == nguoiDungId
                         select new
                         {
                             SanPhamID = s.Id,
                             KichThuocID = k.Id,
                             TenSp = s.TenSp,
                             Gia = s.Gia,
                             SoLuong = g.SoLuong,
                             ThanhTien = g.SoLuong * s.Gia,
                             KichThuoc = k.SoLieu,
                             Anh = a != null ? a.Url : null
                         }).ToList();

            if (!items.Any())
                return Ok(new { message = "Giỏ hàng trống", items = items });

            var tongTien = items.Sum(i => i.ThanhTien);
            return Ok(new
            {
                tongTien,
                soSanPham = items.Count,
                items
            });
        }
        //Cập nhật số lượng sản phẩm trong giỏ
        [HttpPut("update")]
        public IActionResult UpdateCartItem([FromBody] GioHangDTO item)
        {
            var existing = db.Giohangs.FirstOrDefault(g =>
                g.NguoiDungId == item.NguoiDungId &&
                g.SanPhamId == item.SanPhamId &&
                g.KichThuocId == item.KichThuocId);

            if (existing == null)
                return NotFound(new { message = "Không tìm thấy sản phẩm trong giỏ hàng" });

            existing.SoLuong = item.SoLuong;
            db.SaveChanges();

            return Ok(new { message = "Cập nhật giỏ hàng thành công" });
        }
        //Xoá 1 item
        [HttpDelete("delItem/{nguoiDungId}/{sanPhamId}/{kichThuocId}")]
        public IActionResult DelCartItem(int nguoiDungId, int sanPhamId, int kichThuocId)
        {
            var delItem = db.Giohangs.FirstOrDefault(g => g.NguoiDungId == nguoiDungId && g.SanPhamId == sanPhamId && g.KichThuocId == kichThuocId);
            if(delItem == null)
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm!" });
            }
            db.Giohangs.Remove(delItem);
            db.SaveChanges();
            return Ok(new { message = "Xoá sản phẩm thành công!" });
        }
        //Xoá toàn bộ giỏ hàng
        [HttpDelete("DelAllItem/{nguoiDungId}")]
        public IActionResult DelAllItem(int nguoiDungId)
        {
            var list = db.Giohangs.Where(g => g.NguoiDungId == nguoiDungId).ToList();
            db.RemoveRange(list);
            db.SaveChanges();
            return Ok(new { message = "Đã xoá toàn bộ giỏ hàng!" });
        }
        //Thêm sản phẩm vào giỏ hàng
        [HttpPost("AddToCart")]
        public IActionResult AddToCart([FromBody] GioHangDTO item)
        {
            var existing = db.Giohangs.FirstOrDefault(g => g.NguoiDungId == item.NguoiDungId && g.SanPhamId == item.SanPhamId && g.KichThuocId == item.KichThuocId);
            if(existing == null)
            {
                var newItem = new Giohang
                {
                    NguoiDungId = item.NguoiDungId,
                    SanPhamId = item.SanPhamId,
                    KichThuocId = item.KichThuocId,
                    SoLuong = item.SoLuong
                };
                db.Giohangs.Add(newItem);
            }
            else
            {
                existing.SoLuong += item.SoLuong;
            }
            db.SaveChanges();
            return Ok(new { message = "Them san pham thanh cong!" });
        }
    }
}
