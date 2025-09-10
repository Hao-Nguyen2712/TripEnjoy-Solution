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
        /// <summary>
        /// Parameterless constructor used by ORMs and serializers to materialize the entity.
        /// </summary>
        /// <remarks>
        /// Initializes the base entity with a new unique <see cref="BlackListTokenId"/> and sets property placeholders
        /// for <see cref="AccountId"/> and <see cref="Token"/>. Use the public constructors or <see cref="Create"/> factory
        /// to create fully-initialized instances.
        /// </remarks>
        private BlackListToken() : base(BlackListTokenId.CreateUnique())
        {
            AccountId = null!;
            Token = null!;
        }

        /// <summary>
        /// Initializes a new <see cref="BlackListToken"/> with the specified identity, account, token value and expiration.
        /// </summary>
        /// <param name="id">The unique identifier for the blacklist token.</param>
        /// <param name="accountId">The account that owns this blacklisted token.</param>
        /// <param name="token">The token string being blacklisted.</param>
        /// <param name="expiration">The UTC expiration time after which the token is no longer considered.</param>
        /// <remarks>
        /// The <see cref="CreatedAt"/> timestamp is set to <see cref="DateTime.UtcNow"/> when the instance is created.
        /// </remarks>
        public BlackListToken(BlackListTokenId id, AccountId accountId, string token, DateTime expiration) : base(id)
        {
            AccountId = accountId;
            Token = token;
            Expiration = expiration;
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Create a new BlackListToken with a generated unique identifier.
        /// </summary>
        /// <param name="accountId">The account identifier to which the blacklisted token belongs.</param>
        /// <param name="token">The token string to be blacklisted.</param>
        /// <param name="expiration">The UTC expiration time after which the token is no longer considered blacklisted.</param>
        /// <returns>A new <see cref="BlackListToken"/> instance with a generated <see cref="BlackListTokenId"/> and the specified values.</returns>
        public static BlackListToken Create(AccountId accountId, string token, DateTime expiration)
        {
            return new BlackListToken(BlackListTokenId.CreateUnique(), accountId, token, expiration);
        }
    }
}
