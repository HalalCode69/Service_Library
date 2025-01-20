using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Library.Models;
using Service_Library.Entities;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace Service_Library.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly AppDbContext _context;

        public ShoppingCartController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized();
            }

            var cartItems = await _context.ShoppingCartItems
                .Include(item => item.Book)
                .Where(item => item.UserId == userId)
                .ToListAsync();
            var items = await _context.ShoppingCartItems
                .Where(item => item.UserId == userId)
                .ToListAsync();
            return View(items);
        }

        [HttpPost]
        public IActionResult Add([FromBody] CartItemDto cartItemDto)
        {
            try
            {
                if (string.IsNullOrEmpty(cartItemDto.Title))
                {
                    return Json(new { success = false, message = "Title is required." });
                }

                if (string.IsNullOrEmpty(cartItemDto.ItemType))
                {
                    return Json(new { success = false, message = "ItemType is required." });
                }

                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userIdString == null || !int.TryParse(userIdString, out int userId))
                {
                    return Unauthorized();
                }

                if (cartItemDto.ItemType == "Borrow")
                {
                    var borrowedCount = _context.BorrowTransactions
                        .Count(bt => bt.UserId == userId && !bt.IsReturned);

                    if (borrowedCount >= 3)
                    {
                        return Json(new { success = false, message = "You can borrow up to 3 books at the same time." });
                    }
                }

                var book = _context.Books.FirstOrDefault(b => b.BookId == cartItemDto.BookId);
                if (book == null)
                {
                    return NotFound();
                }

                decimal priceToUse = cartItemDto.ItemType == "Borrow" ? book.BorrowPrice : book.BuyPrice;
                if (cartItemDto.ItemType == "Buy" && book.DiscountPrice.HasValue && book.DiscountEndDate.HasValue && book.DiscountEndDate.Value >= DateTime.Now)
                {
                    priceToUse = book.DiscountPrice.Value;
                }

                var item = _context.ShoppingCartItems
                    .FirstOrDefault(i => i.UserId == userId && i.BookId == cartItemDto.BookId && i.ItemType == cartItemDto.ItemType);

                if (item == null)
                {
                    item = new ShoppingCartItem
                    {
                        UserId = userId,
                        BookId = cartItemDto.BookId,
                        Title = cartItemDto.Title,
                        Price = priceToUse,
                        ItemType = cartItemDto.ItemType,
                        Quantity = 1
                    };
                    _context.ShoppingCartItems.Add(item);
                }
                else
                {
                    item.Quantity++;
                }

                _context.SaveChanges();
                return Json(new { success = true, message = "Item added to cart." });
            }
            catch (DbUpdateException ex)
            {
                var innerException = ex.InnerException?.Message ?? ex.Message;
                return Json(new { success = false, message = $"An error occurred while adding to the cart: {innerException}" });
            }
        }












        [HttpPost]
        public IActionResult Remove([FromBody] CartItemDto cartItemDto)
        {
            try
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userIdString == null || !int.TryParse(userIdString, out int userId))
                {
                    return Unauthorized();
                }

                var item = _context.ShoppingCartItems
                    .FirstOrDefault(i => i.UserId == userId && i.BookId == cartItemDto.BookId);

                if (item != null)
                {
                    _context.ShoppingCartItems.Remove(item);
                    _context.SaveChanges();
                    return Json(new { success = true, message = "Item removed from cart." });
                }

                return Json(new { success = false, message = "Item not found in cart." });
            }
            catch (DbUpdateException ex)
            {
                var innerException = ex.InnerException?.Message ?? ex.Message;
                return Json(new { success = false, message = $"An error occurred while removing from the cart: {innerException}" });
            }
        }

        [HttpPost]
        public IActionResult Clear()
        {
            try
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userIdString == null || !int.TryParse(userIdString, out int userId))
                {
                    return Unauthorized();
                }

                var items = _context.ShoppingCartItems.Where(i => i.UserId == userId).ToList();

                if (items.Any())
                {
                    _context.ShoppingCartItems.RemoveRange(items);
                    _context.SaveChanges();
                    return Json(new { success = true, message = "Cart cleared." });
                }

                return Json(new { success = false, message = "Cart is already empty." });
            }
            catch (DbUpdateException ex)
            {
                var innerException = ex.InnerException?.Message ?? ex.Message;
                return Json(new { success = false, message = $"An error occurred while clearing the cart: {innerException}" });
            }
        }
        [HttpGet]
        public IActionResult GetBorrowCount()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier); 
            if (userIdString == null || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized();
            }

            var borrowedCount = _context.BorrowTransactions
                .Count(bt => bt.UserId == userId && !bt.IsReturned);

            var cartBorrowCount = _context.ShoppingCartItems
                .Count(i => i.UserId == userId && i.ItemType == "Borrow");

            return Json(new { borrowedCount, cartBorrowCount });
        }


    }

    public class CartItemDto
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public string ItemType { get; set; }
    }


}
