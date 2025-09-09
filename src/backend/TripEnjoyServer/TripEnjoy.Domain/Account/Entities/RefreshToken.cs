using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Account.Entities
{
    public class RefreshToken : Entity<RefreshTokenId>
    {

        public AccountId AccountId { get; private set; }

        public string Token { get; private set; }
        public DateTime ExpireDate { get; private set; }

        public DateTime CreatedAt { get; private set; }
        public DateTime? RevokeAt { get; private set; }

        public bool IsUsed { get; private set; }

        private RefreshToken() : base(RefreshTokenId.CreateUnique())
        {
            AccountId = null!;
            Token = null!;
        }
        private RefreshToken(RefreshTokenId id, AccountId accountId, string token, DateTime? revokeAt = null) : base(id)
        {
            AccountId = accountId;
            Token = token;
            RevokeAt = revokeAt;
            ExpireDate = DateTime.UtcNow.AddDays(7);
            CreatedAt = DateTime.UtcNow;
            IsUsed = false;
        }

        public static RefreshToken Create(AccountId accountId, string token)
        {
            return new RefreshToken(RefreshTokenId.CreateUnique(), accountId, token);
        }

        public void Revoke()
        {
            RevokeAt = DateTime.UtcNow;
            IsUsed = true;
        }
    }
}
