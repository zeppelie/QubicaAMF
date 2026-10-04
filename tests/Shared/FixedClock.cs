namespace CinemaBooking.Tests.Shared;

internal sealed class FixedClock(DateTime utcNow) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => utcNow;
}