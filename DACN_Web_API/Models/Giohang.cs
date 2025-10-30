using System;
using System.Collections.Generic;

namespace DACN_Web_API.Models;

public partial class Giohang
{
    public int SanPhamId { get; set; }

    public int? SoLuong { get; set; }

    public int NguoiDungId { get; set; }

    public int KichThuocId { get; set; }

    public virtual Kichthuoc KichThuoc { get; set; } = null!;

    public virtual Nguoidung NguoiDung { get; set; } = null!;

    public virtual Sanpham SanPham { get; set; } = null!;
}
