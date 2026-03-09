using System.ComponentModel.DataAnnotations;

namespace FloraKozmetik.Models
{
    public class Address
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Title { get; set; } = string.Empty;  // "Ev", "İş" vb.

        [Required]
        [StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string District { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FullAddress { get; set; } = string.Empty;

        public bool IsDefault { get; set; } = false;

        // Navigation
        public ApplicationUser? User { get; set; }
    }
}
