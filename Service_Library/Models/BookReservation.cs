using Service_Library.Entities;
using System.ComponentModel.DataAnnotations;

namespace Service_Library.Models
{
    public class BookReservation
    {
        [Key]
        public int ReservationId { get; set; }
        public int BookId { get; set; }
        public Book Book { get; set; }
        public int UserId { get; set; }
        public UserAccount User { get; set; }
        public DateTime ReservationExpiry { get; set; }
    }

}
