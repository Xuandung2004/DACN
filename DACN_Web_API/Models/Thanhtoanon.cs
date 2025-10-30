using System;
using System.Collections.Generic;

namespace DACN_Web_API.Models;

public partial class Thanhtoanon
{
    public int Id { get; set; }

    public int? DonHangId { get; set; }

    public string? MaGiaoDich { get; set; }

    public decimal? SoTien { get; set; }

    public string? PhuongThuc { get; set; }

    public DateTime? ThoiGian { get; set; }

    public string? TrangThai { get; set; }

    public string? NoiDung { get; set; }

    public virtual Donhang? DonHang { get; set; }
}
