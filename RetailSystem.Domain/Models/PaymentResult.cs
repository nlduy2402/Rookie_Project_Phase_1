using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Domain.Models
{
    public class PaymentResult
    {
        public bool Success { get; set; }

        public string OrderId { get; set; } = string.Empty;
        public string TxnRef { get; set; } = string.Empty;

        public string ResponseCode { get; set; } = string.Empty;
        public string TransactionNo { get; set; } = string.Empty;
    }
}
