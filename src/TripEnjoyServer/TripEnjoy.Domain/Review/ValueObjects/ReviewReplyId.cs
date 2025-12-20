using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Review.ValueObjects;

public sealed class ReviewReplyId : ValueObject
{
    public Guid Value { get; private set; }

    private ReviewReplyId(Guid value)
    {
        Value = value;
    }

    public static ReviewReplyId CreateUnique()
    {
        return new ReviewReplyId(Guid.NewGuid());
    }

    public static ReviewReplyId Create(Guid value)
    {
        return new ReviewReplyId(value);
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
