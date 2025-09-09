using System.ComponentModel.DataAnnotations;
using TripEnjoy.Domain.Account.Entities;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Account
{
    public class Account : AggregateRoot<AccountId>
    {
        public string AspNetUserId { get; private set; }
        public string AccountEmail { get; private set; }
        public bool IsDeleted { get; private set; }

        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public User? User { get; private set; }
        public Partner? Partner { get; private set; }
        public Wallet? Wallet { get; private set; }

        public readonly List<RefreshToken> _refreshTokens = new();
        public IReadOnlyList<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

        private Account() : base(AccountId.CreateUnique())
        {
            // EF requires a parameterless constructor
            AspNetUserId = null!;
            AccountEmail = null!;
        }
        public Account(AccountId id, string aspNetUserId, string accountEmail) : base(id)
        {
            AspNetUserId = aspNetUserId;
            AccountEmail = accountEmail;
            IsDeleted = false;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public Result AddUserInformation(string fullName)
        {
             var userResult = Domain.Account.Entities.User.Create(this.Id, fullName);
            if (userResult.IsFailure)
            {
                return Result.Failure(userResult.Errors);
            }
            User = userResult.Value;
            return Result.Success();
        }

        public Result<RefreshToken> AddRefreshToken(string token)
        {
            var refreshToken = RefreshToken.Create(Id, token);
            _refreshTokens.Add(refreshToken);
            UpdatedAt = DateTime.UtcNow;
            return Result<RefreshToken>.Success(refreshToken);
        }


        public void MarkAsDeleted()
        {
            IsDeleted = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public Result UpdateEmail(string newEmail)
        {
            var expression = new EmailAddressAttribute();
            if (!expression.IsValid(newEmail))
            {
                return Result<Account>.Failure(DomainError.Account.InvalidEmail);
            }
            AccountEmail = newEmail;
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }

        public static Result<Account> Create(string aspNetUserId, string accountEmail)
        {
            var expression = new EmailAddressAttribute();
            if (!expression.IsValid(accountEmail))
            {
                return Result<Account>.Failure(DomainError.Account.InvalidEmail);
            }
            return Result<Account>.Success(new Account(AccountId.CreateUnique(), aspNetUserId, accountEmail));
        }

        public void AddWallet(AccountId accountId)
        {
            if (Wallet == null)
            {
                Wallet = new Wallet(WalletId.CreateUnique(), accountId);
            }
            Wallet = Wallet;
        }
    }
}
