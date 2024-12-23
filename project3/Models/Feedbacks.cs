using System.ComponentModel.DataAnnotations;

namespace Project3.Models
{
    public class Feedbacks
    {
        [Key]
        public int FeedbackId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}