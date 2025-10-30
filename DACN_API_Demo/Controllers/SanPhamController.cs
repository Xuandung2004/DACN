using DACN_API_Demo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DACN_API_Demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SanPhamController : ControllerBase
    {
        private DacnDemoContext db = new DacnDemoContext();
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(db.Sanphams.ToList());
        }
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var sp = db.Sanphams.FirstOrDefault(s => s.Id == id);
            if(sp == null)
            {
                return NotFound();
            }
            return Ok(sp);
        }
    }
}
