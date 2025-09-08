using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TripEnjoy.Domain.Account.Entities;
using TripEnjoy.Domain.Common.Errors;

namespace TripEnjoy.Domain.Common.Models
{
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error[] Errors { get; }
        protected Result(bool isSuccess, Error[] errors)
        {
            IsSuccess = isSuccess;
            Errors = errors;
        }

        public static Result Success() => new(true, [Error.None]);
        public static Result Failure(Error[] errors) => new(false, errors);

        public static Result Failure(Error error) => new(false, [error]);

    }
     public class Result<TValue> : Result
    {
        private readonly TValue? _value;

        public TValue Value => IsSuccess
            ? _value!
            : throw new InvalidOperationException("The value of a failure result can not be accessed.");

        protected Result(TValue value) : base(true, new[] { Error.None })
        {
            _value = value;
        }

        protected Result(bool isSuccess, Error[] errors) : base(isSuccess, errors)
        {
            _value = default;
        }

        public static Result<TValue> Success(TValue value) => new(value);
        public new static Result<TValue> Failure(Error[] errors) => new(false, errors);
        public new static Result<TValue> Failure(Error error) => new(false, new[] { error });

        internal static Result<User> Failure(object fullNameRequired)
        {
            throw new NotImplementedException();
        }
    }
}