using DACN_Web_API.DTO;

using DACN_Web_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DACN_Web_API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ThongKeController : ControllerBase
{
    private readonly IThongKeService _thongKeService;

    public ThongKeController(IThongKeService thongKeService)
    {
        _thongKeService = thongKeService;
    }

    // GET: api/ThongKe/TongQuan?startDate=2025-01-01&endDate=2025-12-31
    /// <summary>
    /// Thống kê tổng quan doanh thu trong một khoảng thời gian.
    /// </summary>
    [HttpGet("TongQuan")]
    public async Task<ActionResult<DoanhThuTongQuanDto>> GetTongQuan([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
        {
            return BadRequest("Ngày bắt đầu không được lớn hơn ngày kết thúc.");
        }

        // Đảm bảo endDate bao gồm cả ngày cuối cùng (cuối ngày)
        var adjustedEndDate = endDate.Date.AddDays(1).AddSeconds(-1);

        var result = await _thongKeService.GetDoanhThuTongQuanAsync(startDate, adjustedEndDate);
        return Ok(result);
    }

    // Thêm vào ThongKeController.cs

    // GET: api/ThongKe/ChiTietThang?thangNam=09/2025
    /// <summary>
    /// Thống kê doanh thu chi tiết của một tháng cụ thể (ví dụ: 09/2025).
    /// </summary>
    [HttpGet("ChiTietThang")]
    public async Task<ActionResult<DoanhThuChiTietThangDto>> GetDoanhThuChiTietThang([FromQuery] string thangNam)
    {
        // Kiểm tra định dạng đầu vào (ví dụ: MM/YYYY)
        if (string.IsNullOrEmpty(thangNam) || !thangNam.Contains('/'))
        {
            return BadRequest("Định dạng phải là MM/YYYY (ví dụ: 09/2025).");
        }

        // Tách chuỗi
        string[] parts = thangNam.Split('/');
        if (parts.Length != 2 || !int.TryParse(parts[0], out int month) || !int.TryParse(parts[1], out int year))
        {
            return BadRequest("Định dạng tháng hoặc năm không hợp lệ.");
        }

        if (month < 1 || month > 12 || year < 2000 || year > DateTime.Now.Year + 1)
        {
            return BadRequest("Tháng hoặc Năm nằm ngoài phạm vi cho phép.");
        }

        // Gọi Service
        var result = await _thongKeService.GetDoanhThuMotThangAsync(month, year);
        return Ok(result);
    }






    // MỚI: 1. Doanh thu theo Tuần
    // GET: api/ThongKe/ChiTietTuan?tuanNam=45/2025
    /// <summary>
    /// Thống kê doanh thu chi tiết của một tuần cụ thể (ví dụ: 45/2025).
    /// </summary>
    [HttpGet("ChiTietTuan")]
    public async Task<ActionResult<DoanhThuChiTietThangDto>> GetDoanhThuChiTietTuan([FromQuery] string tuanNam)
    {
        // Kiểm tra định dạng đầu vào (ví dụ: WW/YYYY)
        if (string.IsNullOrEmpty(tuanNam) || !tuanNam.Contains('/'))
        {
            return BadRequest("Định dạng phải là WW/YYYY (ví dụ: 45/2025).");
        }

        // Tách chuỗi và Parse giá trị
        string[] parts = tuanNam.Split('/');
        if (parts.Length != 2 || !int.TryParse(parts[0], out int week) || !int.TryParse(parts[1], out int year))
        {
            return BadRequest("Định dạng tuần hoặc năm không hợp lệ.");
        }

        // Kiểm tra tính hợp lệ của Tuần và Năm
        if (week < 1 || week > 53 || year < 2000 || year > DateTime.Now.Year + 1)
        {
            return BadRequest("Tuần hoặc Năm nằm ngoài phạm vi cho phép.");
        }

        // Gọi Service
        var result = await _thongKeService.GetDoanhThuMotTuanAsync(week, year);
        return Ok(result);
    }

    // MỚI: 2. Doanh thu theo Năm
    /// <summary>
    /// Thống kê doanh thu chi tiết cả năm của một năm cụ thể.
    /// Ví dụ: /api/ThongKe/TheoNamChiTiet?year=2025
    /// </summary>
    [HttpGet("TheoNamChiTiet")]
    public async Task<ActionResult<DoanhThuTongQuanDto>> GetDoanhThuTheoNamChiTiet([FromQuery] int year)
    {
        if (year < 2000 || year > DateTime.Now.Year + 1)
        {
            return BadRequest("Năm không hợp lệ.");
        }

        try
        {
            // 1. Tính toán khoảng ngày cho năm đó
            DateTime startDate = new DateTime(year, 1, 1);
            // Ngày cuối cùng của năm, bao gồm cả 23:59:59
            DateTime endDate = new DateTime(year, 12, 31, 23, 59, 59);

            // 2. Sử dụng lại Service thống kê tổng quan
            var result = await _thongKeService.GetDoanhThuTongQuanAsync(startDate, endDate);

            // 3. (Tuỳ chọn): Điều chỉnh DTO để hiển thị rõ là theo năm nào
            // Nếu DoanhThuTongQuanDto không có trường thời gian, bạn có thể tạo một DTO mới 
            // hoặc ghi đè thông tin. Ở đây, ta giữ nguyên DTO và chỉ trả về kết quả.

            return Ok(result);
        }
        catch (Exception ex)
        {
            // Ghi log lỗi nếu cần
            return StatusCode(500, $"Lỗi xảy ra khi thống kê doanh thu theo năm: {ex.Message}");
        }
    }

    // MỚI: 3. Trạng thái đơn hàng
    [HttpGet("TrangThaiDonHang")]
    public async Task<ActionResult<TrangThaiDonHangDto>> GetTrangThaiDonHang()
    {
        var result = await _thongKeService.GetTrangThaiDonHangAsync();
        return Ok(result);
    }

    // MỚI: 4. Tồn kho theo danh mục
    [HttpGet("TonKhoTheoDanhMuc")]
    public async Task<ActionResult<IEnumerable<TonKhoTheoDanhMucDto>>> GetTonKhoTheoDanhMuc()
    {
        var result = await _thongKeService.GetTonKhoTheoDanhMucAsync();
        return Ok(result);
    }

    // MỚI: 5. Thống kê Đánh giá
    [HttpGet("ThongKeDanhGia")]
    public async Task<ActionResult<ThongKeDanhGiaDto>> GetThongKeDanhGia()
    {
        var result = await _thongKeService.GetThongKeDanhGiaAsync();
        return Ok(result);
    }



    // MỚI: 6. Doanh thu theo tháng trong một năm (Cho biểu đồ dashboard)
    /// <summary>
    /// Thống kê doanh thu theo từng tháng trong một năm cụ thể.
    /// Ví dụ: /api/ThongKe/TheoThang?year=2025
    /// </summary>
    [HttpGet("TheoThang")]
    public async Task<ActionResult<IEnumerable<DoanhThuMotThangDto>>> GetDoanhThuTheoThangTrongNam([FromQuery] int year)
    {
        if (year < 2000 || year > DateTime.Now.Year + 1)
        {
            return BadRequest("Năm không hợp lệ.");
        }

        try
        {
            // Yêu cầu Service của bạn phải có hàm GetDoanhThuTheoThangTrongNamAsync()
            // Hàm này trả về List<{ ThoiGian: string, DoanhThu: number }> cho 12 tháng.
            var result = await _thongKeService.GetDoanhThuTheoThangTrongNamAsync(year);
            return Ok(result);
        }
        catch (Exception ex)
        {
            // Ghi log lỗi nếu cần
            return StatusCode(500, $"Lỗi xảy ra khi thống kê doanh thu theo tháng: {ex.Message}");
        }
    }
}