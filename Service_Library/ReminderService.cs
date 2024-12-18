using Service_Library.Entities;
using Service_Library.Services;
using Microsoft.EntityFrameworkCore;

namespace Service_Library.Services
{
    public class ReminderService
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        public ReminderService(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task SendReminderEmails()
        {
            var upcomingReturns = await _context.BorrowTransactions
                .Include(bt => bt.User)
                .Include(bt => bt.Book)
                .Where(bt => !bt.IsReturned &&
                             bt.ReturnDate <= DateTime.Now.AddDays(5) &&
                             bt.ReturnDate > DateTime.Now &&
                             !bt.ReminderSent)
                .ToListAsync();

            foreach (var transaction in upcomingReturns)
            {
                string subject = "Reminder: Return Your Book";
                string body = $@"
                    <p>Dear {transaction.User.FirstName},</p>
                    <p>This is a reminder that you need to return the book 
                    <strong>{transaction.Book.Title}</strong> by 
                    <strong>{transaction.ReturnDate.ToShortDateString()}</strong>.</p>
                    <p>Please ensure timely returns to avoid penalties.</p>
                    <p>Thank you!</p>";

                await _emailService.SendEmailAsync(transaction.User.Email, subject, body);

                // Mark reminder as sent
                transaction.ReminderSent = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}
