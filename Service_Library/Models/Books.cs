using Humanizer.Localisation;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Library.Models
{
    public class Book
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public string Format { get; set; }
        public decimal BorrowPrice { get; set; }
        public int BorrowedCopies { get; set; }
        public decimal BuyPrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public DateTime? DiscountEndDate { get; set; }
        public decimal? PreviousBuyPrice { get; set; }
        public int AvailableCopies { get; set; }
        public bool IsBorrowable { get; set; }
        public byte[]? CoverImage { get; set; }
        public string Category { get; set; }
        public bool IsOwnedByCurrentUser { get; set; }
        public string AgeLimit { get; set; }
        public List<Feedback> Feedbacks { get; set; } = new List<Feedback>();
        public int YearOfPublishing { get; set; }
        public double AverageRating { get; set; } = 0.0;
        public int RatingCount { get; set; } = 0;
        public int? UserRating { get; set; }
        public bool UserHasPostedFeedback { get; set; }
        public byte[]? BookContent { get; set; }

        [NotMapped] 
        public string UserComment { get; set; } = "";

        [NotMapped]
        public int EstimatedAvailabilityInDays { get; set; }

        [NotMapped]
        public int ActiveBorrowCount { get; set; }

        [NotMapped]
        public bool IsReservedForCurrentUser { get; set; }

        [NotMapped]
        public bool IsReservedForOtherUser { get; set; }

        [NotMapped]
        public int WaitingListCount { get; set; } = 0;

        [NotMapped]
        public int UserWaitingPosition { get; set; } = 0;

        [NotMapped]
        public bool IsUserOnWaitingList { get; set; } = false;

        [NotMapped]
        public string RemainingBorrowTime { get; set; } = string.Empty;

        [NotMapped]
        public bool IsAlreadyBorrowed { get; set; }

        [NotMapped]
        public int UsersBeforeInWaitList { get; set; }

        [NotMapped]
        public int? BorrowTransactionId { get; set; }

        public List<Genre> Genres { get; set; } = new List<Genre>();
    }
}
