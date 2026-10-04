using CinemaBooking.Bookings.Api.Contracts;
using CinemaBooking.Bookings.Core;
using CinemaBooking.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.Bookings.Api.Controllers;

[ApiController]
[Authorize]
[Route("bookings")]
public sealed class BookingsController(IBookingService bookings, ILogger<BookingsController> logger) : ControllerBase
{
    /// <summary>Books seats for one or more shows: give the seat ids to choose them, or a quantity to let the system pick.</summary>
    [HttpPost]
    [ProducesResponseType<BookingResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await bookings.BookAsync(userId, [.. request.Items.Select(item => item.ToSeatRequest())], cancellationToken);

        if (result.Booking is null)
        {
            logger.LogWarning("Booking of user {UserId} refused: {Error} on show {ShowId}", userId, result.Error, result.ShowId);
            return Refused(result);
        }

        logger.LogInformation(
            "User {UserId} made booking {BookingId} with {SeatCount} seats",
            userId, result.Booking.BookingId, result.Booking.BookedSeats.Count);
        return CreatedAtAction(nameof(GetById), new { bookingId = result.Booking.BookingId }, result.Booking.ToResponse());
    }

    /// <summary>Lists the bookings of the user, newest first.</summary>
    [HttpGet]
    public async Task<IReadOnlyList<BookingResponse>> List(CancellationToken cancellationToken)
    {
        var mine = await bookings.ListAsync(User.GetUserId(), cancellationToken);
        return [.. mine.Select(booking => booking.ToResponse())];
    }

    /// <summary>Returns one booking of the user.</summary>
    [HttpGet("{bookingId:int}")]
    [ProducesResponseType<BookingResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int bookingId, CancellationToken cancellationToken)
    {
        var booking = await bookings.FindAsync(User.GetUserId(), bookingId, cancellationToken);
        return booking is null ? BookingNotFound(bookingId) : Ok(booking.ToResponse());
    }

    /// <summary>Cancels a booking and frees its seats.</summary>
    [HttpDelete("{bookingId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(int bookingId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var error = await bookings.CancelAsync(userId, bookingId, cancellationToken);

        switch (error)
        {
            case CancelBookingError.BookingNotFound:
                return BookingNotFound(bookingId);
            case CancelBookingError.AlreadyCancelled:
                return Problem($"Booking {bookingId} is already cancelled.", statusCode: StatusCodes.Status409Conflict);
        }

        logger.LogInformation("User {UserId} cancelled booking {BookingId}", userId, bookingId);
        return NoContent();
    }

    private ObjectResult Refused(BookingResult result) => result.Error switch
    {
        BookingError.InvalidRequest => Problem(
            "Each item needs either the seat ids or a quantity greater than zero.", statusCode: StatusCodes.Status400BadRequest),
        BookingError.SeatNotFound => Problem(
            $"One of the seats does not belong to the hall of show {result.ShowId}.", statusCode: StatusCodes.Status400BadRequest),
        BookingError.ShowNotFound => Problem(
            $"Show {result.ShowId} does not exist.", statusCode: StatusCodes.Status404NotFound),
        BookingError.ShowAlreadyStarted => Problem(
            $"Show {result.ShowId} has already started.", statusCode: StatusCodes.Status409Conflict),
        BookingError.NotEnoughSeats => Problem(
            $"Show {result.ShowId} does not have enough free seats.", statusCode: StatusCodes.Status409Conflict),
        _ => Problem("One of the seats has just been taken.", statusCode: StatusCodes.Status409Conflict)
    };

    private ObjectResult BookingNotFound(int bookingId) =>
        Problem($"Booking {bookingId} does not exist.", statusCode: StatusCodes.Status404NotFound);
}
