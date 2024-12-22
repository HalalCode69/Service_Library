using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service_Library.Entities;
using Service_Library.Models;
using Service_Library.Services;
using System.Security.Claims;

namespace Service_Library.Controllers
{
    public class BooksController : Controller
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        public BooksController(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // Display all books with search and filter
        [Authorize]
        public IActionResult Index(string search, string categoryFilter, string formatFilter, string sort)
        {
            var books = _context.Books.AsQueryable();

            // Populate categories for the filter dropdown
            ViewBag.Categories = _context.Books
                .Select(b => b.Category)
                .Distinct()
                .OrderBy(c => c) // Optional: Sort categories alphabetically
                .ToList();

            // Apply search logic for title, author, and publisher
            if (!string.IsNullOrEmpty(search))
            {
                books = books.Where(b =>
                    b.Title.Contains(search) ||
                    b.Author.Contains(search) ||
                    b.Publisher.Contains(search));
            }

            // Apply category filter
            if (!string.IsNullOrEmpty(search))
            {
                books = books.Where(b =>
                    (b.Title.Contains(search) || b.Author.Contains(search) || b.Publisher.Contains(search)) &&
                    (string.IsNullOrEmpty(categoryFilter) || b.Category == categoryFilter));
            }
            else if (!string.IsNullOrEmpty(categoryFilter))
            {
                books = books.Where(b => b.Category == categoryFilter);
            }

            // Apply sorting logic
            if (sort == "price-asc")
            {
                books = books.OrderBy(b => b.BorrowPrice);
            }
            else if (sort == "price-desc")
            {
                books = books.OrderByDescending(b => b.BorrowPrice);
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
            foreach (var book in bookList)
            {
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
                if (book.ReservedForUserId.HasValue && book.ReservationExpiry > DateTime.Now)
                {
                    if (book.ReservedForUserId == userId)
                    {
                        // This user has the reservation
                        book.IsReservedForCurrentUser = true;
                    }
                    else
                    {
                        // Another user has the reservation
                        book.IsReservedForOtherUser = true;
                    }
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
                book.WaitingListCount = bookWaitList.Count;

                var userEntry = bookWaitList.FindIndex(w => w.UserId == userId);
                book.IsUserOnWaitingList = userEntry >= 0;

                if (book.IsUserOnWaitingList)
                {
                    book.UserWaitingPosition = userEntry + 1; // Position is index + 1
                }
                else
                {
                    book.UsersBeforeInWaitList = book.WaitingListCount;
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
        public IActionResult BorrowBook([FromBody] BorrowBookRequest request)
        {
            if (request == null || request.BookId <= 0)
            {
                return Json(new { success = false, message = "Invalid book ID." });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var book = _context.Books.FirstOrDefault(b => b.BookId == request.BookId);

            if (book == null)
            {
                return Json(new { success = false, message = "Book not found." });
            }

            try
            {
                // Check the number of active borrow transactions for this book
                var activeBorrowCount = _context.BorrowTransactions
                    .Count(bt => bt.BookId == request.BookId && !bt.IsReturned);

                if (activeBorrowCount >= 3)
                {
                    return Json(new { success = false, message = "This book has reached its borrow limit. Please join the waiting list." });
                }

                // Check if the book is reserved for another user
                if (book.ReservedForUserId.HasValue && book.ReservationExpiry > DateTime.Now)
                {
                    if (book.ReservedForUserId != userId)
                    {
                        return Json(new { success = false, message = "This book is reserved for another user." });
                    }
                }
                else if (book.ReservationExpiry != null && DateTime.Now > book.ReservationExpiry)
                {
                    book.ReservedForUserId = null;
                    book.ReservationExpiry = null;
                }

                // Check if the user has already borrowed 3 books
                var borrowedCount = _context.BorrowTransactions
                    .Count(bt => bt.UserId == userId && !bt.IsReturned);

                if (borrowedCount >= 3)
                {
                    return Json(new { success = false, message = "You can borrow up to 3 books at the same time." });
                }

                var existingTransaction = _context.BorrowTransactions
                    .FirstOrDefault(bt => bt.BookId == request.BookId && bt.UserId == userId && !bt.IsReturned);

                if (existingTransaction != null)
                {
                    return Json(new { success = false, message = "You have already borrowed this book." });
                }

                // Allow borrowing if the book is reserved for the current user or if there are available copies
                if (book.AvailableCopies > 0 || (book.ReservedForUserId == userId && book.ReservationExpiry > DateTime.Now))
                {
                    var borrowDate = DateTime.Now;
                    var returnDate = borrowDate.AddMinutes(1); // Replace with correct borrow duration logic.

                    var transaction = new BorrowTransaction
                    {
                        BookId = request.BookId,
                        UserId = userId,
                        BorrowDate = borrowDate,
                        ReturnDate = returnDate,
                        IsReturned = false
                    };

                    if (book.ReservedForUserId == userId)
                    {
                        book.ReservedForUserId = null;
                        book.ReservationExpiry = null;
                    }
                    else
                    {
                        book.AvailableCopies--;
                    }

                    _context.BorrowTransactions.Add(transaction);
                    _context.SaveChanges();

                    return Json(new
                    {
                        success = true,
                        message = "You have successfully borrowed the book!",
                        availableCopies = book.AvailableCopies,
                        returnTimestamp = returnDate.ToString("o"),
                        transactionId = transaction.TransactionId
                    });
                }

                return Json(new { success = false, message = "Book is currently unavailable." });
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                Console.WriteLine($"Error in BorrowBook: {ex.Message}");
                Console.WriteLine(ex.StackTrace);

                return Json(new { success = false, message = "An error occurred while processing your request." });
            }
        }





        public class BorrowBookRequest
        {
            public int BookId { get; set; }
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
            if (book.ReservedForUserId == userId)
            {
                book.ReservedForUserId = null;
                book.ReservationExpiry = null;

                // Check if there is another user in the waiting list
                var nextInLine = _context.WaitingList
                    .Where(w => w.BookId == request.BookId)
                    .OrderBy(w => w.AddedDate)
                    .FirstOrDefault();

                if (nextInLine != null && nextInLine.UserId != userId)
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
                    book.ReservedForUserId = nextInLine.UserId;
                    book.ReservationExpiry = DateTime.Now.AddHours(24);

                    // Remove the next user from the waiting list
                    _context.WaitingList.Remove(nextInLine);
                }
            }


            // Save changes immediately to ensure the state is updated
            _context.SaveChanges();

            // Fetch the updated waiting list count and reservation status
            var waitingListCount = _context.WaitingList.Count(w => w.BookId == request.BookId);
            var isReserved = book.ReservedForUserId != null;

            return Json(new
            {
                success = true,
                message = "You have successfully left the waiting list and released your reservation.",
                waitingListCount,
                isReserved
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
                    var nextInLine = _context.WaitingList
                        .Where(w => w.BookId == book.BookId)
                        .OrderBy(w => w.AddedDate)
                        .FirstOrDefault();

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
                        book.ReservedForUserId = nextInLine.UserId;
                        book.ReservationExpiry = DateTime.Now.AddHours(24);

                        // Remove the user from the waiting list after reserving
                        _context.WaitingList.Remove(nextInLine);

                        // Ensure the reserved book is not shown as available for borrowing
                        book.AvailableCopies--;
                    }
                    else
                    {
                        // If no waiting list, book becomes generally available
                        book.ReservedForUserId = null;
                        book.ReservationExpiry = null;
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
        [HttpPost]
        public IActionResult BuyBook([FromBody] BuyBookRequest request)
        {
            if (request == null || request.BookId <= 0)
            {
                return Json(new { success = false, message = "Invalid book ID." });
            }

            var book = _context.Books.FirstOrDefault(b => b.BookId == request.BookId);

            if (book == null)
            {
                return Json(new { success = false, message = "Book not found." });
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Check if user already owns the book
            var existingTransaction = _context.Transactions
                .FirstOrDefault(t => t.UserId == userId && t.BookId == request.BookId && t.TransactionType == "Buy");

            if (existingTransaction != null)
            {
                return Json(new { success = false, message = "You already own this book!" });
            }

            // Remove user from the waiting list (if they are on it)
            var waitingListEntry = _context.WaitingList
                .FirstOrDefault(w => w.BookId == request.BookId && w.UserId == userId);

            if (waitingListEntry != null)
            {
                _context.WaitingList.Remove(waitingListEntry);
            }

            // Handle active borrow transactions
            var borrowTransaction = _context.BorrowTransactions
                .FirstOrDefault(bt => bt.BookId == request.BookId && bt.UserId == userId && !bt.IsReturned);

            if (borrowTransaction != null)
            {
                borrowTransaction.IsReturned = true; // Mark as returned
            }

            // Create a transaction for buying
            var transaction = new Transaction
            {
                UserId = userId,
                BookId = request.BookId,
                TransactionType = "Buy",
                TransactionDate = DateTime.Now,
                Status = "Completed"
            };

            _context.Transactions.Add(transaction);
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                message = "Book purchased successfully!",
                bookId = request.BookId
            });
        }
        public class BuyBookRequest
        {
            public int BookId { get; set; }
        }

        [Authorize]
        public IActionResult Library(string search, string categoryFilter)
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
                .Where(b => borrowedBookIds.Contains(b.BookId) || purchasedBookIds.Contains(b.BookId));

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

            // Pass distinct categories to the view
            ViewBag.Categories = _context.Books
                .Where(b => borrowedBookIds.Contains(b.BookId) || purchasedBookIds.Contains(b.BookId))
                .Select(b => b.Category)
                .Distinct()
                .ToList();

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

    }
}
