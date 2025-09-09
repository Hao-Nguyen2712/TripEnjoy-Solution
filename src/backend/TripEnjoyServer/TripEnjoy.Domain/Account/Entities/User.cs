using System.ComponentModel.DataAnnotations;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Account.Entities
{
    public class User : Entity<UserId>
    {
        public AccountId AccountId { get; private set; }
        public string? FullName { get; private set; }
        public string? PhoneNumber { get; private set; }
        public string? Address { get; private set; }
        public DateOnly? DateOfBirth { get; private set; }


        private User() : base(UserId.CreateUnique())
        {
        }

        public User(UserId id, AccountId accountId, string? fullName, string? phoneNumber = null, string? address = null, DateOnly? dateOfBirth = null) : base(id)
        {
            AccountId = accountId;
            FullName = fullName;
            PhoneNumber = phoneNumber;
            Address = address;
            DateOfBirth = dateOfBirth;
        }

        public Result UpdateProfile(string? fullName, string? phoneNumber, string? address, DateOnly? dateOfBirth)
        {
            FullName = fullName;
            if (phoneNumber != null)
            {
                var expression = new PhoneAttribute();
                if (!expression.IsValid(phoneNumber))
                {
                    return Result.Failure(DomainError.User.InvalidPhoneNumber);
                }
            }
            PhoneNumber = phoneNumber;
            Address = address;
            if (dateOfBirth != null && dateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
            {
                return Result.Failure(DomainError.User.InvalidDateOfBirth);
            }
            DateOfBirth = dateOfBirth;
            return Result.Success();
        }

        /// <summary>
        /// Calculates the user's age in years based on the current UTC date and the stored DateOfBirth.
        /// </summary>
        /// <returns>
        /// A <c>Result&lt;int&gt;</c> containing the age in years on success, or a failure with <see cref="DomainError.User.InvalidDateOfBirth"/> if DateOfBirth is null.
        /// Note: the calculation uses only the year difference (no adjustment for month/day).
        /// </returns>
        public Result<int> GetAge()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (DateOfBirth == null)
            {
                return Result<int>.Failure(DomainError.User.InvalidDateOfBirth);
            }
            var age = today.Year - DateOfBirth.Value.Year;
            return Result<int>.Success(age);
        }
         /// <summary>
        /// Creates a new User with a generated unique UserId after validating required fields.
        /// </summary>
        /// <param name="accountId">The account identifier that will own the user.</param>
        /// <param name="fullName">The user's full name. Must not be null, empty, or whitespace.</param>
        /// <param name="phoneNumber">Optional phone number.</param>
        /// <param name="address">Optional address.</param>
        /// <param name="dateOfBirth">Optional date of birth.</param>
        /// <returns>
        /// A <see cref="Result{User}"/> that is successful containing the created User when validation passes;
        /// otherwise a failure result with <see cref="DomainError.User.FullNameRequired"/> if <paramref name="fullName"/> is null/empty/whitespace.
        /// </returns>
        public static Result<User> Create(AccountId accountId, string fullName, string? phoneNumber = null, string? address = null, DateOnly? dateOfBirth = null)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return Result<User>.Failure(DomainError.User.FullNameRequired);
            }
            
            var user = new User(UserId.CreateUnique(), accountId, fullName, phoneNumber, address, dateOfBirth);
            return Result<User>.Success(user);
        }
    }
}