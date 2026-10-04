using CinemaBooking.Bookings.Core;
using CinemaBooking.Bookings.Core.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Bookings.Infrastructure.Persistence;

public sealed class BookingRepository(BookingsDbContext db) : IBookingRepository
{
    private const int DuplicateKeyInUniqueIndex = 2601;

    public async Task<IReadOnlySet<int>> ListTakenSeatIdsAsync(int showId, CancellationToken cancellationToken) =>
        await db.BookedSeats
            .Where(seat => seat.ShowId == showId && seat.CancelledAt == null)
            .Select(seat => seat.SeatId)
            .ToHashSetAsync(cancellationToken);

    public async Task<bool> TryAddAsync(Booking booking, CancellationToken cancellationToken)
    {
        db.Bookings.Add(booking);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: DuplicateKeyInUniqueIndex })
        {
            return false;
        }
    }

    public Task<Booking?> FindAsync(int bookingId, CancellationToken cancellationToken) =>
        db.Bookings
            .Include(booking => booking.BookedSeats)
            .FirstOrDefaultAsync(booking => booking.BookingId == bookingId, cancellationToken);

    public async Task<IReadOnlyList<Booking>> ListByUserAsync(int userId, CancellationToken cancellationToken) =>
        await db.Bookings.AsNoTracking()
            .Include(booking => booking.BookedSeats)
            .Where(booking => booking.UserId == userId)
            .OrderByDescending(booking => booking.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task UpdateAsync(Booking booking, CancellationToken cancellationToken) =>
        db.SaveChangesAsync(cancellationToken);
}
