using TripEnjoy.Domain.Account;

namespace TripEnjoy.Application.Interfaces.Persistence
{
    public interface IAccountRepository : IGenericRepository<Account>
    {
        Task<Account?> FindByAspNetUserIdAsync(string aspNetUserId);
    }
}
