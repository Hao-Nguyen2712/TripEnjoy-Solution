using TripEnjoy.Domain.Account;

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
    }
}
