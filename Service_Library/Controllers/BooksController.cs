using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service_Library.Entities;
using Service_Library.Models;
using System.Security.Claims;

namespace Service_Library.Controllers
{
    public class BooksController : Controller
    {
        private readonly AppDbContext _context;

        public BooksController(AppDbContext context)
        {
            _context = context;
        }

        // Display all books with search and filter
        [Authorize]
        public IActionResult Index(string search, string formatFilter, string sort)
        {
            var books = _context.Books.AsQueryable();

            // Apply search, filter, and sort logic
            if (!string.IsNullOrEmpty(search))
                books = books.Where(b => b.Title.Contains(search) || b.Author.Contains(search));

            if (!string.IsNullOrEmpty(formatFilter))
                books = books.Where(b => b.Format == formatFilter);

            if (sort == "price-asc")
                books = books.OrderBy(b => b.BorrowPrice);
            else if (sort == "price-desc")
                books = books.OrderByDescending(b => b.BorrowPrice);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            // Fetch borrowed books, purchased books, and waiting list for the current user
            var borrowedBooks = _context.BorrowTransactions.Where(bt => bt.UserId == userId && !bt.IsReturned).ToList();
            var purchasedBooks = _context.Transactions.Where(t => t.UserId == userId && t.TransactionType == "Buy").ToList();
            var waitingList = _context.WaitingList.ToList();

            var bookList = books.ToList();
            foreach (var book in bookList)
            {
                // Check if book is borrowed
                var borrowTransaction = borrowedBooks.FirstOrDefault(bt => bt.BookId == book.BookId);
                if (borrowTransaction != null)
                {
                    var remainingTime = borrowTransaction.ReturnDate - DateTime.Now;
                    book.IsAlreadyBorrowed = true;
                    book.RemainingBorrowTime = remainingTime.TotalSeconds > 0
                        ? $"{Math.Floor(remainingTime.TotalDays)} days, {remainingTime.Hours} hours left"
                        : "Overdue"; // Handle overdue case
                }

                // Check if book is owned by the user
                book.IsOwnedByCurrentUser = purchasedBooks.Any(t => t.BookId == book.BookId);

                // Calculate user's position in waiting list
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
        public IActionResult BorrowBook(int bookId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var book = _context.Books.FirstOrDefault(b => b.BookId == bookId);
            if (book == null) return NotFound();

            // Check if user has already borrowed this book and hasn't returned it
            var existingTransaction = _context.BorrowTransactions
                .FirstOrDefault(bt => bt.BookId == bookId && bt.UserId == userId && !bt.IsReturned);

            if (existingTransaction != null)
            {
                TempData["Message"] = "You have already borrowed this book.";
                return RedirectToAction("Index");
            }

            // Allow borrowing if copies are available
            if (book.AvailableCopies > 0)
            {
                var transaction = new BorrowTransaction
                {
                    BookId = bookId,
                    UserId = userId,
                    BorrowDate = DateTime.Now,
                    ReturnDate = DateTime.Now.AddDays(7), // Borrow period is 7 days
                    IsReturned = false
                };

                book.AvailableCopies--;
                _context.BorrowTransactions.Add(transaction);
                _context.SaveChanges();

                TempData["Message"] = "You have successfully borrowed the book!";
            }
            else
            {
                TempData["Message"] = "Book is currently unavailable.";
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        [Authorize]
        public IActionResult JoinWaitingList(int bookId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var book = _context.Books.FirstOrDefault(b => b.BookId == bookId);
            if (book == null) return NotFound();

            var alreadyWaiting = _context.WaitingList.Any(w => w.BookId == bookId && w.UserId == userId);

            if (!alreadyWaiting)
            {
                var waitingEntry = new WaitingList
                {
                    BookId = bookId,
                    UserId = userId,
                    AddedDate = DateTime.Now
                };

                _context.WaitingList.Add(waitingEntry);
                _context.SaveChanges();

                TempData["Message"] = "You have been added to the waiting list.";
            }
            else
            {
                TempData["Message"] = "You are already on the waiting list for this book.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize]
        public IActionResult LeaveWaitingList(int bookId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var entry = _context.WaitingList.FirstOrDefault(w => w.BookId == bookId && w.UserId == userId);

            if (entry != null)
            {
                _context.WaitingList.Remove(entry);
                _context.SaveChanges();
                TempData["Message"] = "You have left the waiting list.";
            }
            else
            {
                TempData["Message"] = "You are not on the waiting list for this book.";
            }

            return RedirectToAction("Index");
        }

        [Authorize]
        public IActionResult ReturnBook(int transactionId)
        {
            var transaction = _context.BorrowTransactions.FirstOrDefault(t => t.TransactionId == transactionId);

            if (transaction == null) return NotFound();

            // Mark as returned
            transaction.IsReturned = true;

            // Update book copies
            var book = _context.Books.FirstOrDefault(b => b.BookId == transaction.BookId);
            if (book != null)
            {
                book.AvailableCopies++;

                // Check if there is a waiting list for this book
                var nextInLine = _context.WaitingList.Where(w => w.BookId == book.BookId).OrderBy(w => w.AddedDate).FirstOrDefault();

                if (nextInLine != null)
                {
                    TempData["Message"] = $"Book '{book.Title}' is now available for user ID {nextInLine.UserId}.";

                    // Remove user from waiting list
                    _context.WaitingList.Remove(nextInLine);

                    // Reserve the book for the next user
                    book.AvailableCopies--;
                    var borrowTransaction = new BorrowTransaction
                    {
                        BookId = book.BookId,
                        UserId = nextInLine.UserId,
                        BorrowDate = DateTime.Now,
                        ReturnDate = DateTime.Now.AddDays(7),
                        IsReturned = false
                    };
                    _context.BorrowTransactions.Add(borrowTransaction);
                }

                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpPost]
        public IActionResult BuyBook(int id)
        {
            var book = _context.Books.FirstOrDefault(b => b.BookId == id);

            if (book == null)
            {
                TempData["Message"] = "Book not found.";
                return RedirectToAction("Index");
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Check if user already owns the book
            var existingTransaction = _context.Transactions
                .FirstOrDefault(t => t.UserId == userId && t.BookId == id && t.TransactionType == "Buy");

            if (existingTransaction != null)
            {
                TempData["Message"] = "You already own this book!";
                return RedirectToAction("Index");
            }

            // Remove user from the waiting list (if they are on it)
            var waitingListEntry = _context.WaitingList
                .FirstOrDefault(w => w.BookId == id && w.UserId == userId);

            if (waitingListEntry != null)
            {
                _context.WaitingList.Remove(waitingListEntry);
            }

            // Handle active borrow transactions
            var borrowTransaction = _context.BorrowTransactions
                .FirstOrDefault(bt => bt.BookId == id && bt.UserId == userId && !bt.IsReturned);

            if (borrowTransaction != null)
            {
                borrowTransaction.IsReturned = true; // Mark as returned
            }

            // Create a transaction for buying
            var transaction = new Transaction
            {
                UserId = userId,
                BookId = id,
                TransactionType = "Buy",
                TransactionDate = DateTime.Now,
                Status = "Completed"
            };

            _context.Transactions.Add(transaction);
            _context.SaveChanges();

            TempData["Message"] = "Book purchased successfully!";
            return RedirectToAction("Index");
        }

    }
}
