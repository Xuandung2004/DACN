using DACN_Web_API.DTO;


namespace DACN_Web_API.Services;

public interface IThongKeService
{
    Task<DoanhThuTongQuanDto> GetDoanhThuTongQuanAsync(DateTime startDate, DateTime endDate);

    // BÁO CÁO DOANH THU THEO THỜI GIAN
    // Thống kê doanh thu chi tiết của một tuần cụ thể
    // Tái sử dụng DoanhThuChiTietThangDto vì cấu trúc tương đương
    Task<DoanhThuChiTietThangDto> GetDoanhThuMotTuanAsync(int week, int year);
    // Thêm vào IThongKeService.cs
    Task<DoanhThuChiTietThangDto> GetDoanhThuMotThangAsync(int month, int year);
    //Task<List<DoanhThuTheoThoiGianDto>> GetDoanhThuTheoNamAsync();

    // BÁO CÁO TỒN KHO VÀ ĐƠN HÀNG
    Task<TrangThaiDonHangDto> GetTrangThaiDonHangAsync();
    Task<List<TonKhoTheoDanhMucDto>> GetTonKhoTheoDanhMucAsync();

    // BÁO CÁO ĐÁNH GIÁ
    Task<ThongKeDanhGiaDto> GetThongKeDanhGiaAsync();


    // Phương thức mới cho biểu đồ 12 tháng
    Task<IEnumerable<DoanhThuMotThangDto>> GetDoanhThuTheoThangTrongNamAsync(int year);
}