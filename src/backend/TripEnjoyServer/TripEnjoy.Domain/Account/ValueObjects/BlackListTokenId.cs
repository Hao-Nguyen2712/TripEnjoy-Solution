using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Account.ValueObjects
{
    public class BlackListTokenId : ValueObject
    {
        public Guid Id { get; private set; }

        public BlackListTokenId(Guid id)
        {
            Id = id;
        }
        public static BlackListTokenId Create(Guid id)
        {
            return new BlackListTokenId(id);
        }
        public static BlackListTokenId CreateUnique()
        {
            return new BlackListTokenId(Guid.NewGuid());
        }
        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
        }
    }
}
