using Service_Library.Entities;

namespace Service_Library.Models
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        public int UserId { get; set; }
        public UserAccount User { get; set; }
        public int BookId { get; set; }
        public Book Book { get; set; }
        public string TransactionType { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Status { get; set; }
    }
}
