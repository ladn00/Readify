namespace Readify.Models
{
    public class Book
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
        public string Photo {  get; set; }  
        public DateTime UploadedAt { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public int UserId { get; set; }
        public User UploadedByUser { get; set; }

    }
}
