using System;
using System.Collections.Generic;

namespace DACN_Web_API.Models;

public partial class Anh
{
    public int Id { get; set; }

    public string? Url { get; set; }

    public int? SanPhamId { get; set; }

    public virtual Sanpham? SanPham { get; set; }
}
