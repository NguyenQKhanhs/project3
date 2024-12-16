using System.ComponentModel.DataAnnotations;

namespace Project3.Models
{
    public class Enrollment
    {
        [Key]
        public int EnrollmentId { get; set; }
        public int ClassesId { get; set; }
        public string CustomerInformationId { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Relationships
        public Class Classes { get; set; }
        public CustomerInformation CustomerInformation { get; set; }
    }
}