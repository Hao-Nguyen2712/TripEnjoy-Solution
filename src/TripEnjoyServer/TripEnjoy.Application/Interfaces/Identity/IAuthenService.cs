using System.Security.Claims;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Interfaces.Identity
{
    public interface IAuthenService
    {
        /// <summary>
        /// Creates a new user account with the specified email, password, and role.
        /// </summary>
        /// <param name="email">The user's email address (used as the account identifier).</param>
        /// <param name="password">The password for the new account.</param>
        /// <param name="role">The role to assign to the user (e.g., "User", "Admin").</param>
        /// <returns>
        /// A Result wrapping a tuple of (UserId, confirmToken) on success:
        /// - UserId: the created user's identifier.
        /// - confirmToken: a token used to confirm the user's email.
        /// The Result indicates success or failure and carries error information on failure.
        /// </returns>
        Task<Result<(string UserId, string confirmToken)>> CreateUserAsync(string email, string password, string role);

        /// <summary>
        /// Creates a new user account with the specified email, password, and role. Optionally handles email confirmation.
        /// </summary>
        /// <param name="email">The user's email address (used as the account identifier).</param>
        /// <param name="password">The password for the new account.</param>
        /// <param name="role">The role to assign to the user (e.g., "User", "Partner", "Admin").</param>
        /// <param name="requireEmailConfirmation">Whether to require email confirmation. False for partners who undergo document approval.</param>
        /// <returns>
        /// A Result wrapping a tuple of (UserId, confirmToken) on success:
        /// - UserId: the created user's identifier.
        /// - confirmToken: a token used to confirm the user's email (empty if confirmation not required).
        /// The Result indicates success or failure and carries error information on failure.
        /// </returns>
        Task<Result<(string UserId, string confirmToken)>> CreateUserAsync(string email, string password, string role, bool requireEmailConfirmation);
        /// <summary>
        /// Performs the first step of the login flow with role validation by validating the provided credentials and expected role before initiating the next authentication step.
        /// </summary>
        /// <param name="email">The user's email address used as the login identifier.</param>
        /// <param name="password">The user's plaintext password to validate against stored credentials.</param>
        /// <param name="expectedRole">The expected role for the account (e.g., "User", "Partner"). If provided, validates that the account has this role.</param>
        /// <returns>A <see cref="Result"/> that indicates whether the credential validation, role validation, and initiation of the next authentication step succeeded or failed.</returns>
        Task<Result> LoginStepOneAsync(string email, string password, string expectedRole);
        /// <summary>
        /// Completes second step of a two-step login by validating the provided one-time password (OTP).
        /// </summary>
        /// <param name="email">The user's email address used in step one of login.</param>
        /// <param name="otp">The one-time password supplied to the user (e.g., via email/SMS) for verification.</param>
        /// <returns>
        /// A <see cref="Result"/> containing a tuple on success:
        /// - <c>AuthResult</c>: authentication result data (tokens, expiration, user info) as <see cref="AuthResultDTO"/>.
        /// - <c>CacheKey</c>: a key for any temporary login state stored during the multi-step flow.
        /// The <see cref="Result"/> indicates failure details when verification does not succeed.
        /// </returns>
        Task<Result<(AuthResultDTO AuthResult, string CacheKey)>> LoginStepTwoAsync(string email, string otp);
        /// <summary>
        /// Confirms a user's email using the provided user ID and confirmation token.
        /// </summary>
        /// <param name="userId">The identifier of the user whose email will be confirmed.</param>
        /// <param name="confirmToken">The email confirmation token previously issued for the user.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing a string payload on success (e.g., a confirmation message or related data); on failure the Result will indicate the reason (invalid/expired token, non-existent user, etc.).
        /// </returns>
        Task<Result<string>> ConfirmEmailAsync(string userId, string confirmToken);
        /// <summary>
        /// Extracts a ClaimsPrincipal from a JWT that may be expired.
        /// </summary>
        /// <param name="token">The JWT string to parse. May be expired; expected to be a well-formed token.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the extracted <see cref="ClaimsPrincipal"/> on success, or a failure Result if the token is invalid or cannot be parsed. The contained <see cref="ClaimsPrincipal"/> may be null when extraction succeeds but no principal could be constructed.
        /// </returns>
        Task<Result<ClaimsPrincipal?>> GetPrincipalFromExpiredToken(string token);
        /// <summary>
        /// Generates a new refresh token string for client authentication flows.
        /// </summary>
        /// <returns>A non-empty refresh token suitable for persisting and later exchanging for an access token.</returns>
        string GenerateRefreshToken();
        /// <summary>
        /// Generates a signed, short-lived access token (JWT) for the specified user identifier.
        /// </summary>
        /// <param name="userId">The identifier of the user for whom the access token will be issued.</param>
        /// <returns>A task that resolves to the access token string (JWT).</returns>
        Task<string> GenerateAccessTokenAsync(string userId);
        Task<Result<string>> GeneratePasswordResetTokenAsync(string email);
    }
}
