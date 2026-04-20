using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Domain.Entities
{
    public class UserToken
    {

        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public User? User { get; set; }

        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;

        public DateTime ExpiredAt { get; set; } = DateTime.UtcNow;
        
    }
}
