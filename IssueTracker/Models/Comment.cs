using System.ComponentModel.DataAnnotations;

namespace IssueTracker.Models
{
    public class Comment
    {
        public int Id { get; set; }
        [Required]
        public string? Text { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int UserId { get; set; }
        [Required]
        public User? User { get; set; }
        public int IssueId { get; set; }
        [Required]
        public Issue? Issue { get; set; }
    }
}
