using System.ComponentModel.DataAnnotations.Schema;

namespace Readify.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        public string CommentText { get; set; }
        public DateTime CommentedAt { get; set; }

        public int BookId { get; set; }
        public Book Book { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
    }
}
