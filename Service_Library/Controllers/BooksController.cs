using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Library.Entities;
using Service_Library.Models;
using Service_Library.Services;
using System.Diagnostics;
using System.Security.Claims;

namespace Service_Library.Controllers
{
    public class BooksController : Controller
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        private readonly ILogger<BooksController> _logger;

        public BooksController(AppDbContext context, EmailService emailService, ILogger<BooksController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public IActionResult Index(string message, string search, string[] categoryFilter, string genreFilter, string formatFilter, string sort, bool? onSale, string availability, string authorFilter, decimal? priceRangeMin, decimal? priceRangeMax)
        {
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.Message = message;
            }
            var booksQuery = _context.Books.Include(b => b.Genres).AsQueryable();

            // Calculate overall min and max prices
            decimal overallMinPrice = 0;
            decimal overallMaxPrice = 0;

            var allBooks = _context.Books.ToList();
            if (allBooks.Any())
            {
                overallMinPrice = allBooks.Min(b => Math.Min(b.BuyPrice, b.BorrowPrice));
                overallMaxPrice = allBooks.Max(b => Math.Max(b.BuyPrice, b.BorrowPrice));
            }

            ViewBag.MinPrice = overallMinPrice;
            ViewBag.MaxPrice = overallMaxPrice;

            // Populate categories for the filter dropdown
            ViewBag.Categories = _context.Books
                .Select(b => b.Category)
                .Distinct()
                .OrderBy(c => c) // Optional: Sort categories alphabetically
                .ToList();

            // Populate genres for the filter dropdown
            ViewBag.Genres = _context.Genres
                .Select(g => g.Name)
                .Distinct()
                .OrderBy(g => g) // Optional: Sort genres alphabetically
                .ToList();

            // Populate authors for the filter dropdown
            ViewBag.Authors = _context.Books
                .Select(b => b.Author)
                .Distinct()
                .OrderBy(a => a) // Optional: Sort authors alphabetically
                .ToList();

            // Apply search logic for title, author, and publisher
            if (!string.IsNullOrEmpty(search))
            {
                booksQuery = booksQuery.Where(b =>
                    b.Title.Contains(search) ||
                    b.Author.Contains(search) ||
                    b.Publisher.Contains(search));
            }

            // Apply genre filter
            if (!string.IsNullOrEmpty(genreFilter))
            {
                var genres = genreFilter.Split(',');
                booksQuery = booksQuery.Where(b => b.Genres.Any(g => genres.Contains(g.Name)));
            }

            // Apply format filter
            if (!string.IsNullOrEmpty(formatFilter))
            {
                booksQuery = booksQuery.Where(b => b.Format == formatFilter);
            }

            // Apply on sale filter
            if (onSale.HasValue && onSale.Value)
            {
                booksQuery = booksQuery.Where(b => b.DiscountPrice.HasValue && b.DiscountEndDate > DateTime.Now);
            }

            // Apply availability filter
            if (!string.IsNullOrEmpty(availability))
            {
                if (availability == "borrow")
                {
                    booksQuery = booksQuery.Where(b => b.IsBorrowable);
                }
                else if (availability == "buy")
                {
                    booksQuery = booksQuery.Where(b => !b.IsBorrowable);
                }
            }

            // Apply author filter
            if (!string.IsNullOrEmpty(authorFilter))
            {
                booksQuery = booksQuery.Where(b => b.Author == authorFilter);
            }

            // Apply price range filter
            if (priceRangeMin.HasValue || priceRangeMax.HasValue)
            {
                decimal minFilterPrice = priceRangeMin ?? 0;
                decimal maxFilterPrice = priceRangeMax ?? decimal.MaxValue;

                booksQuery = booksQuery.Where(b =>
                    (b.BuyPrice >= minFilterPrice && b.BuyPrice <= maxFilterPrice) ||
                    (b.BorrowPrice >= minFilterPrice && b.BorrowPrice <= maxFilterPrice));
            }

            // Fetch the books from the database
            var books = booksQuery.ToList();

            // Apply sorting logic
            switch (sort)
            {
                case "price-asc":
                    books = books.OrderBy(b => b.DiscountPrice.HasValue && b.DiscountEndDate > DateTime.Now ? b.DiscountPrice.Value : b.BuyPrice).ToList();
                    break;
                case "price-desc":
                    books = books.OrderByDescending(b => b.DiscountPrice.HasValue && b.DiscountEndDate > DateTime.Now ? b.DiscountPrice.Value : b.BuyPrice).ToList();
                    break;
                case "most-popular":
                    books = books.OrderByDescending(b => !b.IsBorrowable).ThenByDescending(b => b.RatingCount).ToList();
                    break;
                case "genre":
                    books = books.OrderBy(b => b.Category).ToList();
                    break;
                case "year":
                    books = books.OrderByDescending(b => b.YearOfPublishing).ToList();
                    break;
                default:
                    books = books.OrderBy(b => b.Title).ToList(); // Default sorting by title
                    break;
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            // Fetch borrowed books, purchased books, and waiting list for the current user
            var borrowedBooks = _context.BorrowTransactions
                .Where(bt => bt.UserId == userId && !bt.IsReturned)
                .ToList();
            var purchasedBooks = _context.Transactions
                .Where(t => t.UserId == userId && t.TransactionType == "Buy")
                .ToList();
            var waitingList = _context.WaitingList.ToList();

            var bookList = books.ToList();
            ViewBag.CurrentUserId = userId;

            foreach (var book in bookList)
            {
                book.Feedbacks = _context.Feedbacks
                    .Where(f => f.BookId == book.BookId)
                    .OrderByDescending(f => f.Date)
                    .Select(f => new Feedback
                    {
                        FeedbackId = f.FeedbackId,
                        BookId = f.BookId,
                        UserId = f.UserId,
                        Rating = f.Rating,
                        Comment = f.Comment,
                        Date = f.Date,
                        FirstName = _context.UserAccounts
                            .Where(u => u.Id.ToString() == f.UserId)
                            .Select(u => u.FirstName)
                            .FirstOrDefault() ?? "Anonymous",
                        LastName = _context.UserAccounts
                            .Where(u => u.Id.ToString() == f.UserId)
                            .Select(u => u.LastName)
                            .FirstOrDefault() ?? ""
                    })
                    .ToList();

                // Check active borrow count
                book.ActiveBorrowCount = _context.BorrowTransactions
                    .Count(bt => bt.BookId == book.BookId && !bt.IsReturned);

                // Check if the book is borrowed
                var borrowTransaction = borrowedBooks.FirstOrDefault(bt => bt.BookId == book.BookId);
                if (borrowTransaction != null)
                {
                    var remainingTime = borrowTransaction.ReturnDate - DateTime.Now;
                    book.IsAlreadyBorrowed = true;
                    book.RemainingBorrowTime = remainingTime.TotalSeconds > 0
                        ? $"{Math.Floor(remainingTime.TotalDays)} days, {remainingTime.Hours} hours, {remainingTime.Minutes} mins left"
                        : "Overdue"; // Handle overdue case
                    book.BorrowTransactionId = borrowTransaction.TransactionId; // Include BorrowTransactionId
                }

                // Check if the book is reserved
                var reservations = _context.BookReservations
                    .Where(br => br.BookId == book.BookId && br.ReservationExpiry > DateTime.Now)
                    .ToList();

                if (reservations.Any(r => r.UserId == userId))
                {
                    // This user has a reservation
                    book.IsReservedForCurrentUser = true;
                }
                else if (reservations.Any())
                {
                    // Another user has a reservation
                    book.IsReservedForOtherUser = true;
                }
                else
                {
                    // If no reservation, ensure these flags are reset
                    book.IsReservedForCurrentUser = false;
                    book.IsReservedForOtherUser = false;
                }
                // Check if the book is owned by the user
                book.IsOwnedByCurrentUser = purchasedBooks.Any(t => t.BookId == book.BookId);

                // Calculate user's position in the waiting list
                var bookWaitList = waitingList.Where(w => w.BookId == book.BookId).OrderBy(w => w.AddedDate).ToList();
                book.WaitingListCount = bookWaitList.Count; // Use only the waiting list count

                var userEntry = bookWaitList.FindIndex(w => w.UserId == userId);
                book.IsUserOnWaitingList = userEntry >= 0;

                if (book.IsUserOnWaitingList)
                {
                    book.UserWaitingPosition = userEntry + 1; // Position is index + 1
                }
                else
                {
                    book.UsersBeforeInWaitList = book.WaitingListCount + 1; // Include the current user
                }

                // Fetch the user's rating for the book
                var userRating = _context.UserRatings
                    .Where(r => r.BookId == book.BookId && r.UserId == userId)
                    .Select(r => (int?)r.Rating)
                    .FirstOrDefault();
                book.UserRating = userRating;

                // Fetch the user's comment for the book
                var userFeedback = _context.Feedbacks
                    .FirstOrDefault(f => f.BookId == book.BookId && f.UserId == userId.ToString());
                book.UserComment = userFeedback?.Comment ?? string.Empty; // Precompute the user's comment

                // Calculate estimated availability
                var borrowedCopies = _context.BorrowTransactions
                    .Where(bt => bt.BookId == book.BookId && !bt.IsReturned)
                    .OrderBy(bt => bt.ReturnDate)
                    .ToList();

                if (book.IsUserOnWaitingList)
                {
                    if (book.UserWaitingPosition <= 3)
                    {
                        // User is within the first 3 positions
                        var closestReturnDate = borrowedCopies.FirstOrDefault()?.ReturnDate ?? DateTime.Now;
                        book.EstimatedAvailabilityInDays = (closestReturnDate - DateTime.Now).Days;
                    }
                    else if (book.UserWaitingPosition <= 6)
                    {
                        // User is within the 4th to 6th positions
                        var secondClosestReturnDate = borrowedCopies.Skip(1).FirstOrDefault()?.ReturnDate ?? DateTime.Now;
                        book.EstimatedAvailabilityInDays = (secondClosestReturnDate - DateTime.Now).Days;
                    }
                    else
                    {
                        // User is in the 7th position or beyond
                        var thirdClosestReturnDate = borrowedCopies.Skip(2).FirstOrDefault()?.ReturnDate ?? DateTime.Now;
                        book.EstimatedAvailabilityInDays = (thirdClosestReturnDate - DateTime.Now).Days;
                    }
                }
                else
                {
                    // If the user is not on the waiting list, calculate the estimated availability as if they were joining
                    var potentialPosition = book.WaitingListCount + 1;
                    if (potentialPosition <= 3)
                    {
                        // User would be within the first 3 positions
                        var closestReturnDate = borrowedCopies.FirstOrDefault()?.ReturnDate ?? DateTime.Now;
                        book.EstimatedAvailabilityInDays = (closestReturnDate - DateTime.Now).Days;
                    }
                    else if (potentialPosition <= 6)
                    {
                        // User would be within the 4th to 6th positions
                        var secondClosestReturnDate = borrowedCopies.Skip(1).FirstOrDefault()?.ReturnDate ?? DateTime.Now;
                        book.EstimatedAvailabilityInDays = (secondClosestReturnDate - DateTime.Now).Days;
                    }
                    else
                    {
                        // User would be in the 7th position or beyond
                        var thirdClosestReturnDate = borrowedCopies.Skip(2).FirstOrDefault()?.ReturnDate ?? DateTime.Now;
                        book.EstimatedAvailabilityInDays = (thirdClosestReturnDate - DateTime.Now).Days;
                    }
                }
            }

            return View(bookList);
        }






        // View book details
        public IActionResult Details(int id)
        {
            var book = _context.Books.FirstOrDefault(b => b.BookId == id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        [HttpPost]
        [Authorize]
        public IActionResult JoinWaitingList([FromBody] JoinWaitingListRequest request)
        {
            if (request == null || request.BookId <= 0)
            {
                return Json(new { success = false, message = "Invalid book ID." });
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var book = _context.Books.FirstOrDefault(b => b.BookId == request.BookId);
            if (book == null)
            {
                return Json(new { success = false, message = "Book not found." });
            }

            // Check if the user is already in the waiting list
            var existingEntry = _context.WaitingList.FirstOrDefault(w => w.BookId == request.BookId && w.UserId == userId);
            if (existingEntry != null)
            {
                var waitingList = _context.WaitingList.Where(w => w.BookId == request.BookId).OrderBy(w => w.AddedDate).ToList();
                var position = waitingList.FindIndex(w => w.UserId == userId) + 1;

                return Json(new
                {
                    success = true,
                    message = $"You are already in the waiting list for this book.",
                    userPosition = position,
                    waitingListCount = waitingList.Count
                });
            }

            // Add the user to the waiting list
            var waitingEntry = new WaitingList
            {
                BookId = request.BookId,
                UserId = userId,
                AddedDate = DateTime.Now
            };

            _context.WaitingList.Add(waitingEntry);
            _context.SaveChanges();

            // Refresh the waiting list
            var updatedList = _context.WaitingList.Where(w => w.BookId == request.BookId).OrderBy(w => w.AddedDate).ToList();
            var newPosition = updatedList.FindIndex(w => w.UserId == userId) + 1;

            return Json(new
            {
                success = true,
                message = "You have been added to the waiting list.",
                userPosition = newPosition,
                waitingListCount = updatedList.Count
            });
        }






        public class JoinWaitingListRequest
        {
            public int BookId { get; set; }
        }

        [HttpPost]
        [Authorize]
        public IActionResult SubmitWebsiteRating([FromBody] UserRating request)
        {
            if (request == null || request.Rating < 1 || request.Rating > 5)
            {
                return Json(new { success = false, message = "Invalid rating data." });
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var user = _context.UserAccounts.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            var existingRating = _context.UserRatings.FirstOrDefault(r => r.UserId == userId && r.IsWebsiteRating);

            if (existingRating != null)
            {
                // Update existing rating
                existingRating.Rating = request.Rating;
                existingRating.Comment = request.Comment ?? string.Empty;
                existingRating.Date = DateTime.Now; // Update the date
            }
            else
            {
                // Create new rating
                var rating = new UserRating
                {
                    UserId = userId,
                    Rating = request.Rating,
                    Comment = request.Comment ?? string.Empty,
                    Date = DateTime.Now, // Set the date
                    IsWebsiteRating = true
                };

                _context.UserRatings.Add(rating);
            }

            _context.SaveChanges();

            return Json(new { success = true, message = "Rating submitted successfully." });
        }


        [HttpGet]
        public IActionResult GetWebsiteRatings()
        {
            var ratings = _context.UserRatings
                .Where(r => r.IsWebsiteRating)
                .Select(r => new
                {
                    r.Rating,
                    r.Comment,
                    UserName = _context.UserAccounts
                        .Where(u => u.Id == r.UserId)
                        .Select(u => u.FirstName + " " + u.LastName)
                        .FirstOrDefault(),
                    r.Date // Include the date
                })
                .ToList();

            return Json(ratings);
        }






        [HttpPost]
        [Authorize]
        public IActionResult LeaveWaitingList([FromBody] LeaveWaitingListRequest request)
        {
            if (request == null || request.BookId <= 0)
            {
                return Json(new { success = false, message = "Invalid book ID." });
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            // Find the book
            var book = _context.Books.FirstOrDefault(b => b.BookId == request.BookId);
            if (book == null)
            {
                return Json(new { success = false, message = "Book not found." });
            }

            // Remove the current user from the waiting list
            var waitingEntry = _context.WaitingList.FirstOrDefault(w => w.BookId == request.BookId && w.UserId == userId);
            if (waitingEntry != null)
            {
                _context.WaitingList.Remove(waitingEntry);
            }

            // If the user had a reservation, release it
            var userReservation = _context.BookReservations.FirstOrDefault(br => br.BookId == book.BookId && br.UserId == userId);
            if (userReservation != null)
            {
                _context.BookReservations.Remove(userReservation);

                // Check if there is another user in the waiting list who is not already in the BookReservations table
                var nextInLine = _context.WaitingList
                    .Where(w => w.BookId == request.BookId)
                    .OrderBy(w => w.AddedDate)
                    .FirstOrDefault(w => !_context.BookReservations.Any(br => br.BookId == request.BookId && br.UserId == w.UserId));

                if (nextInLine != null)
                {
                    // Notify the next user
                    var user = _context.UserAccounts.FirstOrDefault(u => u.Id == nextInLine.UserId);
                    if (user != null)
                    {
                        string subject = "Book Available for Borrowing";
                        string body = $@"
                <p>Dear {user.FirstName},</p>
                <p>The book <strong>{book.Title}</strong> is now reserved for you.</p>
                <p>Please borrow it within the next 24 hours.</p>
                <p>Thank you!</p>";

                        _emailService.SendEmailAsync(user.Email, subject, body).Wait();
                    }

                    // Reserve the book for the next user
                    var reservation = new BookReservation
                    {
                        BookId = book.BookId,
                        UserId = nextInLine.UserId,
                        ReservationExpiry = DateTime.Now.AddHours(24)
                    };
                    _context.BookReservations.Add(reservation);

                    // Remove the next user from the waiting list
                    _context.WaitingList.Remove(nextInLine);
                }
            }

            // Save changes immediately to ensure the state is updated
            _context.SaveChanges();

            // Fetch the updated waiting list count
            var waitingListCount = _context.WaitingList.Count(w => w.BookId == request.BookId);

            return Json(new
            {
                success = true,
                message = "You have successfully left the waiting list and released your reservation.",
                waitingListCount
            });
        }

        public class LeaveWaitingListRequest
        {
            public int BookId { get; set; }
        }

        [HttpPost]
        [Authorize]
        public IActionResult ReturnBook([FromBody] ReturnBookRequest request)
        {
            if (request == null || request.TransactionId <= 0)
            {
                return Json(new { success = false, message = "Invalid transaction ID." });
            }

            var transaction = _context.BorrowTransactions.FirstOrDefault(t => t.TransactionId == request.TransactionId);

            if (transaction == null || transaction.IsReturned)
            {
                return Json(new { success = false, message = "Invalid transaction or the book has already been returned." });
            }

            try
            {
                // Debug logging
                Console.WriteLine($"Processing ReturnBook for TransactionId: {transaction.TransactionId}, BookId: {transaction.BookId}");

                // Mark the transaction as returned
                transaction.IsReturned = true;

                // Update book copies
                var book = _context.Books.FirstOrDefault(b => b.BookId == transaction.BookId);
                if (book != null)
                {
                    book.AvailableCopies++;

                    // Check if there is a waiting list for this book
                    var nextInLineUsers = _context.WaitingList
                        .Where(w => w.BookId == book.BookId)
                        .OrderBy(w => w.AddedDate)
                        .Take(3) // Get the first three users in the waiting list
                        .ToList();

                    if (nextInLineUsers.Any())
                    {
                        foreach (var nextInLine in nextInLineUsers)
                        {
                            // Notify the next user
                            var user = _context.UserAccounts.FirstOrDefault(u => u.Id == nextInLine.UserId);
                            if (user != null)
                            {
                                string subject = "Book Available for Borrowing";
                                string body = $@"
                        <p>Dear {user.FirstName},</p>
                        <p>The book <strong>{book.Title}</strong> is now reserved for you.</p>
                        <p>Please borrow it within the next 24 hours.</p>
                        <p>Thank you!</p>";

                                _emailService.SendEmailAsync(user.Email, subject, body).Wait();
                            }

                            // Reserve the book for the next user
                            var reservation = new BookReservation
                            {
                                BookId = book.BookId,
                                UserId = nextInLine.UserId,
                                ReservationExpiry = DateTime.Now.AddHours(24)
                            };
                            _context.BookReservations.Add(reservation);
                        }
                    }
                }

                _context.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = $"Book '{book?.Title}' has been successfully returned.",
                    bookId = book?.BookId
                });
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                Console.WriteLine($"Error in ReturnBook: {ex.Message}");
                Console.WriteLine(ex.StackTrace);

                return Json(new { success = false, message = "An error occurred while processing your request." });
            }
        }

        public class ReturnBookRequest
        {
            public int TransactionId { get; set; }
        }
        [Authorize]
        public IActionResult Library(string search, string categoryFilter, string genreFilter, string sort, string authorFilter, decimal? priceRangeMin, decimal? priceRangeMax, bool? onSale, string availability)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            // Get all books in the library
            var borrowedBookIds = _context.BorrowTransactions
                .Where(bt => bt.UserId == userId && !bt.IsReturned) // Exclude returned books
                .Select(bt => bt.BookId)
                .ToList();

            var purchasedBookIds = _context.Transactions
                .Where(t => t.UserId == userId && t.TransactionType == "Buy")
                .Select(t => t.BookId)
                .ToList();

            var booksQuery = _context.Books
                .Include(b => b.Genres) // Include genres
                .Where(b => borrowedBookIds.Contains(b.BookId) || purchasedBookIds.Contains(b.BookId));

            // Calculate overall min and max prices
            decimal overallMinPrice = 0;
            decimal overallMaxPrice = 0;

            var allBooks = _context.Books.ToList();
            if (allBooks.Any())
            {
                overallMinPrice = allBooks.Min(b => Math.Min(b.BuyPrice, b.BorrowPrice));
                overallMaxPrice = allBooks.Max(b => Math.Max(b.BuyPrice, b.BorrowPrice));
            }

            ViewBag.MinPrice = overallMinPrice;
            ViewBag.MaxPrice = overallMaxPrice;

            // Populate categories for the filter dropdown
            ViewBag.Categories = _context.Books
                .Where(b => borrowedBookIds.Contains(b.BookId) || purchasedBookIds.Contains(b.BookId))
                .Select(b => b.Category)
                .Distinct()
                .OrderBy(c => c) // Optional: Sort categories alphabetically
                .ToList();

            // Populate genres for the filter dropdown
            ViewBag.Genres = _context.Genres
                .Select(g => g.Name)
                .Distinct()
                .OrderBy(g => g) // Optional: Sort genres alphabetically
                .ToList();

            // Populate authors for the filter dropdown
            ViewBag.Authors = _context.Books
                .Where(b => borrowedBookIds.Contains(b.BookId) || purchasedBookIds.Contains(b.BookId))
                .Select(b => b.Author)
                .Distinct()
                .OrderBy(a => a) // Optional: Sort authors alphabetically
                .ToList();

            // Filter by search
            if (!string.IsNullOrEmpty(search))
            {
                booksQuery = booksQuery.Where(b => b.Title.Contains(search) || b.Author.Contains(search) || b.Publisher.Contains(search));
            }

            // Filter by category
            if (!string.IsNullOrEmpty(categoryFilter))
            {
                booksQuery = booksQuery.Where(b => b.Category == categoryFilter);
            }

            // Apply genre filter
            if (!string.IsNullOrEmpty(genreFilter))
            {
                var genres = genreFilter.Split(',');
                booksQuery = booksQuery.Where(b => b.Genres.Any(g => genres.Contains(g.Name)));
            }

            // Filter by author
            if (!string.IsNullOrEmpty(authorFilter))
            {
                booksQuery = booksQuery.Where(b => b.Author == authorFilter);
            }

            // Apply price range filter
            if (priceRangeMin.HasValue || priceRangeMax.HasValue)
            {
                decimal minFilterPrice = priceRangeMin ?? 0;
                decimal maxFilterPrice = priceRangeMax ?? decimal.MaxValue;

                booksQuery = booksQuery.Where(b =>
                    (b.BuyPrice >= minFilterPrice && b.BuyPrice <= maxFilterPrice) ||
                    (b.BorrowPrice >= minFilterPrice && b.BorrowPrice <= maxFilterPrice));
            }

            // Apply on sale filter
            if (onSale.HasValue && onSale.Value)
            {
                booksQuery = booksQuery.Where(b => b.DiscountPrice.HasValue && b.DiscountEndDate > DateTime.Now);
            }

            // Apply availability filter
            if (!string.IsNullOrEmpty(availability))
            {
                if (availability == "borrow")
                {
                    booksQuery = booksQuery.Where(b => b.IsBorrowable);
                }
                else if (availability == "buy")
                {
                    booksQuery = booksQuery.Where(b => !b.IsBorrowable);
                }
            }

            // Apply sorting logic
            switch (sort)
            {
                case "price-asc":
                    booksQuery = booksQuery.OrderBy(b => b.BuyPrice);
                    break;
                case "price-desc":
                    booksQuery = booksQuery.OrderByDescending(b => b.BuyPrice);
                    break;
                case "most-popular":
                    booksQuery = booksQuery.OrderByDescending(b => !b.IsBorrowable).ThenByDescending(b => b.RatingCount);
                    break;
                case "genre":
                    booksQuery = booksQuery.OrderBy(b => b.Category);
                    break;
                case "year":
                    booksQuery = booksQuery.OrderByDescending(b => b.YearOfPublishing);
                    break;
                default:
                    booksQuery = booksQuery.OrderBy(b => b.Title); // Default sorting by title
                    break;
            }

            var libraryBooks = booksQuery.ToList();

            // Populate transaction-related properties for borrowed books
            foreach (var book in libraryBooks)
            {
                var borrowTransaction = _context.BorrowTransactions
                    .FirstOrDefault(bt => bt.BookId == book.BookId && bt.UserId == userId && !bt.IsReturned);

                if (borrowTransaction != null)
                {
                    var remainingTime = borrowTransaction.ReturnDate - DateTime.Now;
                    book.RemainingBorrowTime = remainingTime.TotalSeconds > 0
                        ? $"{Math.Floor(remainingTime.TotalDays)} days, {remainingTime.Hours} hours, {remainingTime.Minutes} mins left"
                        : "Overdue";

                    book.BorrowTransactionId = borrowTransaction.TransactionId; // Include BorrowTransactionId
                }

                // Populate feedbacks for each book
                book.Feedbacks = _context.Feedbacks
                    .Where(f => f.BookId == book.BookId)
                    .OrderByDescending(f => f.Date)
                    .Select(f => new Feedback
                    {
                        FeedbackId = f.FeedbackId,
                        BookId = f.BookId,
                        UserId = f.UserId,
                        Rating = f.Rating,
                        Comment = f.Comment,
                        Date = f.Date,
                        FirstName = _context.UserAccounts
                            .Where(u => u.Id.ToString() == f.UserId)
                            .Select(u => u.FirstName)
                            .FirstOrDefault() ?? "Anonymous",
                        LastName = _context.UserAccounts
                            .Where(u => u.Id.ToString() == f.UserId)
                            .Select(u => u.LastName)
                            .FirstOrDefault() ?? ""
                    })
                    .ToList();
            }

            return View(libraryBooks);
        }

        [HttpPost]
        [Authorize]
        public IActionResult DeleteFromLibrary([FromBody] int bookId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            try
            {
                // Remove book from purchased list
                var purchaseTransaction = _context.Transactions
                    .FirstOrDefault(t => t.BookId == bookId && t.UserId == userId && t.TransactionType == "Buy");

                if (purchaseTransaction != null)
                {
                    _context.Transactions.Remove(purchaseTransaction);
                }

                // Remove book from borrow list if applicable
                var borrowTransaction = _context.BorrowTransactions
                    .FirstOrDefault(bt => bt.BookId == bookId && bt.UserId == userId && !bt.IsReturned);

                if (borrowTransaction != null)
                {
                    borrowTransaction.IsReturned = true;
                }

                _context.SaveChanges();

                return Json(new { success = true, message = "Book successfully deleted from your library." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting book: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while deleting the book." });
            }
        }
        [HttpPost]
        [Authorize]
        public IActionResult SubmitFeedback([FromBody] FeedbackRequest request)
        {
            // Basic validations
            if (request == null || request.BookId <= 0)
            {
                return Json(new { success = false, message = "Invalid book ID." });
            }

            if (request.Rating < 1 || request.Rating > 5)
            {
                return Json(new { success = false, message = "Invalid rating. Must be between 1 and 5." });
            }

            // Get the currently logged-in user's ID
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            // Check if the book exists
            var book = _context.Books.FirstOrDefault(b => b.BookId == request.BookId);
            if (book == null)
            {
                return Json(new { success = false, message = "Book not found." });
            }

            // Check if the user has borrowed or purchased the book
            bool hasBorrowed = _context.BorrowTransactions.Any(bt => bt.BookId == request.BookId && bt.UserId == userId && !bt.IsReturned);
            bool hasPurchased = _context.Transactions.Any(t => t.BookId == request.BookId && t.UserId == userId && t.TransactionType == "Buy");

            if (!hasBorrowed && !hasPurchased)
            {
                return Json(new { success = false, message = "You must borrow or buy the book before leaving feedback." });
            }

            try
            {
                // Check if feedback already exists for this user & book
                var existingFeedback = _context.Feedbacks
                    .FirstOrDefault(f => f.BookId == request.BookId && f.UserId == userId.ToString());

                if (existingFeedback != null)
                {
                    // Update existing feedback
                    existingFeedback.Rating = request.Rating;
                    existingFeedback.Comment = request.Comment ?? string.Empty;
                    existingFeedback.Date = DateTime.Now;
                }
                else
                {
                    // Create new feedback
                    var newFeedback = new Feedback
                    {
                        BookId = request.BookId,
                        UserId = userId.ToString(),
                        Rating = request.Rating,
                        Comment = request.Comment ?? string.Empty,
                        Date = DateTime.Now,
                    };
                    _context.Feedbacks.Add(newFeedback);
                }

                // Save changes to ensure feedback is stored before calculating the average rating
                _context.SaveChanges();

                // Update the average rating and rating count
                var allFeedbacksForBook = _context.Feedbacks
                    .Where(f => f.BookId == request.BookId)
                    .ToList();

                double newAvg = 0;
                int newCount = allFeedbacksForBook.Count;

                if (newCount > 0)
                {
                    newAvg = allFeedbacksForBook.Average(f => f.Rating);
                }

                // Update the Book entity in memory
                book.RatingCount = newCount;
                book.AverageRating = newAvg;

                // Save changes to update the book entity
                _context.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = "Feedback submitted successfully.",
                    ratingCount = book.RatingCount,
                    averageRating = book.AverageRating
                });
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                _logger.LogError(ex, "An error occurred while submitting feedback.");
                return Json(new { success = false, message = "An error occurred while submitting your feedback." });
            }
        }

        [HttpPost]
        public IActionResult DownloadBook(int bookId, string targetFormat)
        {
            var book = _context.Books.FirstOrDefault(b => b.BookId == bookId);
            if (book == null || book.BookContent == null)
            {
                return NotFound("Book or its content not found.");
            }

            // Validate target format
            if (string.IsNullOrEmpty(targetFormat))
            {
                return BadRequest("Target format is required.");
            }

            targetFormat = targetFormat.ToUpper();
            var validFormats = new[] { "PDF", "EPUB", "MOBI", "FB2" };
            if (!validFormats.Contains(targetFormat))
            {
                return BadRequest("Invalid format requested.");
            }

            // Define the original content and format
            var originalFormat = book.Format.ToUpper();
            var originalContent = book.BookContent; // Stored book content as a byte array

            // Convert if necessary
            byte[] convertedContent;
            if (originalFormat == targetFormat)
            {
                convertedContent = originalContent;
            }
            else
            {
                try
                {
                    convertedContent = ConvertBookFormat(originalContent, originalFormat, targetFormat);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Error during conversion: {ex.Message}");
                }
            }

            // Set the content type for the download
            string contentType = targetFormat switch
            {
                "PDF" => "application/pdf",
                "EPUB" => "application/epub+zip",
                "MOBI" => "application/x-mobipocket-ebook",
                "FB2" => "application/xml",
                _ => "application/octet-stream"
            };

            var fileName = $"{book.Title}.{targetFormat.ToLower()}";
            Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");

            return File(convertedContent, contentType, fileName);
        }

        // Conversion helper method
        public byte[] ConvertBookFormat(byte[] originalContent, string originalFormat, string targetFormat)
        {
            using (var stream = new MemoryStream(originalContent))
            {
                var document = new Aspose.Words.Document(stream);

                using (var outputStream = new MemoryStream())
                {
                    var saveFormat = targetFormat.ToUpper() switch
                    {
                        "PDF" => Aspose.Words.SaveFormat.Pdf,
                        "EPUB" => Aspose.Words.SaveFormat.Epub,
                        "MOBI" => Aspose.Words.SaveFormat.Mhtml, // Save MHTML as MOBI manually
                        _ => throw new NotSupportedException("Unsupported target format.")
                    };

                    document.Save(outputStream, saveFormat);
                    return outputStream.ToArray();
                }
            }
        }

        [HttpGet]
        public IActionResult GetSuggestions(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return Json(new List<object>());
            }

            var suggestions = _context.Books
                .Where(b => b.Title.Contains(query) || b.Category.Contains(query))
                .Select(b => new { b.BookId, b.Title })
                .Take(5) // Limit to 5 suggestions
                .ToList();

            return Json(suggestions);
        }
        [HttpGet]
        public IActionResult RedirectToPayPal(int bookId, string bookTitle, decimal bookPrice)
        {
            // Construct the PayPal URL
            var paypalUrl = $"https://www.paypal.com/cgi-bin/webscr?cmd=_xclick&business=your-paypal-email@example.com&item_name={bookTitle}&amount={bookPrice}&currency_code=USD&return=https://yourwebsite.com/Books/CompletePurchase?bookId={bookId}&cancel_return=https://yourwebsite.com/Books/CancelPurchase";

            return Redirect(paypalUrl);
        }

        [HttpGet]
        public IActionResult CompletePurchase(int bookId)
        {
            // Update the database to mark the book as purchased
            var book = _context.Books.Find(bookId);
            if (book != null)
            {
                book.IsOwnedByCurrentUser = true;
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult CancelPurchase()
        {
            // Handle purchase cancellation
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> GetBookCover(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null || book.BookContent == null)
            {
                return NotFound();
            }

            return File(book.BookContent, "image/jpeg"); // Adjust the content type as needed
        }

    }
}
