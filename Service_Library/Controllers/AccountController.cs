using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Library.Entities;
using Service_Library.Models;
using Service_Library.Services;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Service_Library.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private readonly IWebHostEnvironment _environment;

        public AccountController(AppDbContext context, IConfiguration configuration, EmailService emailService, IWebHostEnvironment environment)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _environment = environment;
        }

        public IActionResult Index()
        {
            return View(_context.UserAccounts.ToList());
        }

        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registration(RegistrationViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if this is the first user
                bool isFirstUser = !_context.UserAccounts.Any();
                
                UserAccount acc = new UserAccount
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Password = PasswordHasher.HashPassword(model.Password),
                    Role = isFirstUser ? "Admin" : "User" // First user is always admin
                };

                try
                {
                    _context.UserAccounts.Add(acc);
                    _context.SaveChanges();

                    // Add credit card information
                    var creditCard = new CreditCardInfo
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        PersonalId = model.PersonalId,
                        CreditCardNumber = model.CreditCardNumber,
                        ValidDate = model.ValidDate,
                        CVC = model.CVC,
                        UserAccountId = acc.Id,
                        Note = model.Note
                    };

                    _context.CreditCardInfos.Add(creditCard);
                    _context.SaveChanges();

                    ModelState.Clear();
                    return RedirectToAction("Login", "Account", new { message = "Registration successful. Please log in.", email = model.Email });
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Email already exists");
                    return View(model);
                }
            }
            return View(model);
        }

        public IActionResult Login(string message = null, string email = null)
        {
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.Message = message;
            }

            if (!string.IsNullOrEmpty(email))
            {
                ViewBag.Email = email;
            }

            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.UserAccounts
                    .FirstOrDefault(u => u.Email == model.Email);

                if (user != null && PasswordHasher.VerifyPassword(model.Password, user.Password))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.Email),
                        new Claim("FirstName", user.FirstName),
                        new Claim("LastName", user.LastName),
                        new Claim(ClaimTypes.Role, user.Role ?? "User")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity)
                    );

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid Email or Password");
                }
            }
            return View(model);
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.UserAccounts.FirstOrDefault(u => u.Email == model.Email);
                if (user != null)
                {
                    // Generate password reset token
                    var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                    user.PasswordResetToken = token;
                    user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
                    _context.SaveChanges();

                    // Create reset link
                    var resetLink = Url.Action("ResetPassword", "Account",
                        new { email = model.Email, token = token },
                        protocol: Request.Scheme);

                    // Create email body
                    var emailBody = $@"
                        <h2>Password Reset Request</h2>
                        <p>Hello {user.FirstName},</p>
                        <p>You have requested to reset your password. Click the link below to reset it:</p>
                        <p><a href='{resetLink}'>Reset Password</a></p>
                        <p>If you did not request this reset, please ignore this email.</p>
                        <p>This link will expire in 1 hour.</p>
                        <p>Best regards,<br/>Your Library Team</p>";

                    // Send email
                    await _emailService.SendEmailAsync(
                        user.Email,
                        "Password Reset Request",
                        emailBody
                    );

                    return RedirectToAction("Login", new { message = "Password reset instructions have been sent to your email." });
                }
                else
                {
                    // Don't reveal that the user doesn't exist
                    return RedirectToAction("Login", new { message = "If your email is registered, you will receive password reset instructions." });
                }
            }
            return View(model);
        }

        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", new { message = "Invalid password reset link." });
            }

            var user = _context.UserAccounts.FirstOrDefault(u => 
                u.Email == email && 
                u.PasswordResetToken == token &&
                u.PasswordResetTokenExpiry > DateTime.UtcNow);

            if (user == null)
            {
                return RedirectToAction("Login", new { message = "Invalid or expired password reset link." });
            }

            var model = new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.UserAccounts.FirstOrDefault(u => 
                    u.Email == model.Email && 
                    u.PasswordResetToken == model.Token &&
                    u.PasswordResetTokenExpiry > DateTime.UtcNow);

                if (user != null)
                {
                    user.Password = PasswordHasher.HashPassword(model.NewPassword);
                    user.PasswordResetToken = null;
                    user.PasswordResetTokenExpiry = null;
                    _context.SaveChanges();

                    return RedirectToAction("Login", new { message = "Password has been reset successfully. Please log in with your new password." });
                }
                else
                {
                    ModelState.AddModelError("", "Invalid or expired password reset token");
                }
            }
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [Authorize]
        public IActionResult SecurePage()
        {
            ViewBag.Name = HttpContext.User.Identity.Name;
            return View();
        }
    }
}
