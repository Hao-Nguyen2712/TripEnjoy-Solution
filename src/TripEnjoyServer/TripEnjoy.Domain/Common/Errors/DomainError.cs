namespace TripEnjoy.Domain.Common.Errors
{
    public static class DomainError
    {
        public static class Account
        {
            public static readonly Error InvalidEmail = new(
                "Account.InvalidEmail",
                "The email format is invalid.",
                ErrorType.Validation);

            public static readonly Error DuplicateEmail = new(
                "Account.DuplicateEmail",
                "The email is already in use.",
                ErrorType.Conflict);

            public static readonly Error LoginFailed = new(
                "Account.LoginFailed",
                "Invalid email or password.",
                ErrorType.Failure);

            public static readonly Error LockedOut = new(
                "Account.LockedOut",
                "This account is locked out.",
                ErrorType.Failure);

            public static readonly Error EmailNotConfirmed = new(
                "Account.EmailNotConfirmed",
                "The email for this account is not confirmed.",
                ErrorType.Failure);

            public static readonly Error TwoFactorRequired = new(
                "Account.TwoFactorRequired",
                "Two-factor authentication is required for this account.",
                ErrorType.Failure);

            public static readonly Error NotFound = new(
                "Account.NotFound",
                "The account was not found.",
                ErrorType.NotFound);

            public static readonly Error InvalidOtp = new(
                "Account.InvalidOtp",
                "The one-time password provided is invalid or has expired.",
                ErrorType.Validation);

            public static readonly Error InvalidToken = new(
            "Account.InvalidToken",
            "The token is invalid.",
            ErrorType.Unauthorized
        );

            public static readonly Error AlreadyActivated = new(
                "Account.AlreadyActivated",
                "The account has already been activated.",
                ErrorType.Failure
            );
            public static readonly Error AlreadyBanned = new(
                "Account.AlreadyBanned",
                "The account has already been banned.",
                ErrorType.Failure
            );
            public static readonly Error AlreadyLocked = new(
                "Account.AlreadyLocked",
                "The account has already been locked.",
                ErrorType.Failure
            );
            public static readonly Error AlreadyDeleted = new(
                "Account.AlreadyDeleted",
                "The account has already been deleted.",
                ErrorType.Failure
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

            public static readonly Error FullNameRequired = new(
                "User.FullNameRequired",
                "The full name is required.",
                ErrorType.Validation
            );

            public static readonly Error NotFound = new(
                "User.NotFound",
                "The user was not found.",
                ErrorType.NotFound
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

        public static class RefreshToken
        {
            public static readonly Error InvalidToken = new(
                "RefreshToken.InvalidToken",
                "The refresh token is invalid.",
                ErrorType.Failure
            );
            public static readonly Error RefreshTokenNotFound = new(
                "RefreshToken.RefreshTokenNotFound",
                "The refresh token was not found.",
                ErrorType.Failure
            );
            public static readonly Error RefreshTokenInvalidated = new(
                "RefreshToken.RefreshTokenInvalidated",
                "The refresh token has been invalidated.",
                ErrorType.Failure
            );
        }

        public static class Property
        {
            public static readonly Error NameIsRequired = new(
                "Property.NameIsRequired",
                "The name is required.",
                ErrorType.Validation
            );
        }
    }
}