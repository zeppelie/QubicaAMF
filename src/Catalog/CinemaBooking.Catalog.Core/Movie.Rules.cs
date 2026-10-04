namespace CinemaBooking.Catalog.Core.Entities;

public partial class Movie
{
    public static Movie Create(string title, int durationMinutes) =>
        new() { Title = title.Trim(), DurationMinutes = durationMinutes };
}