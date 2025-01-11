using System;
using System.Linq;
using Service_Library.Entities;
using Service_Library.Models;

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

    public void CleanUpExpiredReservations()
    {
        var now = DateTime.Now;
        var expiredReservations = _context.BookReservations
            .Where(br => br.ReservationExpiry <= now)
            .ToList();

        Console.WriteLine($"Found {expiredReservations.Count} expired reservations.");

        foreach (var reservation in expiredReservations)
        {
            Console.WriteLine($"Removing reservation for BookId: {reservation.BookId}, UserId: {reservation.UserId}");

            // Remove the expired reservation
            _context.BookReservations.Remove(reservation);

            // Check if the user is already in the waiting list
            var existingWaitingEntry = _context.WaitingList
                .FirstOrDefault(wl => wl.BookId == reservation.BookId && wl.UserId == reservation.UserId);

            if (existingWaitingEntry != null)
            {
                // Update the AddedDate to now to move the user to the end of the waiting list
                existingWaitingEntry.AddedDate = now;
            }
            else
            {
                // Add the user to the end of the waiting list
                var waitingEntry = new WaitingList
                {
                    BookId = reservation.BookId,
                    UserId = reservation.UserId,
                    AddedDate = now
                };
                _context.WaitingList.Add(waitingEntry);
            }
        }

        // Handle the case where no one borrowed the books from the reservations
        var booksWithNoReservations = _context.Books
            .Where(b => !_context.BookReservations.Any(br => br.BookId == b.BookId))
            .ToList();

        foreach (var book in booksWithNoReservations)
        {
            var waitingListEntries = _context.WaitingList
                .Where(wl => wl.BookId == book.BookId)
                .OrderBy(wl => wl.AddedDate)
                .Take(3)
                .ToList();

            foreach (var entry in waitingListEntries)
            {
                // Copy the user from the waiting list to the book reservation
                var reservation = new BookReservation
                {
                    BookId = entry.BookId,
                    UserId = entry.UserId,
                    ReservationExpiry = now.AddHours(24) // Set the reservation expiry to 24 hours from now
                };
                _context.BookReservations.Add(reservation);
            }
        }

        _context.SaveChanges();
    }



}
