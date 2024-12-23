using System.ComponentModel.DataAnnotations;

namespace Project3.Models
{
    public class Class
    {
        [Key]
        public int ClassesId { get; set; }
        public int? CourseId { get; set; }
        public int InstructorId { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Relationships
        public Course Course { get; set; }
        public Instructor Instructor { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; }
        public ICollection<CustomerInformation> CustomerInformations { get; set; } // Relationship with CustomerInformation
    }
}