using System.ComponentModel.DataAnnotations;

namespace Project3.Models
{
    public class Account
    {
        [Key]
        public int AccountId { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Relationships
        public ICollection<Token> Tokens { get; set; }
    }
}