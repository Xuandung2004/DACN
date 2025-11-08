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
