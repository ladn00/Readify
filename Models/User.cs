using System.Xml.Linq;

namespace Readify.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }

        public ICollection<Book> Books { get; set; } = new List<Book>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
