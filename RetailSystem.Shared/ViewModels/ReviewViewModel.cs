using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Shared.ViewModels
{
    public class ReviewViewModel
    {
        public int OrderId { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public int Rating { get; set; } = 5;

        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
