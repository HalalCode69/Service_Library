using System;

namespace Service_Library.Models
{
    public class Feedback
    {
        public int FeedbackId { get; set; }
        public int BookId { get; set; }
        public string UserId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime Date { get; set; }
        public string FirstName { get; set; } = ""; // Default to empty string
        public string LastName { get; set; } = "";  // Default to empty string
    }
}

