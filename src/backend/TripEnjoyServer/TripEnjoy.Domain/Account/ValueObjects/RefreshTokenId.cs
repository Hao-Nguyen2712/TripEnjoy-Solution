using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Account.ValueObjects
{
    public class RefreshTokenId : ValueObject
    {
        public Guid Id { get; private set; }
        public RefreshTokenId(Guid id)
        {
            Id = id;
        }
        public static RefreshTokenId Create(Guid id)
        {
            return new RefreshTokenId(id);
        }
        public static RefreshTokenId CreateUnique()
        {
            return new RefreshTokenId(Guid.NewGuid());
        }
        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
        }
    }
}
