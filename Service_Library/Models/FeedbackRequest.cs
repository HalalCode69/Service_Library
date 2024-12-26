namespace Service_Library.Models
{
    public class FeedbackRequest
    {
        public int BookId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }
}
