using System.ComponentModel.DataAnnotations;

namespace Project3.Models
{
    public class Token
    {
        [Key]
        public int TokenId { get; set; }
        public int AccountId { get; set; }
        public string TokenValue { get; set; }
        public string TokenType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }

        // Relationships
        public Account Account { get; set; }
    }
}