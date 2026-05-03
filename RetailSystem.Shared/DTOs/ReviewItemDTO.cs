using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Shared.DTOs
{
    public class ReviewItemDTO
    {
        public int ProductId { get; set; }

        public int Rating { get; set; }

        public string? Comment { get; set; }

        public List<string>? ImageUrls { get; set; }
    }
}
