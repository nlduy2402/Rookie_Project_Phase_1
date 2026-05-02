using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Domain.Entities
{
    public class Order : TEntity
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public List<OrderDetail> OrderDetails { get; set; } = new();
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        public string PaymentMethod { get; set; } = "COD"; // hoặc VNPay
        public string? TxnRef { get; set; } // lưu vnp_TxnRef
    }
    public enum OrderStatus { Pending, Processing, Shipped, Completed, Cancelled }
    public enum PaymentStatus
    {
        Pending,
        Paid,
        Failed
    }
}
