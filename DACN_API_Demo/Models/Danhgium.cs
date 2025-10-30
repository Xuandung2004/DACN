using System;
using System.Collections.Generic;

namespace DACN_API_Demo.Models;

public partial class Danhgium
{
    public int Id { get; set; }

    public int? NguoiDungId { get; set; }

    public int? SanPhamId { get; set; }

    public int? Rate { get; set; }

    public string? NoiDung { get; set; }

    public DateTime? NgayDanhGia { get; set; }

    public virtual Nguoidung? NguoiDung { get; set; }

    public virtual Sanpham? SanPham { get; set; }
}
