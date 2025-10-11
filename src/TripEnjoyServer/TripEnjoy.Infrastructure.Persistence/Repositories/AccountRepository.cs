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
            _logger.LogInformation("Searching for Account with AspNetUserId: {AspNetUserId}", aspNetUserId);
            
            var account = await _dbContext.Accounts
                .FirstOrDefaultAsync(a => a.AspNetUserId == aspNetUserId);
                
            if (account == null)
            {
                _logger.LogWarning("Account not found with AspNetUserId: {AspNetUserId}", aspNetUserId);
                
                // Debug: Let's see what accounts exist
                var allAccounts = await _dbContext.Accounts.Take(10).ToListAsync();
                _logger.LogInformation("Found {Count} accounts in database:", allAccounts.Count);
                foreach (var acc in allAccounts)
                {
                    _logger.LogInformation("Account - ID: {Id}, AspNetUserId: {AspNetUserId}, Email: {Email}", 
                        acc.Id, acc.AspNetUserId, acc.AccountEmail);
                }
            }
            else
            {
                _logger.LogInformation("Found Account with ID: {AccountId}", account.Id);
            }
            
            return account;
        }

        public async Task<Account?> FindByAspNetUserIdWithTokensAsync(string aspNetUserId)
        {
            return await _dbContext.Accounts
                             .Include(a => a.RefreshTokens)
                             .FirstOrDefaultAsync(a => a.AspNetUserId == aspNetUserId);
        }

        public async Task<Account?> FindByAspNetUserIdWithBlackListTokensAsync(string aspNetUserId)
        {
            return await _dbContext.Accounts
                            .Include(a => a.BlackListTokens)
                            .Include(a => a.RefreshTokens)
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

        public async Task<Account?> FindByAspNetUserIdAsyncIncludePartnersOrUser(string aspNetUserId)
        {
            return await _dbContext.Accounts
                .Include(a => a.Partner)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AspNetUserId == aspNetUserId);
        }

        /// <summary>
        /// Retrieves an Account by its AccountId with Partner and PartnerDocuments included
        /// </summary>
        public async Task<Account?> GetByAccountIdAsync(AccountId accountId)
        {
            return await _dbContext.Accounts
                .Include(a => a.Partner)
                    .ThenInclude(p => p!.PartnerDocuments)
                .FirstOrDefaultAsync(a => a.Id == accountId);
        }
    }
}