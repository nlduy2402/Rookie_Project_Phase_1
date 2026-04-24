using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RetailSystem.Shared.DTOs;

namespace RetailSystem.Shared.DTOs
{
    public class CartDTO
    {
        public int Count { get; set; }
        public List<CartItemDTO> Items { get; set; } = new();
        public decimal Total { get; set; }
    }
}
