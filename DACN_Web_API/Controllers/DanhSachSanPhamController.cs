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
        private CsdlFinal1Context _context = new CsdlFinal1Context();

        /// <summary>
        /// Lấy chi tiết sản phẩm theo ID, lọc, tìm kiếm, phân trang
        /// </summary>
        /// <param name="id">Id sản phẩm</param>
        /// <returns>Danh sách sản phẩm, stt trang, số sản phẩm 1 trang</returns>
        /// created by: TMHIEU (27/11/2025)
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

        [HttpGet]
        public async Task<IActionResult> GetSanPham(
            int page = 1,
            int pageSize = 8,
            string? search = null,
            double? minPrice = null,
            double? maxPrice = null,
            int? categoryId = null
        )
        {
            var query = _context.Sanphams
                .Include(s => s.DanhMuc)
                .Include(s => s.Kichthuocs)
                .Include(s => s.Anhs)
                .AsQueryable();

            // ---- Lọc theo tên ----
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(sp =>
                    sp.TenSp.Contains(search));
            }

            // ---- Lọc theo giá ----
            if (minPrice.HasValue)
            {
                query = query.Where(sp => sp.Gia >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(sp => sp.Gia <= maxPrice.Value);
            }

            // ---- Lọc theo danh mục ----
            if (categoryId.HasValue)
            {
                query = query.Where(sp => sp.DanhMucId == categoryId.Value);
            }

            // ---- Tổng số sản phẩm sau lọc ----
            var totalItems = await query.CountAsync();

            // ---- Phân trang ----
            var data = await query
                .OrderBy(sp => sp.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // ---- Convert sang format trả về giống GetById ----
            var items = data.Select(sp => new
            {
                sp.Id,
                sp.TenSp,
                sp.Gia,
                sp.MoTa,
                DanhMuc = sp.DanhMuc?.TenDm,
                Anh = sp.Anhs.Select(a => a.Url).ToList(),
                KichThuoc = sp.Kichthuocs.Select(k => k.SoLieu).ToList()
            }).ToList();

            // ---- Trả về dạng chuẩn ----
            return Ok(new
            {
                page,
                pageSize,
                totalItems,
                totalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                items
            });
        }

        [HttpGet("DanhMuc")]
        public async Task<IActionResult> GetSanPham()
        {
            var dsDanhMuc = await _context.Danhmucs
                .Select(sp => new
                {
                    sp.Id,
                    sp.TenDm
                })
                .ToListAsync();

            if (dsDanhMuc == null)
                return NotFound();

            return Ok(dsDanhMuc);
        }
    }
}