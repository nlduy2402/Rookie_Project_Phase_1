using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;


namespace RetailSystem.Shared.DTOs
{
    public class CreateProductWithImageDTO
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(300)]
        public string Description { get; set; } = string.Empty;

        [Range(0, double.MaxValue), Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; } = 0;
        public string ChipSet { get; set; } = string.Empty;
        public string RAM { get; set; } = string.Empty;
        public string SSD { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        public IFormFileCollection? Images { get; set; }
    }
}
