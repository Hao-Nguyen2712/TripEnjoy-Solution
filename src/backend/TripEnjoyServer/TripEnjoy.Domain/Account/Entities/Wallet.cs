using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Account.Entities
{
    public class Wallet : Entity<WalletId>
    {
        public AccountId AccountId { get; private set; }
        public decimal Balance { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }


        private Wallet() : base(WalletId.CreateUnique())
        {
            AccountId = null!;
        }
        public Wallet(WalletId id, AccountId accountId) : base(id)
        {
            AccountId = accountId;
            Balance = 0;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public Result Credit(decimal amount)
        {
            if (amount <= 0)
            {
                return Result.Failure(DomainError.Wallet.InvalidTransactionAmount);
            }
            Balance += amount;
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }

        public Result Debit(decimal amount)
        {
            if (amount <= 0)
            {
                return Result.Failure(DomainError.Wallet.InvalidTransactionAmount);
            }
            if (amount > Balance)
            {
                return Result.Failure(DomainError.Wallet.InsufficientFunds);
            }
            Balance -= amount;
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }
    }
}
