using DACN_Web_API.DTO;
using DACN_Web_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DACN_Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatHangController : ControllerBase
    {
        private CsdlFinal1Context db = new CsdlFinal1Context();
        [HttpPost("DatHang")]
        public IActionResult DatHang([FromBody] OrderRequestDTO req)
        {
            try
            {
                // 1. Kiểm tra thông tin trống
                if (req == null || req.NguoiDungId <= 0 || req.DiaChiId <= 0)
                    return BadRequest(new { message = "Thông tin đơn hàng không hợp lệ!" });

                // 2. Lấy giỏ hàng của người dùng
                var gioHang = db.Giohangs
                                .Where(g => g.NguoiDungId == req.NguoiDungId)
                                .ToList();

                if (!gioHang.Any())
                    return BadRequest(new { message = "Giỏ hàng hiện đang trống!" });

                // 3. Tính tổng tiền
                var tongTien = (from g in gioHang
                                join s in db.Sanphams on g.SanPhamId equals s.Id
                                select g.SoLuong * s.Gia).Sum();

                // 4. Tạo đơn hàng
                var donHang = new Donhang
                {
                    NguoiDungId = req.NguoiDungId,
                    DiaChiId = req.DiaChiId,
                    NgayDat = DateTime.Now,
                    TrangThai = "đang xử lý",
                    GhiChu = req.GhiChu,
                    TongTien = (int)tongTien
                };

                db.Donhangs.Add(donHang);
                db.SaveChanges(); // để lấy ID đơn hàng

                // 5. Thêm chi tiết đơn hàng
                foreach (var item in gioHang)
                {
                    var sp = db.Sanphams.Find(item.SanPhamId);

                    var chiTiet = new DonhangChitiet
                    {
                        DonHangId = donHang.Id,
                        SanPhamId = item.SanPhamId,
                        KichThuocId = item.KichThuocId,
                        SoLuong = item.SoLuong,
                        Gia = (decimal)sp.Gia
                    };

                    db.DonhangChitiets.Add(chiTiet);
                }

                db.SaveChanges();

                // 6. Xóa giỏ hàng sau khi đặt
                db.Giohangs.RemoveRange(gioHang);
                db.SaveChanges();

                // 7. Trả về phản hồi thành công
                return Ok(new
                {
                    message = "Đặt hàng thành công!",
                    donHangId = donHang.Id,
                    tongTien = tongTien
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống!", error = ex.Message });
            }
        }
        [HttpGet("LichSu/{nguoiDungId}")]
        public IActionResult LichSuDonHang(int nguoiDungId)
        {
            try
            {
                var donHangs = db.Donhangs
                    .Where(d => d.NguoiDungId == nguoiDungId)
                    .OrderByDescending(d => d.NgayDat)
                    .Select(d => new
                    {
                        DonHangID = d.Id,
                        NgayDat = d.NgayDat,
                        TrangThai = d.TrangThai,
                        TongTien = d.TongTien,

                        SanPhams = d.DonhangChitiets.Select(ct => new
                        {
                            SanPhamID = ct.SanPhamId,
                            TenSp = ct.SanPham.TenSp,
                            SoLuong = ct.SoLuong,
                            Gia = ct.Gia,
                            ThanhTien = ct.SoLuong * ct.Gia,
                            KichThuoc = ct.KichThuoc.SoLieu,

                            Anh = db.Anhs
                                .Where(a => a.SanPhamId == ct.SanPhamId)
                                .OrderBy(a => a.Id)
                                .Select(a => a.Url)
                                .FirstOrDefault()
                        }).ToList()
                    })
                    .ToList();

                return Ok(donHangs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }
    }
}
