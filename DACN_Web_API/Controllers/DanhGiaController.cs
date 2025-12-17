using DACN_Web_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DACN_Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DanhGiaController : ControllerBase
    {

        private CsdlFinal1Context _context = new CsdlFinal1Context();

        //thêm đánh giá
        [HttpPost("AddDanhGia")]
        public async Task<IActionResult> AddDanhGia(
            int nguoiDungId,
            int sanPhamId,
            int rate,
            string noiDung)
        {
            var dg = new Danhgium
            {
                NguoiDungId = nguoiDungId,
                SanPhamId = sanPhamId,
                Rate = rate,
                NoiDung = noiDung,
                NgayDanhGia = DateTime.Now
            };

            _context.Danhgia.Add(dg);
            await _context.SaveChangesAsync();

            return Ok(dg);
        }



        //load danh sách đánh giá theo id sản phẩm
        [HttpGet("reviews/{productId}")]
        public async Task<IActionResult> GetReviewsByProductId(int productId)
        {
            var reviews = await _context.Danhgia
        .Where(x => x.SanPhamId == productId)
        .Include(x => x.NguoiDung)
        .OrderByDescending(x => x.NgayDanhGia)
        .Select(x => new {
            x.Rate,
            x.NoiDung,
            x.NgayDanhGia,
            HoTen = x.NguoiDung.HoTen
        })
        .ToListAsync();

            return Ok(reviews);
        }

    }
}
