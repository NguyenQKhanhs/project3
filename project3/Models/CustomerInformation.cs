using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Project3.Models
{
    public class CustomerInformation
    {
        [Key]
        [StringLength(8)] // Ensures that the ID is 8 characters long
        public string CustomerInformationId { get; set; } // 8 characters

        [Required] // Makes FullName a required field
        public string FullName { get; set; }

        [EmailAddress] // Validates that the Email is in a valid email format
        public string Email { get; set; }

        public string? PhoneNumber { get; set; }

        [Range(0, 100)] // Ensures Mark is within a valid range, adjust as necessary
        public decimal? Mark { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        public string? Schedule { get; set; }

        // Foreign key properties
        public int? ClassesId { get; set; } // Foreign key to Class
        public int? CourseId { get; set; } // Foreign key to Course

        // Navigation properties
        public virtual Class Class { get; set; }
        public virtual Course Course { get; set; }

        // Relationships
        public ICollection<Order> Orders { get; set; } = new List<Order>(); // Initialize to avoid null reference
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>(); // Initialize to avoid null reference
    }
}
