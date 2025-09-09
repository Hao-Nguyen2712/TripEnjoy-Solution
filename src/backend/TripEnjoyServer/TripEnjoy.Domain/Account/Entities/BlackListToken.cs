using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Account.Entities
{
    public class BlackListToken : Entity<BlackListTokenId>
    {
        public AccountId AccountId { get; private set; }
        public string Token { get; private set; }
        public DateTime Expiration { get; private set; }
        public DateTime CreatedAt { get; private set; }
        private BlackListToken() : base(BlackListTokenId.CreateUnique())
        {
            AccountId = null!;
            Token = null!;
        }

        public BlackListToken(BlackListTokenId id, AccountId accountId, string token, DateTime expiration) : base(id)
        {
            AccountId = accountId;
            Token = token;
            Expiration = expiration;
            CreatedAt = DateTime.UtcNow;
        }

        public static BlackListToken Create(AccountId accountId, string token, DateTime expiration)
        {
            return new BlackListToken(BlackListTokenId.CreateUnique(), accountId, token, expiration);
        }
    }
}
