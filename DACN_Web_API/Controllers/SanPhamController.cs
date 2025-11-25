using DACN_Web_API.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DACN_Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SanPhamController : ControllerBase
    {
        private readonly CsdlFinal1Context db = new CsdlFinal1Context();
        private readonly IWebHostEnvironment _env;

        public SanPhamController(IWebHostEnvironment env)
        {
            _env = env;
        }

        // GET: api/SanPham
        // Returns products with related DanhMuc and Anhs projected to friendly names expected by frontend
        [HttpGet]
        public IActionResult GetAllSP()
        {
            var items = db.Sanphams
                .Include(s => s.DanhMuc)
                .Include(s => s.Anhs)
                .Select(s => new
                {
                    id = s.Id,
                    tenSp = s.TenSp,
                    moTa = s.MoTa,
                    gia = s.Gia,
                    tonKho = s.TonKho,
                    slug = s.Slug,
                    ngayTao = s.NgayTao,
                    capNhat = s.CapNhat,
                    danhMuc = s.DanhMuc == null ? null : new { id = s.DanhMuc.Id, tenDanhMuc = s.DanhMuc.TenDm },
                    anhs = s.Anhs.Select(a => new { id = a.Id, duongDan = a.Url })
                })
                .ToList();

            return Ok(items);
        }

        // GET: api/SanPham/5
        [HttpGet("{id}")]
        public IActionResult GetSP(int id)
        {
            var s = db.Sanphams
                .Include(sp => sp.DanhMuc)
                .Include(sp => sp.Anhs)
                .Where(sp => sp.Id == id)
                .Select(sp => new
                {
                    id = sp.Id,
                    tenSp = sp.TenSp,
                    moTa = sp.MoTa,
                    gia = sp.Gia,
                    tonKho = sp.TonKho,
                    slug = sp.Slug,
                    ngayTao = sp.NgayTao,
                    capNhat = sp.CapNhat,
                    danhMuc = sp.DanhMuc == null ? null : new { id = sp.DanhMuc.Id, tenDanhMuc = sp.DanhMuc.TenDm },
                    anhs = sp.Anhs.Select(a => new { id = a.Id, duongDan = a.Url })
                })
                .FirstOrDefault();

            if (s == null) return NotFound();
            return Ok(s);
        }

        // POST: api/SanPham
        [HttpPost]
        public IActionResult CreateSP([FromBody] Sanpham model)
        {
            if (model == null) return BadRequest();

            model.NgayTao = DateTime.Now;
            db.Sanphams.Add(model);
            db.SaveChanges();

            // Return created resource (projected)
            return CreatedAtAction(nameof(GetSP), new { id = model.Id }, new { id = model.Id });
        }

        // PUT: api/SanPham/5
        [HttpPut("{id}")]
        public IActionResult UpdateSP(int id, [FromBody] Sanpham model)
        {
            if (model == null || id != model.Id) return BadRequest();

            var existing = db.Sanphams.Find(id);
            if (existing == null) return NotFound();

            // Update scalar properties only; do not overwrite navigation collections here
            existing.TenSp = model.TenSp;
            existing.MoTa = model.MoTa;
            existing.Gia = model.Gia;
            existing.DanhMucId = model.DanhMucId;
            existing.Slug = model.Slug;
            existing.TonKho = model.TonKho;
            existing.CapNhat = DateTime.Now;

            db.Sanphams.Update(existing);
            db.SaveChanges();

            return NoContent();
        }

        // DELETE: api/SanPham/5
        // Instead of deleting the product, set TonKho = 0 to preserve historical data
        [HttpDelete("{id}")]
        public IActionResult DeleteSP(int id)
        {
            var sp = db.Sanphams.Find(id);

            if (sp == null) return NotFound();

            try
            {
                // Disable FK constraint temporarily, update product, re-enable FK
                db.Database.ExecuteSqlRaw("SET FOREIGN_KEY_CHECKS=0");

                sp.TonKho = 0;
                sp.CapNhat = DateTime.Now;
                db.Sanphams.Update(sp);
                db.SaveChanges();

                db.Database.ExecuteSqlRaw("SET FOREIGN_KEY_CHECKS=1");

                return NoContent();
            }
            catch (Exception ex)
            {
                db.Database.ExecuteSqlRaw("SET FOREIGN_KEY_CHECKS=1");
                return StatusCode(500, new { message = "Lỗi khi cập nhật sản phẩm: " + ex.Message });
            }
        }

        // POST: api/SanPham/upload-image
        // Accepts a single file, saves it under wwwroot/uploads and creates an Anh record
        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            if (image == null || image.Length == 0) return BadRequest("No file provided");

            var uploadsRoot = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
            if (!Directory.Exists(uploadsRoot)) Directory.CreateDirectory(uploadsRoot);

            var fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(image.FileName);
            var filePath = Path.Combine(uploadsRoot, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            var relativeUrl = $"/uploads/{fileName}";
            var anh = new Anh { Url = relativeUrl, SanPhamId = null };
            db.Anhs.Add(anh);
            db.SaveChanges();

            return Ok(new { id = anh.Id, url = anh.Url });
        }
    }
}
