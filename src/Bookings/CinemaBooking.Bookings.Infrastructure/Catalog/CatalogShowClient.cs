using System.Net;
using System.Net.Http.Json;
using CinemaBooking.Bookings.Core;

namespace CinemaBooking.Bookings.Infrastructure.Catalog;

public sealed class CatalogShowClient(HttpClient http) : IShowCatalog
{
    public async Task<ShowInfo?> FindShowAsync(int showId, CancellationToken cancellationToken)
    {
        using var response = await http.GetAsync($"shows/{showId}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        var show = await response.Content.ReadFromJsonAsync<CatalogShow>(cancellationToken);
        var seats = await http.GetFromJsonAsync<List<HallSeat>>($"shows/{showId}/seats", cancellationToken);

        return new ShowInfo(showId, show!.StartsAt.UtcDateTime, seats!);
    }

    private sealed record CatalogShow(DateTimeOffset StartsAt);
}
