using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service_Library.Entities;
using Service_Library.Models;

namespace Service_Library.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ManageBooks()
        {
            var books = _context.Books.ToList();
            return View(books);
        }

        public IActionResult ManageAccounts()
        {
            var users = _context.UserAccounts.ToList();
            return View(users);
        }

        [HttpPost]
        public IActionResult PromoteToAdmin(int id)
        {
            var user = _context.UserAccounts.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                user.Role = "Admin";
                _context.SaveChanges();
            }
            return RedirectToAction("ManageAccounts");
        }

        [HttpPost]
        public IActionResult DemoteToUser(int id)
        {
            var user = _context.UserAccounts.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                var adminCount = _context.UserAccounts.Count(u => u.Role == "Admin");
                if (adminCount > 1)
                {
                    user.Role = "User";
                    _context.SaveChanges();
                }
                else
                {
                    ModelState.AddModelError("", "There must be at least one admin.");
                }
            }
            return RedirectToAction("ManageAccounts");
        }

        [HttpPost]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.UserAccounts.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                var adminCount = _context.UserAccounts.Count(u => u.Role == "Admin");
                if (user.Role == "Admin" && adminCount <= 1)
                {
                    ModelState.AddModelError("", "There must be at least one admin.");
                }
                else
                {
                    _context.UserAccounts.Remove(user);
                    _context.SaveChanges();
                }
            }
            return RedirectToAction("ManageAccounts");
        }

        [HttpGet]
        public IActionResult AddBook()
        {
            ViewBag.Categories = new List<string>
            {
                "Adventure", "Adventure Travel", "Afrofuturism", "Alternative Medicine", "Animation",
                "Animal Stories", "Anthology", "Anthropology", "Archaeology", "Architecture",
                "Art", "Art History", "Astrology", "Astronomy", "Biology",
                "Biography", "Business", "Chemistry", "Children's Books", "Classic Literature",
                "Comic Books", "Cooking", "Crafts & Hobbies", "Cultural Studies", "Cyberpunk",
                "Design", "Digital Media", "DIY Projects", "Drama", "Dystopian",
                "Economics", "Education", "Engineering", "Environment", "Environmental Science",
                "Epic Fiction", "Espionage", "Esoterica", "Fantasy", "Fantasy Fiction",
                "Fiction", "Folklore", "Futurism", "Gardening", "Geography",
                "Graphic Novels", "Health", "Historical Fiction", "History", "Horror",
                "Humor", "Inspirational", "Legal Thriller", "Linguistics", "Meditation",
                "Memoir", "Military", "Mindfulness", "Music", "Mystery",
                "Mythology", "Non-Fiction", "Paleontology", "Paranormal", "Parenting",
                "Performing Arts", "Personal Development", "Philosophy", "Photography", "Physics",
                "Poetry", "Politics", "Pop Culture", "Psychology", "Religion",
                "Romance", "Satire", "Science", "Science Fiction", "Self-Help",
                "Short Stories", "Social Justice", "Space Exploration", "Spirituality", "Sports",
                "Survival Stories", "Technology", "Thriller", "Travel", "True Crime",
                "Urban Fiction", "Vegan Cooking", "War Stories", "Western", "Wine & Spirits",
                "World Cultures", "Zoology"
            };


            return View();
        }


        [HttpPost]
        public IActionResult AddBook(Book model, IFormFile CoverImage, IFormFile BookContent)
        {
            if (ModelState.IsValid)
            {
                // Handle cover image upload
                if (CoverImage != null && CoverImage.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        CoverImage.CopyTo(ms);
                        model.CoverImage = ms.ToArray(); // Convert the file to byte array
                    }
                }

                // Handle book content upload
                if (BookContent != null && BookContent.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        BookContent.CopyTo(ms);
                        model.BookContent = ms.ToArray(); // Convert the file to byte array
                    }
                }

                // Save the book in the database
                _context.Books.Add(model);
                _context.SaveChanges();
                return RedirectToAction("ManageBooks");
            }

            // Repopulate categories if validation fails
            ViewBag.Categories = new List<string>
                {
                    "Adventure", "Adventure Travel", "Afrofuturism", "Alternative Medicine", "Animation",
                    "Animal Stories", "Anthology", "Anthropology", "Archaeology", "Architecture",
                    "Art", "Art History", "Astrology", "Astronomy", "Biology",
                    "Biography", "Business", "Chemistry", "Children's Books", "Classic Literature",
                    "Comic Books", "Cooking", "Crafts & Hobbies", "Cultural Studies", "Cyberpunk",
                    "Design", "Digital Media", "DIY Projects", "Drama", "Dystopian",
                    "Economics", "Education", "Engineering", "Environment", "Environmental Science",
                    "Epic Fiction", "Espionage", "Esoterica", "Fantasy", "Fantasy Fiction",
                    "Fiction", "Folklore", "Futurism", "Gardening", "Geography",
                    "Graphic Novels", "Health", "Historical Fiction", "History", "Horror",
                    "Humor", "Inspirational", "Legal Thriller", "Linguistics", "Meditation",
                    "Memoir", "Military", "Mindfulness", "Music", "Mystery",
                    "Mythology", "Non-Fiction", "Paleontology", "Paranormal", "Parenting",
                    "Performing Arts", "Personal Development", "Philosophy", "Photography", "Physics",
                    "Poetry", "Politics", "Pop Culture", "Psychology", "Religion",
                    "Romance", "Satire", "Science", "Science Fiction", "Self-Help",
                    "Short Stories", "Social Justice", "Space Exploration", "Spirituality", "Sports",
                    "Survival Stories", "Technology", "Thriller", "Travel", "True Crime",
                    "Urban Fiction", "Vegan Cooking", "War Stories", "Western", "Wine & Spirits",
                    "World Cultures", "Zoology"
                };

            return View(model);
        }








        [HttpGet]
        public IActionResult EditBook(int id)
        {
            var book = _context.Books.FirstOrDefault(b => b.BookId == id);
            if (book == null) return NotFound();

            ViewBag.Categories = new List<string>
            {
                "Adventure", "Adventure Travel", "Afrofuturism", "Alternative Medicine", "Animation",
                "Animal Stories", "Anthology", "Anthropology", "Archaeology", "Architecture",
                "Art", "Art History", "Astrology", "Astronomy", "Biology",
                "Biography", "Business", "Chemistry", "Children's Books", "Classic Literature",
                "Comic Books", "Cooking", "Crafts & Hobbies", "Cultural Studies", "Cyberpunk",
                "Design", "Digital Media", "DIY Projects", "Drama", "Dystopian",
                "Economics", "Education", "Engineering", "Environment", "Environmental Science",
                "Epic Fiction", "Espionage", "Esoterica", "Fantasy", "Fantasy Fiction",
                "Fiction", "Folklore", "Futurism", "Gardening", "Geography",
                "Graphic Novels", "Health", "Historical Fiction", "History", "Horror",
                "Humor", "Inspirational", "Legal Thriller", "Linguistics", "Meditation",
                "Memoir", "Military", "Mindfulness", "Music", "Mystery",
                "Mythology", "Non-Fiction", "Paleontology", "Paranormal", "Parenting",
                "Performing Arts", "Personal Development", "Philosophy", "Photography", "Physics",
                "Poetry", "Politics", "Pop Culture", "Psychology", "Religion",
                "Romance", "Satire", "Science", "Science Fiction", "Self-Help",
                "Short Stories", "Social Justice", "Space Exploration", "Spirituality", "Sports",
                "Survival Stories", "Technology", "Thriller", "Travel", "True Crime",
                "Urban Fiction", "Vegan Cooking", "War Stories", "Western", "Wine & Spirits",
                "World Cultures", "Zoology"
            };


            return View(book);
        }



        [HttpPost]
        public IActionResult EditBook([Bind("BookId,Title,Author,Publisher,Format,BorrowPrice,BuyPrice,AvailableCopies,IsBorrowable,Category,AgeLimit,YearOfPublishing,DiscountPrice,DiscountEndDate")] Book model, IFormFile? CoverImage, IFormFile? BookContent)
        {
            // Always populate ViewBag.Categories
            ViewBag.Categories = new List<string>
            {
                "Adventure", "Adventure Travel", "Afrofuturism", "Alternative Medicine", "Animation",
                "Animal Stories", "Anthology", "Anthropology", "Archaeology", "Architecture",
                "Art", "Art History", "Astrology", "Astronomy", "Biology",
                "Biography", "Business", "Chemistry", "Children's Books", "Classic Literature",
                "Comic Books", "Cooking", "Crafts & Hobbies", "Cultural Studies", "Cyberpunk",
                "Design", "Digital Media", "DIY Projects", "Drama", "Dystopian",
                "Economics", "Education", "Engineering", "Environment", "Environmental Science",
                "Epic Fiction", "Espionage", "Esoterica", "Fantasy", "Fantasy Fiction",
                "Fiction", "Folklore", "Futurism", "Gardening", "Geography",
                "Graphic Novels", "Health", "Historical Fiction", "History", "Horror",
                "Humor", "Inspirational", "Legal Thriller", "Linguistics", "Meditation",
                "Memoir", "Military", "Mindfulness", "Music", "Mystery",
                "Mythology", "Non-Fiction", "Paleontology", "Paranormal", "Parenting",
                "Performing Arts", "Personal Development", "Philosophy", "Photography", "Physics",
                "Poetry", "Politics", "Pop Culture", "Psychology", "Religion",
                "Romance", "Satire", "Science", "Science Fiction", "Self-Help",
                "Short Stories", "Social Justice", "Space Exploration", "Spirituality", "Sports",
                "Survival Stories", "Technology", "Thriller", "Travel", "True Crime",
                "Urban Fiction", "Vegan Cooking", "War Stories", "Western", "Wine & Spirits",
                "World Cultures", "Zoology"
            };

            // Check ModelState validity
            if (!ModelState.IsValid)
            {
                // Log the errors for debugging
                foreach (var modelStateKey in ModelState.Keys)
                {
                    var value = ModelState[modelStateKey];
                    foreach (var error in value.Errors)
                    {
                        Console.WriteLine($"Error in {modelStateKey}: {error.ErrorMessage}");
                    }
                }

                // Return view with validation errors displayed
                return View(model);
            }

            var existingBook = _context.Books.FirstOrDefault(b => b.BookId == model.BookId);
            if (existingBook == null)
            {
                return NotFound();
            }

            // Validation: Borrow price must be less than buy price
            if (model.BorrowPrice >= model.BuyPrice)
            {
                ModelState.AddModelError("", "Borrow price must be lower than the buy price.");
                return View(model);
            }

            // Update basic book properties
            existingBook.Title = model.Title;
            existingBook.Author = model.Author;
            existingBook.Publisher = model.Publisher;
            existingBook.Format = model.Format;
            existingBook.BorrowPrice = model.BorrowPrice;
            existingBook.BuyPrice = model.BuyPrice;
            existingBook.AvailableCopies = model.AvailableCopies;
            existingBook.IsBorrowable = model.IsBorrowable;
            existingBook.Category = model.Category;

            // Update Cover Image if a new one is provided
            if (CoverImage != null && CoverImage.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    CoverImage.CopyTo(ms);
                    existingBook.CoverImage = ms.ToArray();
                }
            }

            // Update Book Content if a new one is provided
            if (BookContent != null && BookContent.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    BookContent.CopyTo(ms);
                    existingBook.BookContent = ms.ToArray(); // Convert the file to byte array
                }
            }

            // Handle Discount Logic
            if (model.DiscountPrice.HasValue && model.DiscountEndDate.HasValue)
            {
                if (model.DiscountEndDate <= DateTime.Now.AddDays(7))
                {
                    // Apply the discount
                    existingBook.DiscountPrice = model.DiscountPrice;
                    existingBook.DiscountEndDate = model.DiscountEndDate;
                    existingBook.PreviousBuyPrice = existingBook.BuyPrice; // Save the original price
                }
                else
                {
                    ModelState.AddModelError("", "Discount duration cannot exceed 7 days.");
                    return View(model);
                }
            }
            else
            {
                // Reset discount fields if no discount is applied
                existingBook.DiscountPrice = null;
                existingBook.DiscountEndDate = null;
                existingBook.PreviousBuyPrice = null;
            }

            _context.SaveChanges();
            return RedirectToAction("ManageBooks");
        }




        public IActionResult DeleteBook(int id)
        {
            var book = _context.Books.FirstOrDefault(b => b.BookId == id);
            if (book != null)
            {
                _context.Books.Remove(book);
                _context.SaveChanges();
            }
            return RedirectToAction("ManageBooks");
        }
        [HttpGet]
        public IActionResult UploadIcon()
        {
            return View();
        }

        [HttpPost]
        public IActionResult UploadIcon(IFormFile iconFile)
        {
            if (iconFile != null && iconFile.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    iconFile.CopyTo(ms);
                    var websiteIcon = new WebsiteIcon
                    {
                        Name = iconFile.FileName,
                        IconData = ms.ToArray(),
                        ContentType = iconFile.ContentType
                    };

                    // Remove old icon if it exists
                    var oldIcon = _context.WebsiteIcons.FirstOrDefault();
                    if (oldIcon != null)
                    {
                        _context.WebsiteIcons.Remove(oldIcon);
                    }

                    _context.WebsiteIcons.Add(websiteIcon);
                    _context.SaveChanges();
                }
            }

            return RedirectToAction("Index"); // Or another action
        }
        public IActionResult GetIcon()
        {
            var icon = _context.WebsiteIcons.FirstOrDefault();
            if (icon != null)
            {
                return File(icon.IconData, icon.ContentType);
            }

            return NotFound(); // Or return a default placeholder image
        }

    }
}
