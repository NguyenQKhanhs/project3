using System.ComponentModel.DataAnnotations;

namespace Project3.Models
{
    public class CustomerInformation
    {
        [Key]
        public string CustomerInformationId { get; set; } // 8 characters
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public decimal? Mark { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Relationships
        public ICollection<Order> Orders { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; }
    }
}