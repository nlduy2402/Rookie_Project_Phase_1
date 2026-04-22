using RetailSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Shared.DTOs
{
    public class UpdateProductDTO
    {
        [Required]
        public int Id { get; set; }
        [MaxLength(200)]
        public string? Name { get; set; } 

        [MaxLength(300)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue),Column(TypeName = "decimal(18,2)")]
        public decimal? Price { get; set; }

        [Range(0, int.MaxValue)]
        public int? Quantity { get; set; }
        public string? ChipSet { get; set; } 
        public string? RAM { get; set; }
        public string? SSD { get; set; } 
        public int? CategoryId { get; set; }
    }
}
