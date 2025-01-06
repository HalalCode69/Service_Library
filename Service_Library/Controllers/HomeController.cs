using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Service_Library.Models;
using System.Linq;
using Service_Library.Entities;
using Service_Library.Services;

namespace Service_Library.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context; // Add DbContext
        private readonly EmailService _emailService;

        public HomeController(ILogger<HomeController> logger, AppDbContext context, EmailService emailService)
        {
            _logger = logger;
            _context = context; // Inject DbContext
            _emailService = emailService;
        }

        public IActionResult Index()
        {

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult GetWebsiteIcon()
        {
            var icon = _context.WebsiteIcons.FirstOrDefault(); // Fetch the first icon from the table

            if (icon != null)
            {
                return File(icon.IconData, icon.ContentType); // Return the image as a file response
            }

            return NotFound(); // Return 404 if no icon exists
        }
        [HttpGet("trigger-reminder-emails")]
        public async Task<IActionResult> TriggerReminderEmails([FromServices] ReminderService reminderService)
        {
            await reminderService.SendReminderEmails();
            return Ok("Reminder emails sent!");
        }
        [HttpGet]
        public IActionResult GetFavicon()
        {
            var icon = _context.WebsiteIcons.FirstOrDefault();
            if (icon == null)
            {
                return NotFound();
            }

            return File(icon.IconData, icon.ContentType);
        }
        [HttpGet]
        public async Task<IActionResult> TestEmail()
        {
            try
            {
                string recipient = "israelnewdj@gmail.com"; // Replace with your test email address
                string subject = "Test Email";
                string body = "<h1>This is a test email</h1><p>Testing SMTP integration.</p>";

                await _emailService.SendEmailAsync(recipient, subject, body);

                return Ok("Test email sent successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error sending email: {ex.Message}");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
