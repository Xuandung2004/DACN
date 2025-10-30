using System;
using System.Collections.Generic;

namespace DACN_Web_API.Models;

public partial class Thongtinnhan
{
    public int Id { get; set; }

    public string? TenNguoiNhan { get; set; }

    public string? Sdtnn { get; set; }

    public int? NguoiDungId { get; set; }

    public string? DiaChiNhan { get; set; }

    public virtual ICollection<Donhang> Donhangs { get; set; } = new List<Donhang>();

    public virtual Nguoidung? NguoiDung { get; set; }
}
