using Microsoft.AspNetCore.Mvc;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Models.ApiResult;

namespace TripEnjoy.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApiControllerBase : ControllerBase
    {
        /// <summary>
        /// Converts a domain Result&lt;T&gt; into an IActionResult: returns 200 OK with a successful ApiResponse when the result is successful,
        /// or an ObjectResult containing a standardized error ApiResponse and an HTTP status code derived from the first error's type when it is not.
        /// </summary>
        /// <param name="result">The domain result to translate; on failure its Errors collection will be converted to ApiError entries included in the response.</param>
        /// <param name="message">Optional success message. Defaults to "Operation successful".</param>
        /// <returns>
        /// An IActionResult representing either a successful ApiResponse&lt;T&gt; (HTTP 200) or an error ObjectResult containing ApiResponse&lt;T&gt; with a list of ApiError and the corresponding HTTP status code.
        /// </returns>
        protected IActionResult HandleResult<T>(Result<T> result, string? message = null)
        {
            if (result.IsSuccess)
            {
                return Ok(ApiResponse<T>.Ok(result.Value, message ?? "Operation successful"));
            }

            var firstError = result.Errors.First();
            var statusCode = GetStatusCode(firstError.Type);

            var apiErrors = result.Errors.Select(e => new ApiError(e.Code, e.Description));

            return new ObjectResult(ApiResponse<T>.CreateError("One or more errors occurred.", apiErrors, (System.Net.HttpStatusCode)statusCode))
            {
                StatusCode = statusCode
            };
        }

        /// <summary>
        /// Converts a domain Result&lt;T&gt; into an IActionResult with a custom success status code: returns the specified status code with a successful ApiResponse when the result is successful,
        /// or an ObjectResult containing a standardized error ApiResponse and an HTTP status code derived from the first error's type when it is not.
        /// </summary>
        /// <param name="result">The domain result to translate; on failure its Errors collection will be converted to ApiError entries included in the response.</param>
        /// <param name="successStatusCode">The HTTP status code to return on success (e.g., 201 for Created, 202 for Accepted).</param>
        /// <param name="message">Optional success message. Defaults to "Operation successful".</param>
        /// <returns>
        /// An IActionResult representing either a successful ApiResponse&lt;T&gt; with custom status code or an error ObjectResult containing ApiResponse&lt;T&gt; with a list of ApiError and the corresponding HTTP status code.
        /// </returns>
        protected IActionResult HandleResult<T>(Result<T> result, int successStatusCode, string? message = null)
        {
            if (result.IsSuccess)
            {
                return new ObjectResult(ApiResponse<T>.Ok(result.Value, message ?? "Operation successful"))
                {
                    StatusCode = successStatusCode
                };
            }

            var firstError = result.Errors.First();
            var statusCode = GetStatusCode(firstError.Type);

            var apiErrors = result.Errors.Select(e => new ApiError(e.Code, e.Description));

            return new ObjectResult(ApiResponse<T>.CreateError("One or more errors occurred.", apiErrors, (System.Net.HttpStatusCode)statusCode))
            {
                StatusCode = statusCode
            };
        }
        /// <summary>
        /// Converts a domain Result into an IActionResult: returns 200 OK with a success ApiResponse when the result is successful, or an error response containing one or more ApiError entries and an appropriate HTTP status code when the result represents failure.
        /// </summary>
        /// <param name="result">The domain result to translate; on failure its Errors collection will be converted to ApiError entries included in the response.</param>
        /// <param name="message">Optional success message. Defaults to "Operation successful".</param>
        /// <returns>
        /// On success: an OkObjectResult containing ApiResponse.Ok(); on failure: an ObjectResult whose payload is ApiResponse.CreateError with the message "One or more errors occurred." and a list of ApiError (one per domain error), and whose HTTP status code is derived from the first error's type.
        /// </returns>
        protected IActionResult HandleResult(Result result, string? message = null)
        {
            if (result.IsSuccess)
            {
                return Ok(ApiResponse.Ok(message ?? "Operation successful"));
            }

            var firstError = result.Errors.First().Type;
            var statusCode = GetStatusCode(firstError);

            var apiErrors = result.Errors.Select(e => new ApiError(e.Code, e.Description));

            return new ObjectResult(ApiResponse.CreateError("One or more errors occurred.", apiErrors, (System.Net.HttpStatusCode)statusCode))
            {
                StatusCode = statusCode
            };
        }

        /// <summary>
        /// Converts a domain Result into an IActionResult with a custom success status code: returns the specified status code with a success ApiResponse when the result is successful, or an error response containing one or more ApiError entries and an appropriate HTTP status code when the result represents failure.
        /// </summary>
        /// <param name="result">The domain result to translate; on failure its Errors collection will be converted to ApiError entries included in the response.</param>
        /// <param name="successStatusCode">The HTTP status code to return on success (e.g., 201 for Created, 202 for Accepted).</param>
        /// <param name="message">Optional success message. Defaults to "Operation successful".</param>
        /// <returns>
        /// On success: an ObjectResult containing ApiResponse.Ok() with custom status code; on failure: an ObjectResult whose payload is ApiResponse.CreateError with the message "One or more errors occurred." and a list of ApiError (one per domain error), and whose HTTP status code is derived from the first error's type.
        /// </returns>
        protected IActionResult HandleResult(Result result, int successStatusCode, string? message = null)
        {
            if (result.IsSuccess)
            {
                return new ObjectResult(ApiResponse.Ok(message ?? "Operation successful"))
                {
                    StatusCode = successStatusCode
                };
            }

            var firstError = result.Errors.First().Type;
            var statusCode = GetStatusCode(firstError);

            var apiErrors = result.Errors.Select(e => new ApiError(e.Code, e.Description));

            return new ObjectResult(ApiResponse.CreateError("One or more errors occurred.", apiErrors, (System.Net.HttpStatusCode)statusCode))
            {
                StatusCode = statusCode
            };
        }

        /// <summary>
        /// Maps a domain <see cref="ErrorType"/> to the corresponding HTTP status code.
        /// </summary>
        /// <param name="errorType">The domain error type used to determine the HTTP status code.</param>
        /// <returns>An int HTTP status code (e.g., 404 for NotFound, 409 for Conflict). Defaults to 400 for unknown types.</returns>
        private int GetStatusCode(ErrorType errorType)
        {
            return errorType switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized, // Lỗi 401
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,     // Lỗi 403
                _ => StatusCodes.Status400BadRequest
            };
        }

    }
}