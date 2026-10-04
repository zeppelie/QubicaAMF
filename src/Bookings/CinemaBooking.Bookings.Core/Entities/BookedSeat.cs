using System;
using System.Collections.Generic;

namespace CinemaBooking.Bookings.Core.Entities;

public partial class BookedSeat
{
    public int BookedSeatId { get; set; }

    public int BookingId { get; set; }

    public int ShowId { get; set; }

    public int SeatId { get; set; }

    public DateTime? CancelledAt { get; set; }

    public virtual Booking Booking { get; set; } = null!;
}
