using Service_Library.Models;
using Microsoft.EntityFrameworkCore;

namespace Service_Library.Entities
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<WaitingList> WaitingList { get; set; }  // To store users waiting for a book
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<WebsiteIcon> WebsiteIcons { get; set; }
        public DbSet<BorrowTransaction> BorrowTransactions { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           base.OnModelCreating(modelBuilder);
        }
    }
}
