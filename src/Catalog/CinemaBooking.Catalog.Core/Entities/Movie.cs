using System;
using System.Collections.Generic;

namespace CinemaBooking.Catalog.Core.Entities;

public partial class Movie
{
    public int MovieId { get; set; }

    public string Title { get; set; } = null!;

    public int DurationMinutes { get; set; }

    public virtual ICollection<Show> Shows { get; set; } = new List<Show>();
}
