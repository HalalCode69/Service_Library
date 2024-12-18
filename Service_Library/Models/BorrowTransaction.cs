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
        public int UserId { get; set; } // Assume you have UserAccount model
        public UserAccount User { get; set; }
        public DateTime BorrowDate { get; set; } // Start of borrow
        public DateTime ReturnDate { get; set; } // Due date for return
        public bool IsReturned { get; set; } = false; // Track return status
        public bool ReminderSent { get; set; } = false;
    }
}
