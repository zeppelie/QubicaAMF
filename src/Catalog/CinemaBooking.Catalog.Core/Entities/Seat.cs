using System;
using System.Collections.Generic;

namespace CinemaBooking.Catalog.Core.Entities;

public partial class Seat
{
    public int SeatId { get; set; }

    public int HallId { get; set; }

    public string RowLabel { get; set; } = null!;

    public int SeatNumber { get; set; }

    public virtual Hall Hall { get; set; } = null!;
}
