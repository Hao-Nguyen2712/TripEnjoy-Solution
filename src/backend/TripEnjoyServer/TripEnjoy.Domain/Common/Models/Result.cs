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
        /// <summary>
/// Creates a failed <see cref="Result"/> populated with the provided errors.
/// </summary>
/// <param name="errors">Array of <see cref="Error"/> objects describing why the operation failed.</param>
/// <returns>A <see cref="Result"/> with <see cref="Result.IsSuccess"/> set to <c>false</c> and <see cref="Result.Errors"/> containing <paramref name="errors"/>.</returns>
public static Result Failure(Error[] errors) => new(false, errors);

        /// <summary>
/// Creates a failed Result containing the specified error.
/// </summary>
/// <param name="error">The error to include in the failure result.</param>
/// <returns>A <see cref="Result"/> representing failure with the provided error.</returns>
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

        /// <summary>
/// Creates a successful Result&lt;TValue&gt; that wraps the specified value.
/// </summary>
/// <param name="value">The value produced by a successful operation.</param>
/// <returns>A <see cref="Result{TValue}"/> marked as success containing <paramref name="value"/>.</returns>
public static Result<TValue> Success(TValue value) => new(value);
        /// <summary>
/// Creates a failed <see cref="Result{TValue}"/> containing the specified errors.
/// </summary>
/// <param name="errors">Array of <see cref="Error"/> objects describing the failure(s).</param>
/// <returns>A <see cref="Result{TValue}"/> with <see cref="Result.IsSuccess"/> == <c>false</c> and its <see cref="Result.Errors"/> set to <paramref name="errors"/>.</returns>
public new static Result<TValue> Failure(Error[] errors) => new(false, errors);
        /// <summary>
/// Creates a failed <see cref="Result{TValue}"/> containing a single <see cref="Error"/>.
/// </summary>
/// <param name="error">The error describing the failure.</param>
/// <returns>A <see cref="Result{TValue}"/> representing failure with the provided error.</returns>
public new static Result<TValue> Failure(Error error) => new(false, new[] { error });

    }
}