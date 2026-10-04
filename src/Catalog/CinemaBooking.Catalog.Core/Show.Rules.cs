namespace CinemaBooking.Catalog.Core.Entities;

public partial class Show
{
    /// <summary>Needs the movie to be loaded.</summary>
    public DateTime EndsAt => StartsAt.AddMinutes(Movie.DurationMinutes);

    public bool Overlaps(DateTime from, DateTime to) => StartsAt < to && from < EndsAt;
}