using Microsoft.AspNetCore.Http;
using RetailSystem.Domain.Entities;
using RetailSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Infrastructure.Services.Interfaces
{
    public interface IVNPayService
    {
        string CreatePaymentUrl(HttpContext context, Order order);
        PaymentResult Execute(IQueryCollection query);
    }
}
