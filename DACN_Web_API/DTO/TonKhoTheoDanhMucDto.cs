namespace DACN_Web_API.DTO;

public class TonKhoTheoDanhMucDto
{
    public int DanhMucId { get; set; }
    public string? TenDanhMuc { get; set; }
    public int TongSoLuongTonKho { get; set; }
    public int TongSoLuongSanPham { get; set; } // Tổng số sản phẩm trong danh mục
}