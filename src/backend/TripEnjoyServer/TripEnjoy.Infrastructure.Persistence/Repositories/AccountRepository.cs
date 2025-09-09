using Microsoft.EntityFrameworkCore;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Account;

namespace TripEnjoy.Infrastructure.Persistence.Repositories
{
    public class AccountRepository : GenericRepository<Account>, IAccountRepository
    {
        public AccountRepository(TripEnjoyDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Account?> FindByAspNetUserIdAsync(string aspNetUserId)
        {
            return await _dbContext.Accounts
                .FirstOrDefaultAsync(a => a.AspNetUserId == aspNetUserId);
        }
    }
}
