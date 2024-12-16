using System.ComponentModel.DataAnnotations;

namespace Project3.Models
{
    public class OrderDetail
    {
        [Key]
        public int OrderDetailId { get; set; }
        public int OrderId { get; set; }
        public int CourseId { get; set; }
        public decimal Price { get; set; }
        public int? DiscountId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Relationships
        public Order Order { get; set; }
        public Course Course { get; set; }
        public Discount? Discount { get; set; }
    }
}