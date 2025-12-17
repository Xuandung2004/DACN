using DACN_Web_API.DTO;
using DACN_Web_API.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace DACN_Web_API.Services;

// Đảm bảo bạn đã có IThongKeService.cs như hướng dẫn trước
public class ThongKeService : IThongKeService
{
    //// THAY THẾ DACN_Web_APIContext bằng CsdlFinal1Context
    //private readonly CsdlFinal1Context _context;

    //public ThongKeService(CsdlFinal1Context context)
    //{
    //    _context = context;
    //}
    private readonly CsdlFinal1Context _context = new CsdlFinal1Context();

    // --- Thống kê Tổng quan theo khoảng thời gian ---
    public async Task<DoanhThuTongQuanDto> GetDoanhThuTongQuanAsync(DateTime startDate, DateTime endDate)
    {
        // Giả định: Đơn hàng thành công có TrangThai = "Hoàn thành"
        var donHangsThanhCong = await _context.Donhangs
            // Cần chuyển đổi DateOnly? sang DateTime? để so sánh với DateTime
            .Where(dh => dh.NgayDat >= startDate && dh.NgayDat <= endDate && dh.TrangThai == "đã giao")
            .ToListAsync();

        if (!donHangsThanhCong.Any())
        {
            return new DoanhThuTongQuanDto();
        }

        decimal tongDoanhThu = donHangsThanhCong.Sum(dh => (decimal?)dh.TongTien) ?? 0;
        var donHangIds = donHangsThanhCong.Select(dh => dh.Id).ToList();

        // Lấy tổng số lượng sản phẩm từ DonhangChitiet
        var totalQuantity = await _context.DonhangChitiets
            .Where(ct => donHangIds.Contains(ct.DonHangId))
            .SumAsync(ct => ct.SoLuong);

        decimal trungBinhDonHang = tongDoanhThu > 0 ? tongDoanhThu / donHangsThanhCong.Count : 0;

        return new DoanhThuTongQuanDto
        {
            TongDoanhThu = tongDoanhThu,
            SoLuongDonHangHoanThanh = donHangsThanhCong.Count,
            TongSoLuongSanPhamBan = totalQuantity ?? 0,
            TrungBinhGiaTriDonHang = Math.Round(trungBinhDonHang, 2)
        };
    }

    // SỬA LỖI TRÍCH XUẤT THÁNG CHO MYSQL
    public async Task<DoanhThuChiTietThangDto> GetDoanhThuMotThangAsync(int month, int year)
    {
        // Lọc theo năm, tháng và trạng thái "đã giao"
        var monthYearData = await _context.Donhangs
            .Where(dh => dh.NgayDat.HasValue &&
                         dh.TrangThai == "đã giao" &&
                         dh.NgayDat.Value.Year == year &&
                         dh.NgayDat.Value.Month == month)
            .ToListAsync(); // Lấy tất cả đơn hàng trong tháng/năm đó

        if (!monthYearData.Any())
        {
            return new DoanhThuChiTietThangDto
            {
                ThoiGian = $"Tháng {month}/{year}",
                TongDoanhThu = 0,
                SoDonHang = 0
            };
        }

        // Tính tổng doanh thu và số đơn hàng
        decimal tongDoanhThu = monthYearData.Sum(dh => (decimal?)dh.TongTien) ?? 0;
        int soDonHang = monthYearData.Count;

        return new DoanhThuChiTietThangDto
        {
            ThoiGian = $"Tháng {month}/{year}",
            TongDoanhThu = tongDoanhThu,
            SoDonHang = soDonHang
        };
    }






    // =====================================================================
    //                            PHƯƠNG THỨC MỚI
    // =====================================================================

