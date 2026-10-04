namespace CinemaBooking.Bookings.Core.Entities;

public partial class Booking
{
    public bool IsCancelled => CancelledAt is not null;

    public void Cancel(DateTime now)
    {
        CancelledAt = now;
        foreach (var seat in BookedSeats)
            seat.CancelledAt = now;
    }
}
