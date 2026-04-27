using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RetailSystem.Domain.Enums;

namespace RetailSystem.Domain.Entities
{
    public class Product : TEntity
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(300)]
        public string Description { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be >=0")]
        public int Quantity { get; set; }
        public string ChipSet { get; set; } = string.Empty;
        public string RAM { get; set; } = string.Empty;
        public string SSD { get; set; } = string.Empty;
        public List<ProductImage> Images { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }
    }
}
