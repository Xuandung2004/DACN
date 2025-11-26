namespace DACN_Web_API.DTO
{
    public class DoanhThuTheoThoiGianDto
    {
        public string ThoiGian { get; set; } = string.Empty; // Ví dụ: "Tuần 45/2025" hoặc "Tháng 11/2025"
        public decimal DoanhThu { get; set; }
        public int SoDonHang { get; set; }
    }
}