    // 1. MỚI: Thống kê Doanh thu theo tuần
    // PHƯƠNG THỨC MỚI: Thống kê doanh thu theo một tuần cụ thể
    public async Task<DoanhThuChiTietThangDto> GetDoanhThuMotTuanAsync(int week, int year)
    {
        // 1. TÍNH TOÁN KHOẢNG NGÀY CỦA TUẦN (theo ISO 8601)
        // Đây là bước quan trọng nhất để Entity Framework có thể truy vấn trong DB.

        DateTime startDate = ISOWeek.ToDateTime(year, week, DayOfWeek.Monday);
        DateTime endDate = ISOWeek.ToDateTime(year, week, DayOfWeek.Sunday).AddHours(23).AddMinutes(59).AddSeconds(59);

        // 2. TRUY VẤN CƠ SỞ DỮ LIỆU
        var weeklyData = await _context.Donhangs
            .Where(dh => dh.NgayDat.HasValue &&
                         dh.TrangThai == "đã giao" &&
                         // So sánh trực tiếp với khoảng ngày đã tính
                         dh.NgayDat.Value >= startDate &&
                         dh.NgayDat.Value <= endDate)
            .ToListAsync();

        // 3. TÍNH TỔNG VÀ TRẢ VỀ
        decimal tongDoanhThu = weeklyData.Sum(dh => (decimal?)dh.TongTien) ?? 0;
        int soDonHang = weeklyData.Count;

        return new DoanhThuChiTietThangDto
        {
            ThoiGian = $"Tuần {week}/{year} ({startDate:dd/MM} - {endDate:dd/MM})",
            TongDoanhThu = tongDoanhThu,
            SoDonHang = soDonHang
        };
    }



    // 2. MỚI: Thống kê Doanh thu theo năm
    // SỬA LỖI TRÍCH XUẤT NĂM CHO MYSQL (Phương thức theo năm)
    // Thêm vào ThongKeService.cs

    //public async Task<List<DoanhThuTheoThoiGianDto>> GetDoanhThuTheoNamAsync()
    //{
    //    var yearlyData = await _context.Donhangs
    //        // 1. Lọc điều kiện: Đơn hàng đã đặt (có NgayDat) và đã giao
    //        .Where(dh => dh.NgayDat.HasValue && dh.TrangThai == "đã giao")

    //        // 2. Nhóm theo Năm
    //        .GroupBy(dh => dh.NgayDat.Value.Year)

    //        .Select(g => new DoanhThuTheoThoiGianDto
    //        {
    //            // ThoiGian là số năm (int), chuyển sang string
    //            ThoiGian = g.Key.ToString(),
    //            DoanhThu = (decimal)(g.Sum(dh => dh.TongTien) ?? 0),
    //            SoDonHang = g.Count()
    //        })
    //        // 3. Sắp xếp theo Năm giảm dần
    //        .OrderByDescending(d => int.Parse(d.ThoiGian))
    //        .ToListAsync();

    //    return yearlyData;
    //}

    // 3. MỚI: Thống kê trạng thái đơn hàng
    public async Task<TrangThaiDonHangDto> GetTrangThaiDonHangAsync()
    {
        var allOrders = await _context.Donhangs.ToListAsync();

        return new TrangThaiDonHangDto
        {
            TongDonHang = allOrders.Count,
            DangXuLy = allOrders.Count(dh => dh.TrangThai == "đang xử lý"),
            DaGiao = allOrders.Count(dh => dh.TrangThai == "đã giao"),
            DaVanChuyen = allOrders.Count(dh => dh.TrangThai == "đã vận chuyển"),
            DaHuy = allOrders.Count(dh => dh.TrangThai == "đã hủy"),
        };
    }

    // 4. MỚI: Thống kê tồn kho theo danh mục
    public async Task<List<TonKhoTheoDanhMucDto>> GetTonKhoTheoDanhMucAsync()
    {
        // Join Sanpham với Danhmuc và tính tổng tồn kho
        var inventoryData = await _context.Sanphams
            .Include(sp => sp.DanhMuc)
            .GroupBy(sp => new { sp.DanhMucId, sp.DanhMuc!.TenDm })
            .Select(g => new TonKhoTheoDanhMucDto
            {
                DanhMucId = g.Key.DanhMucId ?? 0,
                TenDanhMuc = g.Key.TenDm ?? "Không xác định",
                TongSoLuongTonKho = g.Sum(sp => sp.TonKho) ?? 0,
                TongSoLuongSanPham = g.Count()
            })
            .OrderByDescending(d => d.TongSoLuongTonKho)
            .ToListAsync();

        return inventoryData;
    }

