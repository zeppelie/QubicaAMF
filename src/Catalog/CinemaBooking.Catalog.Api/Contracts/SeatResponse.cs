namespace CinemaBooking.Catalog.Api.Contracts;

public sealed record SeatResponse(int SeatId, string RowLabel, int SeatNumber);