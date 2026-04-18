using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Shared.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string PriceFormatted => $"{Price:N0}đ";

        public string ImageUrl { get; set; } = string.Empty;

        // Specs
        public string ChipSet { get; set; } = string.Empty;
        public string RAM { get; set; } = string.Empty;
        public string SSD { get; set; } = string.Empty;

        // Gộp sẵn cho UI
        public List<string> Specs { get; set; } = new();
    }
}
