using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Review.ValueObjects;

public sealed class ReviewId : ValueObject
{
    public Guid Value { get; private set; }

    private ReviewId(Guid value)
    {
        Value = value;
    }

    public static ReviewId CreateUnique()
    {
        return new ReviewId(Guid.NewGuid());
    }

    public static ReviewId Create(Guid value)
    {
        return new ReviewId(value);
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
