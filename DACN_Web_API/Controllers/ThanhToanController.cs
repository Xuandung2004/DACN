using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DACN_Web_API.Models;
using DACN_Web_API.Services.Vnpay;
using Microsoft.EntityFrameworkCore;

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
        // Tạo URL thanh toán VNPay cho đơn hàng
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
            try
            {
                var response = _vnPayService.PaymentExecute(Request.Query);

                // Try to obtain order id (VNPay txn ref) safely
                var orderIdStr = response?.OrderId;
                if (string.IsNullOrEmpty(orderIdStr))
                {
                    orderIdStr = Request.Query.ContainsKey("vnp_TxnRef") ? Request.Query["vnp_TxnRef"].ToString() : null;
                }

                if (!int.TryParse(orderIdStr, out var donHangId))
                {
                    var failUrlBadId = $"http://127.0.0.1:5500/FrontendWeb/user/checkout-success.html?status=fail&message={Uri.EscapeDataString("OrderId không hợp lệ")}";
                    return Redirect(failUrlBadId);
                }

                // Read amount from vnp_Amount (VNPay sends amount as integer *100).
                decimal amount = 0m;
                if (Request.Query.ContainsKey("vnp_Amount") && long.TryParse(Request.Query["vnp_Amount"].ToString(), out var amtLong))
                {
                    amount = amtLong / 100m;
                }
                else if (!string.IsNullOrEmpty(response?.OrderDescription))
                {
                    // fallback: try to parse last token from OrderDescription
                    var parts = response.OrderDescription.Split(' ');
                    if (parts.Length > 0 && decimal.TryParse(parts.Last(), out var parsed))
                        amount = parsed;
                }

                if (response != null && response.Success)
                {
                    var donHang = _context.Donhangs.FirstOrDefault(d => d.Id == donHangId);

                    if (donHang != null)
                    {
                        var thanhToan = new Thanhtoanon
                        {
                            DonHangId = donHangId,
                            MaGiaoDich = response.PaymentId,
                            SoTien = amount,
                            PhuongThuc = "VNPay",
                            ThoiGian = DateTime.Now,
                            TrangThai = "Thành công",
                            NoiDung = response.OrderDescription
                        };
                        _context.Thanhtoanons.Add(thanhToan);
                        _context.SaveChanges();
                    }

                    var successUrl = $"http://127.0.0.1:5500/FrontendWeb/user/checkout-success.html?orderId={donHangId}&status=success&transactionId={Uri.EscapeDataString(response.PaymentId ?? string.Empty)}&amount={amount}";
                    return Redirect(successUrl);
                }
                else
                {
                    var failUrl = $"http://127.0.0.1:5500/FrontendWeb/user/checkout-success.html?orderId={donHangId}&status=fail&message={Uri.EscapeDataString("Thanh toán thất bại")}&responseCode={Uri.EscapeDataString(response?.VnPayResponseCode ?? string.Empty)}";
                    return Redirect(failUrl);
                }
            }
            catch (Exception ex)
            {
                var errUrl = $"http://127.0.0.1:5500/FrontendWeb/user/checkout-success.html?status=fail&message={Uri.EscapeDataString("Lỗi xử lý callback: " + ex.Message)}";
                return Redirect(errUrl);
            }
        }
    }
}


