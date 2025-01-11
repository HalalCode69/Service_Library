using Service_Library.Entities;

namespace Service_Library.Models
{
    public class UserRating
    {
        public int Id { get; set; }
        public int? BookId { get; set; } // Nullable to allow website rating
        public int UserId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } // Add comment property
        public DateTime Date { get; set; } // Add Date property

        // Navigation Properties
        public Book Book { get; set; }
        public UserAccount User { get; set; }
        public bool IsWebsiteRating { get; set; } // New property to indicate website rating
    }

}
