using System;
using System.Collections.Generic;

namespace DACN_Web_API.Models;

public partial class Kichthuoc
{
    public int Id { get; set; }

    public int? SoLieu { get; set; }

    public int? SanPhamId { get; set; }

    public virtual ICollection<DonhangChitiet> DonhangChitiets { get; set; } = new List<DonhangChitiet>();

    public virtual ICollection<Giohang> Giohangs { get; set; } = new List<Giohang>();

    public virtual Sanpham? SanPham { get; set; }
}
