using System.ComponentModel.DataAnnotations;
using TripEnjoy.Domain.Account.Entities;
using TripEnjoy.Domain.Account.Enums;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Account
{
    public class Account : AggregateRoot<AccountId>
    {
        public string AspNetUserId { get; private set; }
        public string AccountEmail { get; private set; }
        public string Status { get; private set; }

        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public User? User { get; private set; }
        public Partner? Partner { get; private set; }
        public Wallet? Wallet { get; private set; }

        public readonly List<RefreshToken> _refreshTokens = new();
        public IReadOnlyList<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

        public readonly List<BlackListToken> _blackListTokens = new();
        public IReadOnlyList<BlackListToken> BlackListTokens => _blackListTokens.AsReadOnly();


        /// <summary>
        /// Parameterless constructor required by Entity Framework for materialization.
        /// </summary>
        private Account() : base(AccountId.CreateUnique())
        {
            // EF requires a parameterless constructor
            AspNetUserId = null!;
            AccountEmail = null!;
            Status = null!;
        }
        /// <summary>
        /// Initializes a new Account with the specified identifier, external ASP.NET user id, and email.
        /// Sets the account as active (IsDeleted = false) and records creation and last-updated timestamps (UTC).
        /// </summary>
        /// <param name="id">The aggregate identifier for the account.</param>
        /// <param name="aspNetUserId">The external ASP.NET user identifier associated with this account.</param>
        /// <param name="accountEmail">The primary email address for the account.</param>
        public Account(AccountId id, string aspNetUserId, string accountEmail) : base(id)
        {
            AspNetUserId = aspNetUserId;
            AccountEmail = accountEmail;
            Status = AccountStatusEnum.PendingVerification.ToString();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates and attaches a User entity to this Account using the provided full name.
        /// </summary>
        /// <param name="fullName">The user's full name to create the associated User entity.</param>
        /// <returns>
        /// A <see cref="Result"/> that is successful when the User was created and assigned to this Account;
        /// on failure it contains the errors produced by the User creation process.
        /// </returns>
        public Result AddUserInformation(string fullName)
        {
            var userResult = Domain.Account.Entities.User.Create(this.Id, fullName);
            if (userResult.IsFailure)
            {
                return Result.Failure(userResult.Errors);
            }
            User = userResult.Value;
            return Result.Success();
        }

        public Result AddNewUser(string? fullName, string? phoneNumber, string? address, DateOnly? dateOfBirth)
        {
            var userResult = Domain.Account.Entities.User.Create(this.Id, fullName, phoneNumber, address, dateOfBirth);
            if (userResult.IsFailure)
            {
                return Result.Failure(userResult.Errors);
            }
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }
        public Result AddNewPartner(string? companyName, string? phoneNumber, string? address, string? email)
        {
            var partnerResult = Partner.Create(this.Id, companyName, phoneNumber, address);
            if (partnerResult.IsFailure)
            {
                return Result.Failure(partnerResult.Errors);
            }
            Partner = partnerResult.Value;
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }

        public Result AddPartnerDocument(string documentType, string documentUrl)
        {
            if (Partner is null)
            {
                return Result.Failure(new Error("Account.NoPartnerProfile", "This account does not have a partner profile.", ErrorType.NotFound));
            }

            var result = Partner.AddDocument(documentType, documentUrl);
            if (result.IsSuccess)
            {
                UpdatedAt = DateTime.UtcNow;
            }

            return result;
        }

        /// <summary>
        /// Creates a new refresh token for this account, stores it in the account's token collection, and updates the account's UpdatedAt timestamp.
        /// </summary>
        /// <param name="token">The token string to create and store.</param>
        /// <returns>A successful Result containing the created <see cref="RefreshToken"/>.</returns>
        public Result<RefreshToken> AddRefreshToken(string token)
        {
            var refreshToken = RefreshToken.Create(Id, token);
            _refreshTokens.Add(refreshToken);
            UpdatedAt = DateTime.UtcNow;
            return Result<RefreshToken>.Success(refreshToken);
        }


        /// <summary>
        /// Soft-deletes the account by marking it as deleted.
        /// </summary>
        /// <remarks>
        /// Sets <see cref="IsDeleted"/> to true and updates <see cref="UpdatedAt"/> to the current UTC time.
        /// </remarks>
        public Result MarkAsDeleted()
        {
            if (Status != AccountStatusEnum.PendingVerification.ToString())
            {
                return Result.Failure(DomainError.Account.AlreadyDeleted);
            }
            Status = AccountStatusEnum.Deleted.ToString();
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }

        public Result MarkAsLocked()
        {
            if (Status == AccountStatusEnum.Locked.ToString())
            {
                return Result.Failure(DomainError.Account.AlreadyLocked);
            }
            Status = AccountStatusEnum.Locked.ToString();
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }

        public Result MarkAsBanned()
        {
            if (Status == AccountStatusEnum.Banned.ToString())
            {
                return Result.Failure(DomainError.Account.AlreadyBanned);
            }
            Status = AccountStatusEnum.Banned.ToString();
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }

        public Result MarkAsActive()
        {
            if (Status == AccountStatusEnum.Active.ToString())
            {
                return Result.Failure(DomainError.Account.AlreadyActivated);
            }
            Status = AccountStatusEnum.Active.ToString();
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }

        public Result UpdateEmail(string newEmail)
        {
            var expression = new EmailAddressAttribute();
            if (!expression.IsValid(newEmail))
            {
                return Result<Account>.Failure(DomainError.Account.InvalidEmail);
            }
            AccountEmail = newEmail;
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }

        public static Result<Account> Create(string aspNetUserId, string accountEmail)
        {
            var expression = new EmailAddressAttribute();
            if (!expression.IsValid(accountEmail))
            {
                return Result<Account>.Failure(DomainError.Account.InvalidEmail);
            }
            return Result<Account>.Success(new Account(AccountId.CreateUnique(), aspNetUserId, accountEmail));
        }

        /// <summary>
        /// Ensures the account has a Wallet; if none exists, creates a new Wallet with a unique WalletId tied to the provided accountId.
        /// This operation is idempotent — if a Wallet is already present, it leaves it unchanged.
        /// </summary>
        /// <param name="accountId">The AccountId to associate with the created Wallet when none exists.</param>
        public void AddWallet(AccountId accountId)
        {
            if (Wallet == null)
            {
                Wallet = new Wallet(WalletId.CreateUnique(), accountId);
            }
            Wallet = Wallet;
        }

        /// <summary>
        /// Adds a token to the account's blacklist and updates the account's UpdatedAt timestamp.
        /// </summary>
        /// <param name="token">The raw token string to blacklist.</param>
        /// <param name="expiration">The UTC expiration time of the blacklisted token.</param>
        /// <returns>A successful <see cref="Result"/> on completion.</returns>
        public Result AddBlackListToken(string token, DateTime expiration)
        {
            var blacklistedToken = BlackListToken.Create(this.Id, token, expiration);
            _blackListTokens.Add(blacklistedToken);
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }

        /// <summary>
        /// Revokes a refresh token belonging to this account.
        /// </summary>
        /// <param name="tokenRemove">The refresh token string to revoke.</param>
        /// <returns>
        /// A successful Result containing the revoked token string when the token is found and revoked;
        /// otherwise a failure Result with <see cref="DomainError.RefreshToken.InvalidToken"/> if no matching token exists.
        /// </returns>
        /// <remarks>
        /// This method mutates the matched <c>RefreshToken</c> by calling its <c>Revoke()</c> method and updates <see cref="UpdatedAt"/> to the current UTC time.
        /// </remarks>
        public Result RevokeRefreshToken(string tokenRemove)
        {
            var refreshToken = _refreshTokens.FirstOrDefault(rt => rt.Token == tokenRemove);
            if (refreshToken == null)
            {
                return Result.Failure(DomainError.RefreshToken.InvalidToken);
            }
            refreshToken.Revoke();
            UpdatedAt = DateTime.UtcNow;
            return Result<string>.Success(refreshToken.Token);
        }

        /// <summary>
        /// Revokes an existing refresh token on this account and returns the revoked token.
        /// </summary>
        /// <remarks>
        /// This locates the refresh token matching <paramref name="oldToken"/>, verifies it has not already been used,
        /// calls <c>Revoke()</c> on it, updates <see cref="UpdatedAt"/> to UTC now, and returns the revoked token wrapped in a successful <c>Result</c>.
        /// The <paramref name="newToken"/> and <paramref name="newExpiryDate"/> parameters are not used by this method.
        /// </remarks>
        /// <param name="oldToken">The existing refresh token string to locate and revoke.</param>
        /// <param name="newToken">Unused; provided for API symmetry but ignored by this implementation.</param>
        /// <param name="newExpiryDate">Unused; provided for API symmetry but ignored by this implementation.</param>
        /// <returns>
        /// A <c>Result&lt;RefreshToken&gt;</c> containing the revoked <c>RefreshToken</c> on success, or a failure result with one of:
        /// <list type="bullet">
        /// <item><description><see cref="DomainError.RefreshToken.RefreshTokenNotFound"/> if no matching token is found.</description></item>
        /// <item><description><see cref="DomainError.RefreshToken.RefreshTokenInvalidated"/> if the found token was already used.</description></item>
        /// </list>
        /// </returns>
        public Result<RefreshToken> RotateRefreshToken(string oldToken, string newToken, DateTime newExpiryDate)
        {
            var tokenToUse = _refreshTokens.FirstOrDefault(rt => rt.Token == oldToken);
            if (tokenToUse is null)
            {
                return Result<RefreshToken>.Failure(DomainError.RefreshToken.RefreshTokenNotFound);
            }

            if (tokenToUse.IsUsed)
            {
                return Result<RefreshToken>.Failure(DomainError.RefreshToken.RefreshTokenInvalidated);
            }
            tokenToUse.Revoke();

            UpdatedAt = DateTime.UtcNow;
            return Result<RefreshToken>.Success(tokenToUse);
        }

        public string GenerateNewOtp()
        {
            var otp = new Random().Next(100000, 999999).ToString();
            UpdatedAt = DateTime.UtcNow;
            return otp;
        }
    }

}
