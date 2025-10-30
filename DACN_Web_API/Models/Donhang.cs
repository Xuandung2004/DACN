using System;
using System.Collections.Generic;

namespace DACN_Web_API.Models;

public partial class Donhang
{
    public int Id { get; set; }

    public DateTime? NgayDat { get; set; }

    public int? NguoiDungId { get; set; }

    public string? TrangThai { get; set; }

    public string? GhiChu { get; set; }

    public int? DiaChiId { get; set; }

    public int? TongTien { get; set; }

    public virtual Thongtinnhan? DiaChi { get; set; }

    public virtual ICollection<DonhangChitiet> DonhangChitiets { get; set; } = new List<DonhangChitiet>();

    public virtual Nguoidung? NguoiDung { get; set; }

    public virtual ICollection<Thanhtoanon> Thanhtoanons { get; set; } = new List<Thanhtoanon>();
}
