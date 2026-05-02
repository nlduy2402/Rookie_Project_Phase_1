using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Shared.DTOs
{
    public class CheckoutDTO
    {
        [Required(ErrorMessage = "Full name is required")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required")]
        //[Phone(ErrorMessage = "")]
        public string PhoneNumber { get; set; } = string.Empty;

        public string? Note { get; set; } 

        //public string PaymentMethod { get; set; }
    }
}
