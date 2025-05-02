using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Service_Library.Models;
using Service_Library.Entities;
using Service_Library.Services;
using System.Security.Claims;

namespace Service_Library.Controllers
{
    public class SqlController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public SqlController(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            try
            {
                var hashedPassword = PasswordHasher.HashPassword(password);
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    bool looksLikeInjection = password.Contains("'") || password.Contains("--");

                    string sql;
                    if (looksLikeInjection)
                    {
                        // vulnerable demo path
                        sql = $@"
                        SELECT *
                            FROM UserAccounts
                        WHERE Email    = '{email}'
                            AND Password = '{password}'";    // raw
                    }
                    else
                    {
                        // normal (hashed) path
                        var hash = PasswordHasher.HashPassword(password);
                        sql = $@"
                        SELECT *
                            FROM UserAccounts
                        WHERE Email    = '{email}'
                            AND Password = '{hash}'";        // secure
                    }

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var claims = new List<Claim>
                                {
                                    new Claim(ClaimTypes.Name, reader["Email"].ToString()),
                                    new Claim(ClaimTypes.Role, reader["Role"].ToString() ?? "User")
                                };

                                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                    new ClaimsPrincipal(claimsIdentity));

                                return RedirectToAction("Index", "Home");
                            }
                        }
                    }
                }
                ModelState.AddModelError("", "Invalid login attempt.");
                return View();
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while logging in.");
                return View();
            }
        }
    }
} 