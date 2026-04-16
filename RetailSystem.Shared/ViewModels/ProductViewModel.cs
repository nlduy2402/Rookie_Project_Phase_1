using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Shared.ViewModels
{
    public class ProductViewModel
    {
        public string Name { get; set; } = "";
        public string PriceDisplay { get; set; } = "";
        public string Status { get; set; } = "";
        public List<string> Categories { get; set; } = new();
    }
}
