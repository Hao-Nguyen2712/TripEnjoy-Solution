using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Account.ValueObjects
{
    public class BlackListTokenId : ValueObject
    {
        public Guid Id { get; private set; }

        /// <summary>
        /// Initializes a new <see cref="BlackListTokenId"/> with the specified identifier.
        /// </summary>
        /// <param name="id">The GUID value to use as the blacklisted token identifier.</param>
        public BlackListTokenId(Guid id)
        {
            Id = id;
        }
        /// <summary>
        /// Creates a BlackListTokenId value object from the specified GUID.
        /// </summary>
        /// <param name="id">The GUID to use as the identifier.</param>
        /// <returns>A new <see cref="BlackListTokenId"/> initialized with <paramref name="id"/>.</returns>
        public static BlackListTokenId Create(Guid id)
        {
            return new BlackListTokenId(id);
        }
        /// <summary>
        /// Creates a new <see cref="BlackListTokenId"/> initialized with a freshly generated GUID.
        /// </summary>
        /// <returns>A <see cref="BlackListTokenId"/> containing a unique identifier.</returns>
        public static BlackListTokenId CreateUnique()
        {
            return new BlackListTokenId(Guid.NewGuid());
        }
        /// <summary>
        /// Returns the sequence of values that define this value object's equality.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{Object}"/> containing the <see cref="Id"/> used for equality comparison.</returns>
        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
        }
    }
}
