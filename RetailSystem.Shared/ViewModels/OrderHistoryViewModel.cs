using RetailSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Shared.ViewModels
{
    public class OrderHistoryViewModel
    {
        public List<OrderItemVM> Items { get; set; }

        public int Page { get; set; }
        public int TotalPages { get; set; }
    }

    public class OrderItemVM
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; }

        public PaymentStatus PaymentStatus { get; set; }
        public string PaymentMethod { get; set; }

        public List<OrderDetailVM> OrderDetails { get; set; }
    }

    public class OrderDetailVM
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
    }
}
