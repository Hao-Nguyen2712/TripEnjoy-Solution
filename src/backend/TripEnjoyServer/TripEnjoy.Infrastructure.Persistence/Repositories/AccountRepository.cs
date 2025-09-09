using Microsoft.EntityFrameworkCore;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Account;

namespace TripEnjoy.Infrastructure.Persistence.Repositories
{
    public class AccountRepository : GenericRepository<Account>, IAccountRepository
    {
        /// <summary>
        /// Initializes a new instance of <see cref="AccountRepository"/> using the provided <see cref="TripEnjoyDbContext"/>.
        —
        /// </summary>
        public AccountRepository(TripEnjoyDbContext dbContext) : base(dbContext)
        {
        }

        /// <summary>
        /// Asynchronously finds the account with the specified AspNet user ID.
        /// </summary>
        /// <param name="aspNetUserId">The AspNet Identity user ID to match against Account.AspNetUserId.</param>
        /// <returns>
        /// A task that resolves to the matching <see cref="Account"/> or <c>null</c> if no account has the given AspNet user ID.
        /// </returns>
        public async Task<Account?> FindByAspNetUserIdAsync(string aspNetUserId)
        {
            return await _dbContext.Accounts
                .FirstOrDefaultAsync(a => a.AspNetUserId == aspNetUserId);
        }
    }
}
