using CinemaBooking.Identity.Api.Accounts;
using CinemaBooking.Identity.Api.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Identity.Api.Persistence;

public sealed class UserAccountRepository(IdentityDbContext db) : IUserAccountRepository
{
    private const int UniqueConstraintViolation = 2627;

    public Task<UserAccount?> FindByNameAsync(string userName, CancellationToken cancellationToken) =>
        db.UserAccounts.AsNoTracking().FirstOrDefaultAsync(account => account.UserName == userName, cancellationToken);

    public async Task<bool> TryAddAsync(UserAccount account, CancellationToken cancellationToken)
    {
        db.UserAccounts.Add(account);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: UniqueConstraintViolation })
        {
            return false;
        }
    }
}
