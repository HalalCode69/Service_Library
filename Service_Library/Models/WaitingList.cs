using Service_Library.Entities;
using System.ComponentModel.DataAnnotations;

namespace Service_Library.Models
{
    public class WaitingList
    {
        [Key]
        public int WaitingListId { get; set; }
        public int BookId { get; set; }
        public Book Book { get; set; }
        public int UserId { get; set; }
        public UserAccount User { get; set; }
        public DateTime AddedDate { get; set; } = DateTime.Now;
    }
}