    // 5. MỚI: Thống kê Đánh giá theo Rate
    public async Task<ThongKeDanhGiaDto> GetThongKeDanhGiaAsync()
    {
        var allRatings = await _context.Danhgia
            .Where(dg => dg.Rate.HasValue) // Chỉ lấy các đánh giá có Rate
            .ToListAsync();

        int tongSoDanhGia = allRatings.Count;
        if (tongSoDanhGia == 0)
        {
            return new ThongKeDanhGiaDto { TongSoDanhGia = 0 };
        }

        int soLuong5Sao = allRatings.Count(dg => dg.Rate == 5);
        int soLuong4Sao = allRatings.Count(dg => dg.Rate == 4);
        int soLuong3Sao = allRatings.Count(dg => dg.Rate == 3);
        int soLuong2Sao = allRatings.Count(dg => dg.Rate == 2);
        int soLuong1Sao = allRatings.Count(dg => dg.Rate == 1);

        double tyLe5Sao = (double)soLuong5Sao / tongSoDanhGia;

        return new ThongKeDanhGiaDto
        {
            TongSoDanhGia = tongSoDanhGia,
            TyLeDanhGia5Sao = Math.Round(tyLe5Sao, 4), // Làm tròn 4 chữ số thập phân
            SoLuong5Sao = soLuong5Sao,
            SoLuong4Sao = soLuong4Sao,
            SoLuong3Sao = soLuong3Sao,
            SoLuong2Sao = soLuong2Sao,
            SoLuong1Sao = soLuong1Sao
        };
    }


    // =================================================================
    // TRIỂN KHAI PHƯƠNG THỨC BỊ THIẾU
    // =================================================================

    /// <summary>
    /// Thống kê doanh thu theo 12 tháng của một năm cụ thể
    /// </summary>
    public async Task<IEnumerable<DoanhThuMotThangDto>> GetDoanhThuTheoThangTrongNamAsync(int year)
    {
        // 1. Lấy dữ liệu doanh thu từ database
        var monthlyData = await _context.DonhangChitiets
            .Where(dc => dc.DonHang != null &&
                         dc.DonHang.NgayDat.HasValue &&
                         dc.DonHang.NgayDat.Value.Year == year &&
                         // Chỉ tính đơn hàng đã hoàn thành (TrangThai = 'Hoàn thành' là ví dụ)
                         dc.DonHang.TrangThai == "đã giao")
            .GroupBy(dc => dc.DonHang!.NgayDat!.Value.Month)
            .Select(g => new
            {
                Thang = g.Key,
                TongDoanhThu = g.Sum(dc => (decimal)(dc.Gia ?? 0) * (dc.SoLuong ?? 0))
            })
            .ToListAsync();

        // 2. Tạo danh sách đầy đủ 12 tháng (để đảm bảo biểu đồ không bị thiếu cột)
        var allMonths = Enumerable.Range(1, 12)
            .Select(month => new DoanhThuMotThangDto
            {
                // Chuyển số tháng thành tên "Tháng X" hoặc "T1, T2..."
                ThoiGian = "Tháng " + month,
                DoanhThu = 0
            }).ToList();

        // 3. Cập nhật dữ liệu vào danh sách 12 tháng
        foreach (var item in monthlyData)
        {
            var targetMonth = allMonths.FirstOrDefault(m => m.ThoiGian == "Tháng " + item.Thang);
            if (targetMonth != null)
            {
                targetMonth.DoanhThu = item.TongDoanhThu;
            }
        }

        return allMonths.OrderBy(m => int.Parse(m.ThoiGian.Replace("Tháng ", "")));
    }
}