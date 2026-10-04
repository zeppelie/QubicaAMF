using Microsoft.AspNetCore.Diagnostics;

namespace CinemaBooking.Bookings.Api;

public sealed class CatalogUnavailableHandler(IProblemDetailsService problemDetails, ILogger<CatalogUnavailableHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not (HttpRequestException or TaskCanceledException { InnerException: TimeoutException }))
            return false;

        logger.LogError(exception, "The Catalog service did not answer");

        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = { Detail = "The Catalog service is not answering, try again in a moment." }
        });
    }
}
