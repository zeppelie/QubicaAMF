namespace CinemaBooking.Bookings.Core;

public enum BookingError
{
    InvalidRequest,
    ShowNotFound,
    ShowAlreadyStarted,
    SeatNotFound,
    SeatTaken,
    NotEnoughSeats
}