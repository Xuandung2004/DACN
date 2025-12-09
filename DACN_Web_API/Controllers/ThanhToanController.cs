using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using DACN_Web_API.Models;
using DACN_Web_API.Services.Vnpay;

namespace DACN_Web_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ThanhToanController : ControllerBase
    {
        private readonly IVnPayService _vnPayService;
        private readonly CsdlFinal1Context _context;

        public ThanhToanController(IVnPayService vnPayService, CsdlFinal1Context context)
        {
            _vnPayService = vnPayService;
            _context = context;
        }

        // POST: api/ThanhToan/create-payment-url
        [HttpPost("create-payment-url")]
        public IActionResult CreatePaymentUrl([FromBody] PaymentInformationModel model)
        {
            if (model == null || model.Amount <= 0)
                return BadRequest(new { message = "Dữ liệu thanh toán không hợp lệ" });

            try
            {
                var paymentUrl = _vnPayService.CreatePaymentUrl(model, HttpContext);

                return Ok(new
                {
                    success = true,
                    message = "Tạo URL thanh toán thành công",
                    paymentUrl = paymentUrl,
                    amount = model.Amount,
                    orderDescription = model.OrderDescription,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi tạo URL thanh toán: " + ex.Message });
            }
        }

        // GET: api/ThanhToan/payment-return
        // Callback từ VNPay sau khi thanh toán
        [HttpGet("payment-return")]
        public IActionResult PaymentReturn()
        {
            // Định nghĩa URL mặc định để tránh lỗi khi không parse được OrderId
            var defaultFailUrl = $"http://127.0.0.1:5500/FrontendWeb/user/checkout-success.html?status=fail&message={Uri.EscapeDataString("Lỗi xử lý giao dịch")}";

            try
            {
                var response = _vnPayService.PaymentExecute(Request.Query);
                var vnpResponseCode = Request.Query.ContainsKey("vnp_ResponseCode") ? Request.Query["vnp_ResponseCode"].ToString() : null;

                // 1. Lấy Order Id (TxnRef)
                var orderIdStr = response?.OrderId;
                if (string.IsNullOrEmpty(orderIdStr))
                {
                    orderIdStr = Request.Query.ContainsKey("vnp_TxnRef") ? Request.Query["vnp_TxnRef"].ToString() : null;
                }

                if (!int.TryParse(orderIdStr, out var donHangId))
                {
                    return Redirect(defaultFailUrl + $"&detail={Uri.EscapeDataString("OrderId không hợp lệ")}");
                }

                // 2. Lấy Số tiền
                decimal amount = 0m;
                if (Request.Query.ContainsKey("vnp_Amount") && long.TryParse(Request.Query["vnp_Amount"].ToString(), out var amtLong))
                {
                    amount = amtLong / 100m;
                }

                // **********************************************
                // 3. XỬ LÝ KẾT QUẢ: Thành công chỉ khi Hash đúng VÀ vnp_ResponseCode = "00"
                // **********************************************
                var donHang = _context.Donhangs.FirstOrDefault(d => d.Id == donHangId);
                if (response != null && response.Success && vnpResponseCode == "00")
                {
                    // Chú ý: Cần kiểm tra DonHang đã được thanh toán chưa để tránh trùng lặp


                    // Nếu đơn hàng tồn tại VÀ chưa được thanh toán
                    if (donHang != null /* && donHang.TrangThaiThanhToan != "Thành công" */)
                    {
                        // 3a. Ghi nhận giao dịch thành công vào DB
                        var thanhToan = new Thanhtoanon
                        {
                            DonHangId = donHangId,
                            MaGiaoDich = response.PaymentId,
                            SoTien = amount,
                            PhuongThuc = "VNPay",
                            ThoiGian = DateTime.Now,
                            TrangThai = "Thành công", // Cập nhật trạng thái thanh toán
                            NoiDung = response.OrderDescription
                        };
                        _context.Thanhtoanons.Add(thanhToan);
                        _context.SaveChanges();

                        // Cập nhật trạng thái Đơn hàng (nếu cần)
                        // Xóa giỏ hàng của khách sau khi thanh toán VNPay thành công
                        var cartItems = _context.Giohangs.Where(g => g.NguoiDungId == donHang.NguoiDungId).ToList();
                        // 5. Thêm chi tiết đơn hàng
                        foreach (var item in cartItems)
                        {
                            var sp = _context.Sanphams.Find(item.SanPhamId);

                            var chiTiet = new DonhangChitiet
                            {
                                DonHangId = donHang.Id,
                                SanPhamId = item.SanPhamId,
                                KichThuocId = item.KichThuocId,
                                SoLuong = item.SoLuong,
                                Gia = (decimal)sp.Gia
                            };

                            _context.DonhangChitiets.Add(chiTiet);
                        }
                        if (cartItems.Any())
                        {
                            _context.Giohangs.RemoveRange(cartItems);
                            _context.SaveChanges();
                        }
                        // donHang.TrangThaiThanhToan = "Đã thanh toán";
                        // _context.SaveChanges();
                    }

                    // 3b. Chuyển hướng đến trang thành công
                    var successUrl = $"http://127.0.0.1:5500/FrontendWeb/user/checkout-success.html?orderId={donHangId}&status=success&transactionId={Uri.EscapeDataString(response.PaymentId ?? string.Empty)}&amount={amount}";
                    return Redirect(successUrl);
                }
                else
                {
                    // 4. XỬ LÝ THẤT BẠI / BỊ HỦY
                    string message;

                    if (vnpResponseCode == "24")
                    {
                        message = "Giao dịch bị hủy bởi khách hàng.";
                    }
                    else if (!response.Success)
                    {
                        // Hash không hợp lệ (lỗi bảo mật)
                        message = "Lỗi bảo mật (Invalid Signature) - Dữ liệu giao dịch không hợp lệ.";
                    }
                    else
                    {
                        // Tra cứu mã lỗi chi tiết từ vnpResponseCode nếu cần, hoặc dùng mô tả chung
                        message = GetVnPayErrorMessage(vnpResponseCode) ?? $"Thanh toán thất bại (Mã: {vnpResponseCode}).";
                    }
                    _context.Donhangs.Remove(donHang);
                    _context.SaveChanges();
                    // Chuyển hướng đến trang thất bại
                    var failUrl = $"http://127.0.0.1:5500/FrontendWeb/user/checkout-success.html?orderId={donHangId}&status=fail&message={Uri.EscapeDataString(message)}&responseCode={Uri.EscapeDataString(vnpResponseCode ?? string.Empty)}";
                    return Redirect(failUrl);
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi hệ thống trong quá trình callback
                var errUrl = defaultFailUrl + $"&detail={Uri.EscapeDataString("Lỗi hệ thống: " + ex.Message)}";
                return Redirect(errUrl);
            }
        }

        // Hàm phụ trợ để lấy thông báo lỗi chi tiết từ mã phản hồi VNPay
        private string GetVnPayErrorMessage(string code)
        {
            return code switch
            {
                "07" => "Trừ tiền thành công, giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường).",
                "09" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng chưa đăng ký dịch vụ Internet Banking tại ngân hàng.",
                "10" => "Giao dịch không thành công do: Khách hàng xác thực thông tin thẻ/tài khoản không đúng quá 3 lần",
                "11" => "Giao dịch không thành công do: Đã hết hạn chờ thanh toán.",
                "12" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng bị khóa.",
                "13" => "Giao dịch không thành công do Quý khách nhập sai mật khẩu xác thực giao dịch (OTP).",
                "24" => "Giao dịch không thành công do: Khách hàng hủy giao dịch.",
                "51" => "Giao dịch không thành công do: Tài khoản của quý khách không đủ số dư để thực hiện giao dịch.",
                "65" => "Giao dịch không thành công do: Tài khoản của Quý khách đã vượt quá hạn mức giao dịch trong ngày.",
                "75" => "Ngân hàng thanh toán đang bảo trì.",
                "79" => "Giao dịch không thành công do: Khách hàng nhập sai mật khẩu thanh toán quá số lần quy định.",
                "99" => "Các lỗi khác (lỗi còn lại, không có trong danh sách mã lỗi đã liệt kê)",
                _ => null,
            };
        }
    }
}