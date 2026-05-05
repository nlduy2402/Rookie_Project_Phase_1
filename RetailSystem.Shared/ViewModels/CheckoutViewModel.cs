using RetailSystem.Domain.Entities;
using RetailSystem.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Shared.ViewModels
{
    public class CheckoutViewModel
    {
        public OrderDTO OrderData { get; set; } = new();
        public List<CartItem> CartItems { get; set; } = new();
        public string PaymentMethod { get; set; } = "COD"; 
        public List<string> StockErrors { get; set; } = new();
        public bool HasStockError => StockErrors.Any();
    }
}
