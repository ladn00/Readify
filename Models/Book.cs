using System.ComponentModel.DataAnnotations;

namespace Readify.Models
{
    public class Book
    {

        public int BookId { get; set; }

        [Required(ErrorMessage = "Название обязательно")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Автор обязателен")]
        public string Author { get; set; }

        [Required(ErrorMessage = "Описание обязательно")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Путь к файлу обязателен")]
        public string? FilePath { get; set; }

        public string? Photo {  get; set; }  
        public DateTime UploadedAt { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public int UserId { get; set; }
        public User? UploadedByUser { get; set; }

    }
}
