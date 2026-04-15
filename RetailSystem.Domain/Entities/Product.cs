using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RetailSystem.Domain.Enums;

namespace RetailSystem.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;
        [Column(TypeName ="decimal(18,2)")]
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public ProductStatus Status { get; set; } = ProductStatus.Draft;
        public List<string> ImageUrl { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
        public List<Category> Categories { get; set; } = new List<Category>();
    }
}
