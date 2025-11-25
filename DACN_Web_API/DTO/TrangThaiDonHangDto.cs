namespace DACN_Web_API.DTO
{
    public class TrangThaiDonHangDto
    {

        public int TongDonHang { get; set; }
        public int DangXuLy { get; set; }
        public int DaGiao { get; set; }
        public int DaVanChuyen { get; set; } // Trạng thái 'đã vận chuyển' có trong Donhang.cs
        public int DaHuy { get; set; }
    }
}
