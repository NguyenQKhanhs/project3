using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project3.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public DateTime StartDate { get; set; }
        public string Duration { get; set; }  // Example: "28 November 4 weeks"
        public string StudyLevel { get; set; }  // Example: "Beginner", "Intermediate"
        public string Content { get; set; }  // Stored as '\n' separated
        public string? ImageUrl { get; set; }
        public int CategoryId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? ExamDate { get; set; }
        // Relationships
        public Category Category { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }

        public ICollection<Class> Classes { get; set; }

        public ICollection<CustomerInformation> CustomerInformations { get; set;}
    }
}