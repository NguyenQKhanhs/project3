using System.ComponentModel.DataAnnotations;

namespace Project3.Models
{
    public class Instructor
    {
        [Key]
        public int InstructorId { get; set; }
        public string Name { get; set; }
        public string? Gender { get; set; }
        public string? Bio { get; set; }
        public string? ImageLink { get; set; }
        public string? PhoneNumber { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Relationships
        public ICollection<Class> Classes { get; set; }
    }
}