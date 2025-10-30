using System;
using System.Collections.Generic;

namespace DACN_Web_API.Models;

public partial class DonhangChitiet
{
    public int DonHangId { get; set; }

    public int SanPhamId { get; set; }

    public int? SoLuong { get; set; }

    public int KichThuocId { get; set; }

    public decimal? Gia { get; set; }

    public virtual Donhang DonHang { get; set; } = null!;

    public virtual Kichthuoc KichThuoc { get; set; } = null!;

    public virtual Sanpham SanPham { get; set; } = null!;
}
