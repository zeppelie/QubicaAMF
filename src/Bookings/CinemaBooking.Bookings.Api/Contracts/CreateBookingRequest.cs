using System.ComponentModel.DataAnnotations;

namespace CinemaBooking.Bookings.Api.Contracts;

public sealed record CreateBookingRequest([Required, MinLength(1)] IReadOnlyList<BookingItemRequest> Items);
