using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Domain.Entities
{
    public class ReviewImage
    {
        public int Id { get; set; }

        [Required]
        public int ReviewId { get; set; }

        [Required]
        [MaxLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        [ForeignKey(nameof(ReviewId))]
        public Review Review { get; set; } = null!;
    }
}
