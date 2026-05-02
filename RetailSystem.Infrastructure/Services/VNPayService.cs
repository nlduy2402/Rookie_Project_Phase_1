using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RetailSystem.Domain.Entities;
using RetailSystem.Domain.Models;
using RetailSystem.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RetailSystem.Infrastructure.Services
{
    public class VNPayService : IVNPayService
    {
        private readonly IConfiguration _config;

        public VNPayService(IConfiguration config)
        {
            _config = config;
        }

        public string CreatePaymentUrl(HttpContext context, Order order)
        {
            var vnp = new SortedDictionary<string, string>();

            var timeNow = DateTime.Now;

            vnp.Add("vnp_Version", "2.1.0");
            vnp.Add("vnp_Command", "pay");
            vnp.Add("vnp_TmnCode", _config["VNPay:TmnCode"]);

            vnp.Add("vnp_Amount", ((int)(order.TotalAmount * 100)).ToString());
            vnp.Add("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            vnp.Add("vnp_CurrCode", "VND");
            vnp.Add("vnp_IpAddr", "127.0.0.1");
            vnp.Add("vnp_Locale", "vn");

            vnp.Add("vnp_OrderInfo", $"Thanh toan don hang {order.Id}");
            vnp.Add("vnp_OrderType", "other");

            vnp.Add("vnp_ReturnUrl", _config["VNPay:ReturnUrl"]);
            vnp.Add("vnp_TxnRef", order.TxnRef);

            // 👉 VNPay yêu cầu có param này
            vnp.Add("vnp_SecureHashType", "HmacSHA512");

            // 🔥 Build query + signData giống hệt nhau (đây là KEY FIX)
            var queryBuilder = new StringBuilder();
            var hashDataBuilder = new StringBuilder();

            foreach (var item in vnp)
            {
                if (!string.IsNullOrEmpty(item.Value))
                {
                    // query
                    queryBuilder.Append(WebUtility.UrlEncode(item.Key) + "=" + WebUtility.UrlEncode(item.Value) + "&");

                    // signData (loại SecureHashType)
                    if (item.Key != "vnp_SecureHashType")
                    {
                        hashDataBuilder.Append(item.Key + "=" + WebUtility.UrlEncode(item.Value) + "&");
                    }
                }
            }

            // remove last "&"
            string query = queryBuilder.ToString().TrimEnd('&');
            string signData = hashDataBuilder.ToString().TrimEnd('&');

            var secureHash = HmacSHA512(_config["VNPay:HashSecret"], signData);

            var paymentUrl = $"{_config["VNPay:BaseUrl"]}?{query}&vnp_SecureHash={secureHash}";

            return paymentUrl;
        }

        public PaymentResult Execute(IQueryCollection query)
        {
            var vnpData = query
                .Where(q => q.Key.StartsWith("vnp_") &&
                            q.Key != "vnp_SecureHash" &&
                            q.Key != "vnp_SecureHashType")
                .ToDictionary(k => k.Key, v => v.Value.ToString());

            var sorted = new SortedDictionary<string, string>(vnpData);

            var hashDataBuilder = new StringBuilder();

            foreach (var item in sorted)
            {
                if (!string.IsNullOrEmpty(item.Value))
                {
                    hashDataBuilder.Append(item.Key + "=" + WebUtility.UrlEncode(item.Value) + "&");
                }
            }

            string signData = hashDataBuilder.ToString().TrimEnd('&');

            var hash = HmacSHA512(_config["VNPay:HashSecret"], signData);

            var isValid = hash.Equals(query["vnp_SecureHash"], StringComparison.OrdinalIgnoreCase);

            return new PaymentResult
            {
                Success = isValid && query["vnp_ResponseCode"] == "00",
                TxnRef = query["vnp_TxnRef"],
                TransactionNo = query["vnp_TransactionNo"],
                ResponseCode = query["vnp_ResponseCode"]
            };
        }

        private string HmacSHA512(string key, string input)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(input);

            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
