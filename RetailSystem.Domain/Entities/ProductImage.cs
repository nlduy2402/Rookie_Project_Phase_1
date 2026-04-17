using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RetailSystem.Domain.Entities
{
    public class ProductImage
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Url { get; set; } = string.Empty;

        public int ProductId { get; set; }
        [JsonIgnore]
        public Product? Product { get; set; }
    }
}
