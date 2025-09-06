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
    }
}