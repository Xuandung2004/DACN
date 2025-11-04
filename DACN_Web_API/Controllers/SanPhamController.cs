using DACN_Web_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace DACN_Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SanPhamController : ControllerBase
    {
        private CsdlFinal1Context db = new CsdlFinal1Context();

        // GET: api/SanPham
        [HttpGet]
        public IActionResult GetAllSP()
        {
            return Ok(db.Sanphams.ToList());
        }

        // GET: api/SanPham/5
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var sp = db.Sanphams.Find(id);
            if (sp == null) return NotFound();
            return Ok(sp);
        }

        // POST: api/SanPham
        [HttpPost]
        public IActionResult Create([FromBody] Sanpham sp)
        {
            try
            {
                sp.NgayTao = DateTime.Now;
                db.Sanphams.Add(sp);
                db.SaveChanges();
                return Ok("Thêm sản phẩm thành công");
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi: {ex.Message}");
            }
        }

        // PUT: api/SanPham/5
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Sanpham input)
        {
            if (input == null) return BadRequest("Invalid product payload.");
            if (id != input.Id) return BadRequest("Id in URL and payload do not match.");

            var sp = db.Sanphams.Find(id);
            if (sp == null) return NotFound();

            // update allowed fields
            sp.TenSp = input.TenSp;
            sp.MoTa = input.MoTa;
            sp.Gia = input.Gia;
            sp.DanhMucId = input.DanhMucId;
            sp.Slug = input.Slug;
            sp.TonKho = input.TonKho;
            sp.AnhId = input.AnhId;
            sp.KichThuocId = input.KichThuocId;
            sp.CapNhat = DateTime.Now;

            db.SaveChanges();

            // return 204 No Content for a successful update
            return NoContent();
        }

        // DELETE: api/SanPham/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var sp = db.Sanphams.Find(id);
            if (sp == null) return NotFound();

            db.Sanphams.Remove(sp);
            db.SaveChanges();

            return NoContent();
        }

        // NOTE: For admin-only access, apply [Authorize(Roles = "Admin")] to this controller or methods        

    }
}
