using Service_Library.Entities;

namespace Service_Library.Models
{
    public class UserRating
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public int UserId { get; set; }
        public int Rating { get; set; }

        // Navigation Properties
        public Book Book { get; set; }
        public UserAccount User { get; set; }
    }

}
