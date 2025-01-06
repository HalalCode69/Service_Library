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
        public decimal? DiscountPrice { get; set; } // Discounted price (optional)
        public DateTime? DiscountEndDate { get; set; } // Discount expiration date
        public decimal? PreviousBuyPrice { get; set; } // Previous price for strikethrough
        public int AvailableCopies { get; set; }
        public bool IsBorrowable { get; set; }
        public byte[]? CoverImage { get; set; }
        public string Category { get; set; }
        public bool IsOwnedByCurrentUser { get; set; }
        public string AgeLimit { get; set; } // New property for age limitation
        public List<Feedback> Feedbacks { get; set; } = new List<Feedback>(); // Initialize to an empty list
        public int YearOfPublishing { get; set; } // New property for year of publishing
        public double AverageRating { get; set; } = 0.0; // New property for average rating
        public int RatingCount { get; set; } = 0; // New property for rating count
        public int? UserRating { get; set; } // Nullable to indicate no rating
        public bool UserHasPostedFeedback { get; set; }
        public byte[]? BookContent { get; set; } // New property for book content

        [NotMapped] // Ensure it's not part of the database
        public string UserComment { get; set; } = "";

        [NotMapped] // Ensure this is not stored in the database
        public int EstimatedAvailabilityInDays { get; set; }

        [NotMapped]
        public int ActiveBorrowCount { get; set; }

        [NotMapped]
        public bool IsReservedForCurrentUser { get; set; }

        [NotMapped]
        public bool IsReservedForOtherUser { get; set; }

        [NotMapped]
        public int WaitingListCount { get; set; } = 0; // Total users in waiting list

        [NotMapped]
        public int UserWaitingPosition { get; set; } = 0; // User's position in the waiting list

        [NotMapped]
        public bool IsUserOnWaitingList { get; set; } = false; // Whether the user is in the waiting list

        [NotMapped]
        public string RemainingBorrowTime { get; set; } = string.Empty; // Dynamically calculated in the controller

        [NotMapped]
        public bool IsAlreadyBorrowed { get; set; } // Flag to check if user has already borrowed the book

        [NotMapped]
        public int UsersBeforeInWaitList { get; set; } // Users before this user in the waiting list

        [NotMapped]
        public int? BorrowTransactionId { get; set; }

    }
}

