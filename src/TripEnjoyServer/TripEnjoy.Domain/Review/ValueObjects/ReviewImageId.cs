using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Review.ValueObjects;

public sealed class ReviewImageId : ValueObject
{
    public Guid Value { get; private set; }

    private ReviewImageId(Guid value)
    {
        Value = value;
    }

    public static ReviewImageId CreateUnique()
    {
        return new ReviewImageId(Guid.NewGuid());
    }

    public static ReviewImageId Create(Guid value)
    {
        return new ReviewImageId(value);
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
