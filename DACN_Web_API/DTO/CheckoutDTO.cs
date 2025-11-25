using System;
using System.Collections.Generic;

namespace DACN_Web_API.DTO
{
    public class CheckoutDTO
    {
        public int NguoiDungId { get; set; }
        public string TenNguoiNhan { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public string DiaChiNhan { get; set; }
        public string TinhThanh { get; set; }
        public string GhiChu { get; set; }
        public string PhuongThuc { get; set; }
        public decimal TongTien { get; set; }
        public List<CheckoutItemDTO> DanhSachSanPham { get; set; }
    }

    public class CheckoutItemDTO
    {
        public int SanPhamId { get; set; }
        public int SoLuong { get; set; }
        public decimal Gia { get; set; }
        public int KichThuocId { get; set; }
    }
}
