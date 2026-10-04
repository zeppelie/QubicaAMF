using System;
using System.Collections.Generic;

namespace CinemaBooking.Catalog.Core.Entities;

public partial class Hall
{
    public int HallId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();

    public virtual ICollection<Show> Shows { get; set; } = new List<Show>();
}
