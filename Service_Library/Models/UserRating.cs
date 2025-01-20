using Service_Library.Entities;

namespace Service_Library.Models
{
    public class UserRating
    {
        public int Id { get; set; }
        public int? BookId { get; set; }
        public int UserId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime Date { get; set; }
        public Book Book { get; set; }
        public UserAccount User { get; set; }
        public bool IsWebsiteRating { get; set; }
    }

}
