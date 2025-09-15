using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Account;
using TripEnjoy.Domain.Account.Entities;
using TripEnjoy.Domain.Account.ValueObjects;

namespace TripEnjoy.Infrastructure.Persistence.Repositories
{
    public class AccountRepository : GenericRepository<Account>, IAccountRepository
    {

        private readonly ILogger<AccountRepository> _logger;
        /// <summary>
        /// Initializes a new instance of <see cref="AccountRepository"/> using the provided <see cref="TripEnjoyDbContext"/>.
        /// </summary>
        public AccountRepository(TripEnjoyDbContext dbContext) : base(dbContext)
        {
            _logger = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
            }).CreateLogger<AccountRepository>();
        }

        public async Task<Account?> FindByAspNetUserIdAsync(string aspNetUserId)
        {
            return await _dbContext.Accounts
                .FirstOrDefaultAsync(a => a.AspNetUserId == aspNetUserId);
        }

        public async Task<Account?> FindByAspNetUserIdWithTokensAsync(string aspNetUserId)
        {
            return await _dbContext.Accounts
                             .Include(a => a.RefreshTokens)
                             .FirstOrDefaultAsync(a => a.AspNetUserId == aspNetUserId);
        }

        public async Task<Account?> FindByAspNetUserIdWithBlackListTokensAsync(string aspNetUserId)
        {
            return await _dbContext.Accounts.
                            Include(a => a.BlackListTokens)
                            .FirstOrDefaultAsync(a => a.AspNetUserId == aspNetUserId);
        }

        public Task<Partner?> FindPartnerByAccountIdAsync(AccountId accountId)
        {
            return _dbContext.Partners.FirstOrDefaultAsync(p => p.AccountId == accountId);
        }

        public async Task<Account?> GetAccountByEmail(string email)
        {
            _logger.LogInformation("Querying for account with email: {Email}", email);
            var account = await _dbContext.Accounts
                .FirstOrDefaultAsync(a => a.AccountEmail == email);

            if (account == null)
            {
                _logger.LogWarning("No account found with email: {Email}", email);
            }

            return account;
        }
    }
}