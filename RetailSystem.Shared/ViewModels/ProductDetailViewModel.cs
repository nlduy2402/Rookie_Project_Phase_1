using RetailSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Shared.ViewModels
{
    public class ProductDetailViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Price { get; set; }
        public string PriceFormatted => $"{Price:N0}đ";

        public string CategoryName { get; set; } = "";

        public List<string> Images { get; set; } = new();

        public string ChipSet { get; set; } = "";
        public string RAM { get; set; } = "";
        public string SSD { get; set; } = "";

        public double AverageRating { get; set; } = 0;
        public int TotalReviews { get; set; } = 0;
        public List<ReviewViewModel> Reviews { get; set; } = new();
    }
}

