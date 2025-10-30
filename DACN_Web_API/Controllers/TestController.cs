using DACN_Web_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DACN_Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private CsdlFinal1Context db = new CsdlFinal1Context();
        [HttpGet]
        public IActionResult GetSP()
        {
            return Ok(db.Sanphams.ToList());
        }
    }
}
