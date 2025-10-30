using System;
using System.Collections.Generic;

namespace DACN_Web_API.Models;

public partial class Nguoidung
{
    public int Id { get; set; }

    public string? TenDn { get; set; }

    public string? MatKhau { get; set; }

    public string? Email { get; set; }

    public string? Sdt { get; set; }

    public string? HoTen { get; set; }

    public string? ViTri { get; set; }

    public DateTime? NgayTao { get; set; }

    public string? TrangThai { get; set; }

    public virtual ICollection<Danhgium> Danhgia { get; set; } = new List<Danhgium>();

    public virtual ICollection<Dondattruoc> Dondattruocs { get; set; } = new List<Dondattruoc>();

    public virtual ICollection<Donhang> Donhangs { get; set; } = new List<Donhang>();

    public virtual ICollection<Giohang> Giohangs { get; set; } = new List<Giohang>();

    public virtual ICollection<Ngayhen> Ngayhens { get; set; } = new List<Ngayhen>();

    public virtual ICollection<Thongtinnhan> Thongtinnhans { get; set; } = new List<Thongtinnhan>();
}
