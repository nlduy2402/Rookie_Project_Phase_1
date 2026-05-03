using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Shared.ViewModels
{
    public class ProductIndexViewModel
    {
        public List<ProductViewModel> Items { get; set; }

        public int Page { get; set; }
        public int TotalPages { get; set; }
    }
}
