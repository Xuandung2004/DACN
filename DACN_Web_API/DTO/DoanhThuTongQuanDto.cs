namespace DACN_Web_API.DTO;

public class DoanhThuTongQuanDto
{
    // Tổng doanh thu (tính từ các đơn hàng có trạng thái thành công)
    public decimal TongDoanhThu { get; set; }

    // Số lượng đơn hàng đã hoàn thành/thành công
    public int SoLuongDonHangHoanThanh { get; set; }

    // Số lượng sản phẩm đã bán
    public int TongSoLuongSanPhamBan { get; set; }

    // Giá trị trung bình của mỗi đơn hàng
    public decimal TrungBinhGiaTriDonHang { get; set; }
}