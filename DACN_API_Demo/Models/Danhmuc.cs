using System;
using System.Collections.Generic;

namespace DACN_API_Demo.Models;

public partial class Danhmuc
{
    public int Id { get; set; }

    public string? TenDm { get; set; }

    public string? MoTa { get; set; }

    public string? Slug { get; set; }

    public virtual ICollection<Sanpham> Sanphams { get; set; } = new List<Sanpham>();
}
