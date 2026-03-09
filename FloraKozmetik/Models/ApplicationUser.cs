using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FloraKozmetik.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        public DateTime? BirthDate { get; set; }

        [StringLength(20)]
        public string Gender { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
