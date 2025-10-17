using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebBanHang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SanPhamController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAll()
        {
            var sanPhams = new[]
            {
                new { MaSP = 1, TenSP = "Áo thun", Gia = 120000 },
                new { MaSP = 2, TenSP = "Quần jean", Gia = 250000 },
                new { MaSP = 3, TenSP = "Giày sneaker", Gia = 450000 }
            };

            return Ok(sanPhams);
        }
    }
}
