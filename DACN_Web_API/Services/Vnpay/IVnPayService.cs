using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DACN_Web_API.Models;

namespace DACN_Web_API.Services.Vnpay
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
        PaymentResponseModel PaymentExecute(IQueryCollection collections);

    }
}