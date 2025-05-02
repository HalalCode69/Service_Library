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
        public DbSet<WaitingList> WaitingList { get; set; } 
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<WebsiteIcon> WebsiteIcons { get; set; }
        public DbSet<BorrowTransaction> BorrowTransactions { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<UserRating> UserRatings { get; set; }
        public DbSet<BookReservation> BookReservations { get; set; }
        public DbSet<ShoppingCartItem> ShoppingCartItems { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<CreditCardInfo> CreditCardInfos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Book>()
                .HasMany(b => b.Genres)
                .WithMany(g => g.Books)
                .UsingEntity(j => j.ToTable("BookGenres"));

        }
    }
}
