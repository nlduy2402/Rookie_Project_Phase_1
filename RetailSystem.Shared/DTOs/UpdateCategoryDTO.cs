using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Shared.DTOs
{
    public class UpdateCategoryDTO
    {
        [Required(ErrorMessage = "Name is required."),MaxLength(30,ErrorMessage ="Category name doesn't greater than 30 charater")]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
