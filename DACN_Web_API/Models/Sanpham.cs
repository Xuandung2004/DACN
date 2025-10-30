using System;
using System.Collections.Generic;

namespace DACN_Web_API.Models;

public partial class Sanpham
{
    public int Id { get; set; }

    public string? TenSp { get; set; }

    public string? MoTa { get; set; }

    public double? Gia { get; set; }

    public int? DanhMucId { get; set; }

    public string? Slug { get; set; }

    public int? TonKho { get; set; }

    public int? AnhId { get; set; }

    public DateTime? NgayTao { get; set; }

    public DateTime? CapNhat { get; set; }

    public int? KichThuocId { get; set; }

    public virtual Anh? Anh { get; set; }

    public virtual Danhmuc? DanhMuc { get; set; }

    public virtual ICollection<Danhgium> Danhgia { get; set; } = new List<Danhgium>();

    public virtual ICollection<DonhangChitiet> DonhangChitiets { get; set; } = new List<DonhangChitiet>();

    public virtual ICollection<Giohang> Giohangs { get; set; } = new List<Giohang>();

    public virtual Kichthuoc? KichThuoc { get; set; }
}
