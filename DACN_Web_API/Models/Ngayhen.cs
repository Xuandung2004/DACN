using System;
using System.Collections.Generic;

namespace DACN_Web_API.Models;

public partial class Ngayhen
{
    public int Id { get; set; }

    public DateTime? Ngay { get; set; }

    public int? NguoiDungId { get; set; }

    public virtual Nguoidung? NguoiDung { get; set; }
}
