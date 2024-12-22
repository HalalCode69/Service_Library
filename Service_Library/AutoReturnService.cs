using System;
using System.Linq;
using Service_Library.Entities;

public class AutoReturnService
{
    private readonly AppDbContext _context;

    public AutoReturnService(AppDbContext context)
    {
        _context = context;
    }

    public void AutoReturnOverdueBooks()
    {
        var now = DateTime.Now;
        var overdueTransactions = _context.BorrowTransactions
            .Where(bt => !bt.IsReturned && bt.ReturnDate <= now)
            .ToList();

        foreach (var transaction in overdueTransactions)
        {
            transaction.IsReturned = true;

            var book = _context.Books.FirstOrDefault(b => b.BookId == transaction.BookId);
            if (book != null)
            {
                book.AvailableCopies++;
            }
        }

        _context.SaveChanges();
    }
}
