using DACN_Web_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DACN_Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DanhSachSanPhamController : ControllerBase
    {
        private readonly CsdlFinal1Context _context;

        public DanhSachSanPhamController(CsdlFinal1Context context)
        {
            _context = context;
        }

        //  GET: api/DanhSachSanPham
        [HttpGet]
        public async Task<IActionResult> GetSanPhams()
        {
            var sanPhams = await _context.Sanphams
                .Include(sp => sp.Anhs)        
                .Select(sp => new
                {
                    sp.Id,
                    sp.TenSp,
                    sp.Gia,
                    Anh = sp.Anhs.Select(a => a.Url).ToList()
                    //KichThuoc = sp.Kichthuocs.Select(k => k.SoLieu).ToList()
                })
                .ToListAsync();

            return Ok(sanPhams);
        }

        //  GET: api/DanhSachSanPham/5 (chi tiết sản phẩm)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSanPham(int id)
        {
            var sp = await _context.Sanphams
                .Include(s => s.DanhMuc)
                .Include(s => s.Kichthuocs)
                .Include(s => s.Anhs)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sp == null)
                return NotFound();

            var sanPhamChiTiet = new
            {
                sp.Id,
                sp.TenSp,
                sp.Gia,
                sp.MoTa,
                DanhMuc = sp.DanhMuc?.TenDm,
                Anh = sp.Anhs.Select(a => a.Url).ToList(),
                KichThuoc = sp.Kichthuocs.Select(a => a.SoLieu).ToList()
            };

            return Ok(sanPhamChiTiet);
        }
    }
}
