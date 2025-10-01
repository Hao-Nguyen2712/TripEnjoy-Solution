using TripEnjoy.Domain.Account;
using TripEnjoy.Domain.Account.Entities;
using TripEnjoy.Domain.Account.ValueObjects;

namespace TripEnjoy.Application.Interfaces.Persistence
{
    public interface IAccountRepository : IGenericRepository<Account>
    {
        /// <summary>
        /// Asynchronously retrieves an Account associated with the given ASP.NET Identity user ID.
        /// </summary>
        /// <param name="aspNetUserId">The ASP.NET Identity user ID to search for.</param>
        /// <returns>A task that resolves to the matching <see cref="Account"/>, or null if no account is found.</returns>
        Task<Account?> FindByAspNetUserIdAsync(string aspNetUserId);
        Task<Account?> FindByAspNetUserIdWithTokensAsync(string aspNetUserId);
        Task<Account?> FindByAspNetUserIdWithBlackListTokensAsync(string aspNetUserId);
        Task<Partner?> FindPartnerByAccountIdAsync(AccountId accountId);
        Task<Account?> GetAccountByEmail(string email);
        Task<Account?> FindByAspNetUserIdAsyncIncludePartnersOrUser(string aspNetUserId);
        /// <summary>
        /// Retrieves an Account by its strongly-typed AccountId
        /// </summary>
        Task<Account?> GetByAccountIdAsync(AccountId accountId);
    }
}
