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

            public static readonly Error RoleMismatch = new(
                "Account.RoleMismatch",
                "Account role does not match the expected role for this operation.",
                ErrorType.Forbidden);

            public static readonly Error UnauthorizedRole = new(
                "Account.UnauthorizedRole", 
                "This account is not authorized to use this login method.",
                ErrorType.Forbidden);

            public static readonly Error InvalidOrExpiredResetToken = new(
                "Account.InvalidOrExpiredResetToken",
                "The password reset token is invalid or has expired.",
                ErrorType.Validation);
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

            public static readonly Error NotFound = new(
                "Property.NotFound",
                "The property was not found.",
                ErrorType.NotFound
            );

            public static readonly Error ImageNotFound = new(
                "Property.ImageNotFound",
                "The specified image was not found on this property.",
                ErrorType.NotFound
            );

            public static readonly Error InvalidImageUrl = new(
                "Property.InvalidImageUrl",
                "The image URL format is invalid and cannot be processed.",
                ErrorType.Validation
            );

            public static readonly Error CloudinaryDeletionFailed = new(
                "Property.CloudinaryDeletionFailed",
                "Failed to delete the image from cloud storage.",
                ErrorType.Failure
            );
        }

        public static class Partner
        {
            public static readonly Error DuplicateDocumentType = new(
                "Partner.DuplicateDocumentType",
                "A document of this type has already been uploaded.",
                ErrorType.Conflict);

            public static readonly Error CompanyNameRequired = new(
                "Partner.CompanyNameRequired",
                "Company name is required for partner registration.",
                ErrorType.Validation);

            public static readonly Error CompanyNameAlreadyExists = new(
                "Partner.CompanyNameAlreadyExists",
                "A partner with this company name already exists.",
                ErrorType.Conflict);

            public static readonly Error InvalidDocumentType = new(
                "Partner.InvalidDocumentType",
                "The specified document type is not supported.",
                ErrorType.Validation);

            public static readonly Error MissingRequiredDocuments = new(
                "Partner.MissingRequiredDocuments",
                "All required documents must be approved before partner approval.",
                ErrorType.Validation);

            public static readonly Error InvalidStatusTransition = new(
                "Partner.InvalidStatusTransition",
                "The partner status cannot be changed from the current state.",
                ErrorType.Failure);
        }

        public static class Authentication
        {
            public static readonly Error Unauthorized = new(
                "Authentication.Unauthorized",
                "You are not authorized to perform this action.",
                ErrorType.Unauthorized);

            public static readonly Error Forbidden = new(
                "Authentication.Forbidden",
                "You do not have permission to perform this action.",
                ErrorType.Forbidden);
        }

        public static class Booking
        {
            public static readonly Error NotFound = new(
                "Booking.NotFound",
                "The booking was not found.",
                ErrorType.NotFound);

            public static readonly Error InvalidCheckInDate = new(
                "Booking.InvalidCheckInDate",
                "Check-in date cannot be in the past.",
                ErrorType.Validation);

            public static readonly Error InvalidCheckOutDate = new(
                "Booking.InvalidCheckOutDate",
                "Check-out date must be after check-in date.",
                ErrorType.Validation);

            public static readonly Error InvalidGuestCount = new(
                "Booking.InvalidGuestCount",
                "Number of guests must be greater than zero.",
                ErrorType.Validation);

            public static readonly Error InvalidTotalPrice = new(
                "Booking.InvalidTotalPrice",
                "Total price cannot be negative.",
                ErrorType.Validation);

            public static readonly Error InvalidStatusTransition = new(
                "Booking.InvalidStatusTransition",
                "Invalid booking status transition.",
                ErrorType.Failure);

            public static readonly Error CannotCancelBooking = new(
                "Booking.CannotCancelBooking",
                "This booking cannot be cancelled.",
                ErrorType.Failure);

            public static readonly Error CheckInTooEarly = new(
                "Booking.CheckInTooEarly",
                "Check-in date has not arrived yet.",
                ErrorType.Failure);

            public static readonly Error Unauthorized = new(
                "Booking.Unauthorized",
                "You are not authorized to access this booking.",
                ErrorType.Unauthorized);

            // BookingDetail errors
            public static readonly Error InvalidRoomQuantity = new(
                "Booking.InvalidRoomQuantity",
                "Room quantity must be greater than zero.",
                ErrorType.Validation);

            public static readonly Error InvalidNights = new(
                "Booking.InvalidNights",
                "Number of nights must be greater than zero.",
                ErrorType.Validation);

            public static readonly Error InvalidPricePerNight = new(
                "Booking.InvalidPricePerNight",
                "Price per night cannot be negative.",
                ErrorType.Validation);

            public static readonly Error InvalidDiscountAmount = new(
                "Booking.InvalidDiscountAmount",
                "Discount amount cannot be negative.",
                ErrorType.Validation);
        }

        public static class Payment
        {
            public static readonly Error NotFound = new(
                "Payment.NotFound",
                "The payment was not found.",
                ErrorType.NotFound);

            public static readonly Error InvalidAmount = new(
                "Payment.InvalidAmount",
                "Payment amount must be greater than zero.",
                ErrorType.Validation);

            public static readonly Error InvalidStatusTransition = new(
                "Payment.InvalidStatusTransition",
                "Invalid payment status transition.",
                ErrorType.Failure);

            public static readonly Error InvalidTransactionId = new(
                "Payment.InvalidTransactionId",
                "Transaction ID is required.",
                ErrorType.Validation);

            public static readonly Error CannotFailCompletedPayment = new(
                "Payment.CannotFailCompletedPayment",
                "Cannot mark a completed or refunded payment as failed.",
                ErrorType.Failure);

            public static readonly Error CanOnlyRefundSuccessfulPayment = new(
                "Payment.CanOnlyRefundSuccessfulPayment",
                "Only successful payments can be refunded.",
                ErrorType.Failure);

            public static readonly Error CannotCancelCompletedPayment = new(
                "Payment.CannotCancelCompletedPayment",
                "Cannot cancel a completed or refunded payment.",
                ErrorType.Failure);
        }

        public static class RoomType
        {
            public static readonly Error NotFound = new(
                "RoomType.NotFound",
                "The room type was not found.",
                ErrorType.NotFound);

            public static readonly Error NameIsRequired = new(
                "RoomType.NameIsRequired",
                "Room type name is required.",
                ErrorType.Validation);

            public static readonly Error InvalidCapacity = new(
                "RoomType.InvalidCapacity",
                "Room capacity must be greater than zero.",
                ErrorType.Validation);

            public static readonly Error InvalidBasePrice = new(
                "RoomType.InvalidBasePrice",
                "Base price must be greater than zero.",
                ErrorType.Validation);

            public static readonly Error InvalidTotalQuantity = new(
                "RoomType.InvalidTotalQuantity",
                "Total quantity must be greater than zero.",
                ErrorType.Validation);

            public static readonly Error ImageNotFound = new(
                "RoomType.ImageNotFound",
                "The specified image was not found on this room type.",
                ErrorType.NotFound);

            public static readonly Error Unauthorized = new(
                "RoomType.Unauthorized",
                "You are not authorized to manage this room type.",
                ErrorType.Unauthorized);
        }

        public static class RoomAvailability
        {
            public static readonly Error NotFound = new(
                "RoomAvailability.NotFound",
                "The room availability was not found.",
                ErrorType.NotFound);

            public static readonly Error InvalidAvailableQuantity = new(
                "RoomAvailability.InvalidAvailableQuantity",
                "Available quantity cannot be negative.",
                ErrorType.Validation);

            public static readonly Error InvalidPrice = new(
                "RoomAvailability.InvalidPrice",
                "Price must be greater than zero.",
                ErrorType.Validation);

            public static readonly Error InvalidDate = new(
                "RoomAvailability.InvalidDate",
                "Availability date cannot be in the past.",
                ErrorType.Validation);

            public static readonly Error DuplicateEntry = new(
                "RoomAvailability.DuplicateEntry",
                "An availability entry already exists for this room type and date.",
                ErrorType.Conflict);

            public static readonly Error InsufficientQuantity = new(
                "RoomAvailability.InsufficientQuantity",
                "Insufficient room quantity available for booking.",
                ErrorType.Failure);
        }

        public static class RoomPromotion
        {
            public static readonly Error NotFound = new(
                "RoomPromotion.NotFound",
                "The room promotion was not found.",
                ErrorType.NotFound);

            public static readonly Error InvalidDiscountPercent = new(
                "RoomPromotion.InvalidDiscountPercent",
                "Discount percentage must be between 0 and 100.",
                ErrorType.Validation);

            public static readonly Error InvalidDiscountAmount = new(
                "RoomPromotion.InvalidDiscountAmount",
                "Discount amount must be greater than zero.",
                ErrorType.Validation);

            public static readonly Error BothDiscountTypesDefined = new(
                "RoomPromotion.BothDiscountTypesDefined",
                "A promotion can have either a discount percentage OR a discount amount, not both.",
                ErrorType.Validation);

            public static readonly Error NoDiscountDefined = new(
                "RoomPromotion.NoDiscountDefined",
                "A promotion must have either a discount percentage or a discount amount.",
                ErrorType.Validation);

            public static readonly Error InvalidDateRange = new(
                "RoomPromotion.InvalidDateRange",
                "End date must be after start date.",
                ErrorType.Validation);

            public static readonly Error PromotionExpired = new(
                "RoomPromotion.PromotionExpired",
                "The promotion has expired.",
                ErrorType.Failure);
        }
    }
}