using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Shared.DTOs
{
    public class CreateProductDTO
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public List<string> ImageUrl { get; set; } = new();

        // nếu muốn gán category khi tạo
        public List<int> CategoryIds { get; set; } = new();
        
    }
}
