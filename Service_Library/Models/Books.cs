using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Library.Models
{
    public class Book
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
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

        [NotMapped]
        public int WaitingListCount { get; set; } = 0; // Total users in waiting list

        [NotMapped]
        public int UserWaitingPosition { get; set; } = 0; // User's position in the waiting list

        [NotMapped]
        public bool IsUserOnWaitingList { get; set; } = false; // Whether the user is in the waiting list
        [NotMapped]
        public string RemainingBorrowTime { get; set; } // Dynamically calculated in the controller

        [NotMapped]
        public bool IsAlreadyBorrowed { get; set; } // Flag to check if user has already borrowed the book

        [NotMapped]
        public int UsersBeforeInWaitList { get; set; } // Users before this user in the waiting list


        //[NotMapped] // This won't be stored in the database
        //public string RemainingBorrowTime { get; set; }
        //[NotMapped] // Not stored in the database
        //public bool IsUserOnWaitingList { get; set; }

        //[NotMapped] // Not stored in the database
        //public int WaitingListCount { get; set; } // Number of users on the waiting list

    }
}
