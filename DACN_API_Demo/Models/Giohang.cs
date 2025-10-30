using System;
using System.Collections.Generic;

namespace DACN_API_Demo.Models;

public partial class Giohang
{
    public int SanPhamId { get; set; }

    public int? SoLuong { get; set; }

    public int NguoiDungId { get; set; }

    public int KichThuocId { get; set; }

    public virtual Sanpham SanPham { get; set; } = null!;
}
