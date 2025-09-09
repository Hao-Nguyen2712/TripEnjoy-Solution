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

        /// <summary>
        /// Parameterless constructor intended for ORM/serializer use only.
        /// Initializes the entity with a newly created unique <see cref="RefreshTokenId"/> and sets non-null placeholders
        /// for required properties so the object can be hydrated by infrastructure.
        /// </summary>
        private RefreshToken() : base(RefreshTokenId.CreateUnique())
        {
            AccountId = null!;
            Token = null!;
        }
        /// <summary>
        /// Initializes a new RefreshToken instance with the provided identifiers and token value.
        /// </summary>
        /// <remarks>
        /// Sets CreatedAt to the current UTC time, ExpireDate to seven days from now, and IsUsed to false.
        /// </remarks>
        /// <param name="id">The unique identifier for the refresh token.</param>
        /// <param name="accountId">The associated account's identifier.</param>
        /// <param name="token">The refresh token string.</param>
        /// <param name="revokeAt">Optional UTC timestamp indicating when the token was revoked; if null the token is not revoked.</param>
        private RefreshToken(RefreshTokenId id, AccountId accountId, string token, DateTime? revokeAt = null) : base(id)
        {
            AccountId = accountId;
            Token = token;
            RevokeAt = revokeAt;
            ExpireDate = DateTime.UtcNow.AddDays(7);
            CreatedAt = DateTime.UtcNow;
            IsUsed = false;
        }

        /// <summary>
        /// Creates a new RefreshToken for the specified account using a newly generated unique identifier.
        /// </summary>
        /// <param name="accountId">The identifier of the account that owns the refresh token.</param>
        /// <param name="token">The refresh token string value.</param>
        /// <returns>A newly created <see cref="RefreshToken"/> instance (with a unique id, creation and expiration timestamps).</returns>
        public static RefreshToken Create(AccountId accountId, string token)
        {
            return new RefreshToken(RefreshTokenId.CreateUnique(), accountId, token);
        }

        /// <summary>
        /// Marks the refresh token as revoked.
        /// </summary>
        /// <remarks>
        /// Sets <see cref="RevokeAt"/> to the current UTC time and sets <see cref="IsUsed"/> to <c>true</c>.
        /// Calling this method again will update the revocation timestamp to the new UTC time.
        /// </remarks>
        public void Revoke()
        {
            RevokeAt = DateTime.UtcNow;
            IsUsed = true;
        }
    }
}
