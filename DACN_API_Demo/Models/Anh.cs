using System;
using System.Collections.Generic;

namespace DACN_API_Demo.Models;

public partial class Anh
{
    public int Id { get; set; }

    public string? Url { get; set; }

    public virtual ICollection<Sanpham> Sanphams { get; set; } = new List<Sanpham>();
}
