namespace TripEnjoy.Domain.Common.Errors
{
    public static class DomainError
    {
        public static class Account
        {
            public static readonly Error InvalidEmail = new(
                "Account.InvalidEmail",
                "The email address is invalid.",
                ErrorType.Validation
            );
        }

        public static class User
        {
            public static readonly Error InvalidPhoneNumber = new(
                "User.InvalidPhoneNumber",
                "The phone number is invalid.",
                ErrorType.Validation
            );

            public static readonly Error InvalidDateOfBirth = new(
                "User.InvalidDateOfBirth",
                "The date of birth is invalid.",
                ErrorType.Validation
            );
        }

        public static class Wallet
        {
            public static readonly Error InsufficientFunds = new(
                "Wallet.InsufficientFunds",
                "The wallet has insufficient funds for this operation.",
                ErrorType.Failure
            );
            public static readonly Error InvalidTransactionAmount = new(
                "Wallet.InvalidTransactionAmount",
                "The transaction amount must be greater than zero.",
                ErrorType.Validation
            );
        }
    }
}