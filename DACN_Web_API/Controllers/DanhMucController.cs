using DACN_Web_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace DACN_Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DanhMucController : ControllerBase
    {
        private readonly CsdlFinal1Context db = new CsdlFinal1Context();

        // GET: api/DanhMuc
        [HttpGet]
        public IActionResult GetAll()
        {
            var danhmucs = db.Danhmucs
                .Select(d => new
                {
                    id = d.Id,
                    tenDanhMuc = d.TenDm,
                    moTa = d.MoTa,
                    slug = d.Slug,
                    soLuongSanPham = d.Sanphams.Count
                })
                .ToList();

            return Ok(danhmucs);
        }

        // GET: api/DanhMuc/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var danhmuc = db.Danhmucs
                .Select(d => new
                {
                    id = d.Id,
                    tenDanhMuc = d.TenDm,
                    moTa = d.MoTa,
                    slug = d.Slug,
                    soLuongSanPham = d.Sanphams.Count
                })
                .FirstOrDefault(d => d.id == id);

            if (danhmuc == null)
                return NotFound();

            return Ok(danhmuc);
        }

        // POST: api/DanhMuc
        [HttpPost]
        public IActionResult Create([FromBody] Danhmuc model)
        {
            if (model == null)
                return BadRequest();

            // Kiểm tra tên danh mục
            if (string.IsNullOrEmpty(model.TenDm))
                return BadRequest("Tên danh mục không được để trống");

            // Kiểm tra trùng tên
            if (db.Danhmucs.Any(d => d.TenDm == model.TenDm))
                return BadRequest("Tên danh mục đã tồn tại");

            // Tự động tạo slug nếu chưa có
            if (string.IsNullOrEmpty(model.Slug))
            {
                model.Slug = CreateSlug(model.TenDm);
            }

            db.Danhmucs.Add(model);
            db.SaveChanges();

            return CreatedAtAction(nameof(Get), new { id = model.Id }, new
            {
                id = model.Id,
                tenDanhMuc = model.TenDm,
                moTa = model.MoTa,
                slug = model.Slug
            });
        }

        // PUT: api/DanhMuc/5
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Danhmuc model)
        {
            if (model == null || id != model.Id)
                return BadRequest();

            var existing = db.Danhmucs.Find(id);
            if (existing == null)
                return NotFound();

            // Kiểm tra tên danh mục
            if (string.IsNullOrEmpty(model.TenDm))
                return BadRequest("Tên danh mục không được để trống");

            // Kiểm tra trùng tên (trừ chính nó)
            if (db.Danhmucs.Any(d => d.TenDm == model.TenDm && d.Id != id))
                return BadRequest("Tên danh mục đã tồn tại");

            // Cập nhật thông tin
            existing.TenDm = model.TenDm;
            existing.MoTa = model.MoTa;
            existing.Slug = string.IsNullOrEmpty(model.Slug) ? CreateSlug(model.TenDm) : model.Slug;

            db.Danhmucs.Update(existing);
            db.SaveChanges();

            return NoContent();
        }

        // DELETE: api/DanhMuc/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var danhmuc = db.Danhmucs
                .Include(d => d.Sanphams)
                .FirstOrDefault(d => d.Id == id);

            if (danhmuc == null)
                return NotFound();

            // Kiểm tra xem danh mục có sản phẩm không
            if (danhmuc.Sanphams.Any())
                return BadRequest("Không thể xóa danh mục đang có sản phẩm");

            db.Danhmucs.Remove(danhmuc);
            db.SaveChanges();

            return NoContent();
        }

        // Helper method to create URL-friendly slug
        private string CreateSlug(string name)
        {
            if (string.IsNullOrEmpty(name)) return "";

            // Remove diacritics (accents)
            string slug = RemoveDiacritics(name.ToLower());

            // Replace spaces with hyphens and remove invalid characters
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
            slug = slug.Trim('-');

            return slug;
        }

        // Helper method to remove Vietnamese diacritics
        private string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
            var stringBuilder = new System.Text.StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC);
        }
    }
}