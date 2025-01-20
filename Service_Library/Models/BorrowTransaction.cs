using Service_Library.Entities;
using System.ComponentModel.DataAnnotations;

namespace Service_Library.Models
{
    public class BorrowTransaction
    {
        [Key]
        public int TransactionId { get; set; }
        public int BookId { get; set; }
        public Book Book { get; set; }
        public int UserId { get; set; }
        public UserAccount User { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public bool IsReturned { get; set; } = false;
        public bool ReminderSent { get; set; } = false;
    }
}
