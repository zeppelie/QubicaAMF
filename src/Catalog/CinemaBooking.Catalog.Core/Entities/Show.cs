using System;
using System.Collections.Generic;

namespace CinemaBooking.Catalog.Core.Entities;

public partial class Show
{
    public int ShowId { get; set; }

    public int MovieId { get; set; }

    public int HallId { get; set; }

    public DateTime StartsAt { get; set; }

    public virtual Hall Hall { get; set; } = null!;

    public virtual Movie Movie { get; set; } = null!;
}
