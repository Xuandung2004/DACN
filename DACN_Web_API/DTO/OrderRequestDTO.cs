namespace DACN_Web_API.DTO
{
    public class OrderRequestDTO
    {
        public int NguoiDungId { get; set; }
        public int DiaChiId { get; set; }

        public string GhiChu { get; set; }
        public string PhuongThucThanhToan { get; set; }
    }
}
