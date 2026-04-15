using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Shared.DTOs
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public string Status { get; set; } = "";

        public List<string> ImageUrl { get; set; } = new();

        public List<string> Categories { get; set; } = new();

        public DateTime CreatedAt { get; set; }
    }
}
