using System;
using System.Collections.Generic;

namespace DACN_Web_API.Models;

public partial class Dondattruoc
{
    public int Id { get; set; }

    public int? NguoiDungId { get; set; }

    public DateOnly? NgayDat { get; set; }

    public string? MoTa { get; set; }

    public decimal? GiaDuKien { get; set; }

    public DateTime? NgayHt { get; set; }

    public string? TrangThai { get; set; }

    public virtual Nguoidung? NguoiDung { get; set; }
}
