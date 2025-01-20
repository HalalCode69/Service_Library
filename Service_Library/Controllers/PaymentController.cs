using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Library.Services;
using Service_Library.Models;
using Service_Library.Entities;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Service_Library.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : Controller
    {
        private readonly PayPalService _payPalService;
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        private readonly ILogger<PaymentController> _logger; 

        public PaymentController(PayPalService payPalService, AppDbContext context, ILogger<PaymentController> logger, EmailService emailService)
        {
            _payPalService = payPalService;
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }
        [HttpPost("BuyOrBorrowItem")]
        public async Task<IActionResult> BuyOrBorrowItem([FromBody] BuyOrBorrowRequest request)
        {
            _logger.LogInformation("BuyOrBorrowItem called with bookId: {BookId}, itemType: {ItemType}, title: {Title}, price: {Price}", request.BookId, request.ItemType, request.Title, request.Price);

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized();
            }

            var book = await _context.Books.FindAsync(request.BookId);
            if (book == null)
            {
                return NotFound();
            }

            if (request.ItemType == "Borrow")
            {
                var borrowedCount = _context.BorrowTransactions
                    .Count(bt => bt.UserId == userId && !bt.IsReturned);

                if (borrowedCount >= 3)
                {
                    return BadRequest("You can borrow up to 3 books at the same time.");
                }
            }

            decimal priceToUse;
            if (request.ItemType == "Borrow")
            {
                priceToUse = book.BorrowPrice;
            }
            else
            {
                priceToUse = book.BuyPrice;
                if (book.DiscountPrice.HasValue && book.DiscountEndDate.HasValue && book.DiscountEndDate.Value >= DateTime.Now)
                {
                    priceToUse = book.DiscountPrice.Value;
                }
            }

            var returnUrl = Url.Action("CompleteBuyOrBorrow", "Payment", new { bookId = request.BookId, itemType = request.ItemType }, Request.Scheme);
            var approvalUrl = await _payPalService.CreateOrder(priceToUse, "USD", returnUrl);

            return Ok(new { approvalUrl });
        }


        [HttpGet("CompleteBuyOrBorrow")]
        public async Task<IActionResult> CompleteBuyOrBorrow(string token, int bookId, string itemType)
        {
            var isSuccess = await _payPalService.CompleteOrder(token);
            string message;
            string emailSubject;
            string emailBody;
            Book book = await _context.Books.FindAsync(bookId);

            if (book == null)
            {
                return NotFound();
            }

            if (isSuccess)
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userIdString != null && int.TryParse(userIdString, out int userId))
                {
                    if (itemType == "Buy")
                    {
                        var borrowTransaction = _context.BorrowTransactions
                            .FirstOrDefault(bt => bt.BookId == bookId && bt.UserId == userId && !bt.IsReturned);

                        if (borrowTransaction != null)
                        {
                            borrowTransaction.IsReturned = true;
                        }

                        var transaction = new Transaction
                        {
                            UserId = userId,
                            BookId = bookId,
                            TransactionType = "Buy",
                            TransactionDate = DateTime.Now,
                            Status = "Completed"
                        };
                        _context.Transactions.Add(transaction);

                        var waitingEntry = _context.WaitingList.FirstOrDefault(w => w.BookId == bookId && w.UserId == userId);
                        if (waitingEntry != null)
                        {
                            _context.WaitingList.Remove(waitingEntry);
                        }
                    }
                    else if (itemType == "Borrow")
                    {
                        var activeBorrowCount = _context.BorrowTransactions
                            .Count(bt => bt.BookId == bookId && !bt.IsReturned);

                        if (activeBorrowCount >= 3)
                        {
                            return BadRequest("This book has reached its borrow limit. Please join the waiting list.");
                        }

                        var reservations = _context.BookReservations
                            .Where(br => br.BookId == book.BookId && br.ReservationExpiry > DateTime.Now)
                            .ToList();

                        if (reservations.Any(r => r.UserId != userId) && !reservations.Any(r => r.UserId == userId))
                        {
                            return BadRequest("This book is reserved for another user.");
                        }

                        var borrowedCount = _context.BorrowTransactions
                            .Count(bt => bt.UserId == userId && !bt.IsReturned);

                        if (borrowedCount >= 3)
                        {
                            return BadRequest("You can borrow up to 3 books at the same time.");
                        }

                        var existingTransaction = _context.BorrowTransactions
                            .FirstOrDefault(bt => bt.BookId == bookId && bt.UserId == userId && !bt.IsReturned);

                        if (existingTransaction != null)
                        {
                            return BadRequest("You have already borrowed this book.");
                        }

                        if (book.AvailableCopies > 0 || reservations.Any(r => r.UserId == userId))
                        {
                            var borrowDate = DateTime.Now;
                            var returnDate = borrowDate.AddDays(30).AddMinutes(1);

                            var transaction = new BorrowTransaction
                            {
                                BookId = bookId,
                                UserId = userId,
                                BorrowDate = borrowDate,
                                ReturnDate = returnDate,
                                IsReturned = false
                            };

                            _context.BookReservations.RemoveRange(reservations);

                            if (!reservations.Any(r => r.UserId == userId))
                            {
                                book.AvailableCopies--;
                            }

                            _context.BorrowTransactions.Add(transaction);
                            await _context.SaveChangesAsync();

                            foreach (var reservation in reservations.Where(r => r.UserId != userId))
                            {
                                var user = _context.UserAccounts.FirstOrDefault(u => u.Id == reservation.UserId);
                                if (user != null)
                                {
                                    string subject = "Book Borrowed by Another User";
                                    string body = $@"
                                        <p>Dear {user.FirstName},</p>
                                        <p>Unfortunately, the book <strong>{book.Title}</strong> has been borrowed by another user.</p>
                                        <p>We apologize for the inconvenience.</p>
                                        <p>Thank you!</p>";

                                    await _emailService.SendEmailAsync(user.Email, subject, body);
                                }
                            }

                            var waitingEntry = _context.WaitingList.FirstOrDefault(w => w.BookId == book.BookId && w.UserId == userId);
                            if (waitingEntry != null)
                            {
                                _context.WaitingList.Remove(waitingEntry);
                                await _context.SaveChangesAsync();
                            }
                        }
                    }

                    await _context.SaveChangesAsync();

                    var userAccount = await _context.UserAccounts.FindAsync(userId);
                    if (userAccount != null)
                    {
                        emailSubject = "Payment Successful";
                        emailBody = $@"
                            <p>Dear {userAccount.FirstName},</p>
                            <p>Your payment for the book <strong>{book.Title}</strong> was successful.</p>
                            <p>Thank you for your purchase!</p>";
                        await _emailService.SendEmailAsync(userAccount.Email, emailSubject, emailBody);
                    }
                }
                message = "Payment successful! Thank you for your purchase.";
            }
            else
            {
                message = "Payment failed. Please try again.";

                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userIdString != null && int.TryParse(userIdString, out int userId))
                {
                    var userAccount = await _context.UserAccounts.FindAsync(userId);
                    if (userAccount != null)
                    {
                        emailSubject = "Payment Failed";
                        emailBody = $@"
                            <p>Dear {userAccount.FirstName},</p>
                            <p>Your payment for the book <strong>{book.Title}</strong> has failed.</p>
                            <p>Please try again.</p>";
                        await _emailService.SendEmailAsync(userAccount.Email, emailSubject, emailBody);
                    }
                }
            }
            return RedirectToAction("Index", "Books", new { message });
        }

        [HttpPost("CreateOrder")]
        public async Task<IActionResult> CreateOrder([FromForm] decimal amount, [FromForm] string currency = "USD")
        {
            _logger.LogInformation("CreateOrder called with amount: {Amount}, currency: {Currency}", amount, currency);

            if (amount <= 0)
            {
                _logger.LogWarning("Invalid amount: {Amount}", amount);
                return BadRequest("Amount must be greater than zero.");
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized();
            }

            var cartItems = _context.ShoppingCartItems.Where(i => i.UserId == userId).ToList();
            var borrowedCount = _context.BorrowTransactions
                .Count(bt => bt.UserId == userId && !bt.IsReturned);
            var cartBorrowCount = cartItems.Count(i => i.ItemType == "Borrow");

            if (borrowedCount + cartBorrowCount > 3)
            {
                return BadRequest("You can only borrow up to 3 books at the same time.");
            }

            decimal totalAmount = 0;

            foreach (var item in cartItems)
            {
                var book = await _context.Books.FindAsync(item.BookId);
                if (item.ItemType == "Borrow" && cartItems.Any(i => i.BookId == item.BookId && i.ItemType == "Buy"))
                {
                    _context.ShoppingCartItems.Remove(item);
                }
                else
                {
                    decimal priceToUse = item.ItemType == "Borrow" ? book.BorrowPrice : book.BuyPrice;
                    if (item.ItemType == "Buy" && book.DiscountPrice.HasValue && book.DiscountEndDate.HasValue && book.DiscountEndDate.Value >= DateTime.Now)
                    {
                        priceToUse = book.DiscountPrice.Value;
                    }
                    totalAmount += priceToUse * item.Quantity;
                }
            }

            await _context.SaveChangesAsync();

            var returnUrl = Url.Action("CompleteOrder", "Payment", null, Request.Scheme);
            var approvalUrl = await _payPalService.CreateOrder(totalAmount, currency, returnUrl);
            return Redirect(approvalUrl);
        }

        [HttpGet]
        public async Task<IActionResult> CompleteOrder(string token, string PayerID)
        {
            var isSuccess = await _payPalService.CompleteOrder(token);
            string message = "";
            string emailSubject;
            string emailBody;
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString != null && int.TryParse(userIdString, out int userId))
            {
                var items = _context.ShoppingCartItems.Where(i => i.UserId == userId).ToList();
                var borrowedCount = _context.BorrowTransactions
                    .Count(bt => bt.UserId == userId && !bt.IsReturned);

                if (isSuccess)
                {
                    var emailDetails = new List<string>();

                    foreach (var item in items)
                    {
                        var book = await _context.Books.FindAsync(item.BookId);
                        if (item.ItemType == "Buy")
                        {
                            var borrowTransaction = _context.BorrowTransactions
                                .FirstOrDefault(bt => bt.BookId == item.BookId && bt.UserId == userId && !bt.IsReturned);

                            if (borrowTransaction != null)
                            {
                                borrowTransaction.IsReturned = true;
                            }

                            var transaction = new Transaction
                            {
                                UserId = userId,
                                BookId = item.BookId,
                                TransactionType = "Buy",
                                TransactionDate = DateTime.Now,
                                Status = "Completed"
                            };
                            _context.Transactions.Add(transaction);

                            var waitingEntry = _context.WaitingList.FirstOrDefault(w => w.BookId == item.BookId && w.UserId == userId);
                            if (waitingEntry != null)
                            {
                                _context.WaitingList.Remove(waitingEntry);
                            }

                            emailDetails.Add($"Bought: {book.Title} for {book.BuyPrice.ToString("C", new System.Globalization.CultureInfo("en-US"))}");
                        }
                        else if (item.ItemType == "Borrow")
                        {
                            var borrowDate = DateTime.Now;
                            var returnDate = borrowDate.AddDays(30).AddMinutes(1);

                            var borrowTransaction = new BorrowTransaction
                            {
                                UserId = userId,
                                BookId = item.BookId,
                                BorrowDate = borrowDate,
                                ReturnDate = returnDate,
                                IsReturned = false
                            };
                            _context.BorrowTransactions.Add(borrowTransaction);

                            emailDetails.Add($"Borrowed: {book.Title} for {book.BorrowPrice.ToString("C", new System.Globalization.CultureInfo("en-US"))}");
                        }
                    }

                    _context.ShoppingCartItems.RemoveRange(items);
                    await _context.SaveChangesAsync();

                    var userAccount = await _context.UserAccounts.FindAsync(userId);
                    if (userAccount != null)
                    {
                        emailSubject = "Payment Successful";
                        emailBody = $@"
                    <p>Dear {userAccount.FirstName},</p>
                    <p>Your payment for the items in your cart was successful. Here are the details:</p>
                    <ul>
                        {string.Join("", emailDetails.Select(detail => $"<li>{detail}</li>"))}
                    </ul>
                    <p>Thank you for your purchase!</p>";
                        await _emailService.SendEmailAsync(userAccount.Email, emailSubject, emailBody);
                    }

                    message = "Payment successful! Thank you for your purchase.";
                }
                else
                {
                    message = "Payment failed. Please try again.";

                    var userAccount = await _context.UserAccounts.FindAsync(userId);
                    if (userAccount != null)
                    {
                        emailSubject = "Payment Failed";
                        emailBody = $@"
                            <p>Dear {userAccount.FirstName},</p>
                            <p>Your payment for the items in your cart has failed.</p>
                            <p>Please try again.</p>";
                        await _emailService.SendEmailAsync(userAccount.Email, emailSubject, emailBody);
                    }
                }
            }
            return RedirectToAction("Index", "Books", new { message });
        }

        public class BuyOrBorrowRequest
        {
            public int BookId { get; set; }
            public string ItemType { get; set; }
            public string Title { get; set; }
            public decimal Price { get; set; }
        }
    }
}
