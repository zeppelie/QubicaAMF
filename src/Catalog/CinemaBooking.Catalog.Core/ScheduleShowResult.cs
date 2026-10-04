using CinemaBooking.Catalog.Core.Entities;

namespace CinemaBooking.Catalog.Core;

public enum ScheduleShowError
{
    MovieNotFound,
    HallNotFound,
    StartsInThePast,
    HallBusy
}

public sealed record ScheduleShowResult(Show? Show, ScheduleShowError? Error)
{
    public static ScheduleShowResult Scheduled(Show show) => new(show, null);

    public static ScheduleShowResult Failed(ScheduleShowError error) => new(null, error);
}
