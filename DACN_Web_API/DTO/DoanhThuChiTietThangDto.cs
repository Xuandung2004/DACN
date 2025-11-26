namespace DACN_Web_API.DTO
{
    public class DoanhThuChiTietThangDto
    {

        public string ThoiGian { get; set; } = string.Empty; // Ví dụ: "Tháng 9/2025"
        public decimal TongDoanhThu { get; set; }
        public int SoDonHang { get; set; }
    }
}
