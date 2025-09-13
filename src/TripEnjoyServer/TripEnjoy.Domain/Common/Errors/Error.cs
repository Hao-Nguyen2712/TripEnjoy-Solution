

namespace TripEnjoy.Domain.Common.Errors
{
    public record Error
    {
        public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
        public static readonly Error NullValue = new("Error.NullValue", "The value is null.", ErrorType.Failure);

        public string Code { get; }
        public string Description { get; }
        public ErrorType Type { get; }

        public Error(string code, string description, ErrorType type)
        {
            Code = code;
            Description = description;
            Type = type;
        }

    }
    public enum ErrorType
    {
        Failure,
        Validation,
        NotFound,
        Conflict,
        Unauthorized,
        Forbidden
    }
}