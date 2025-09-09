using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        protected IActionResult HandleResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
            {
                return Ok(ApiResponse<T>.Ok(result.Value));
            }


            var firstError = result.Errors.First();
            var statusCode = GetStatusCode(firstError.Type);

            var apiErrors = result.Errors.Select(e => new ApiError(e.Code, e.Description));

            return new ObjectResult(ApiResponse<T>.CreateError("One or more errors occurred.", apiErrors, (System.Net.HttpStatusCode)statusCode))
            {
                StatusCode = statusCode
            };
        }
        protected IActionResult HandleResult(Result result)
        {
            if (result.IsSuccess)
            {
                return Ok(ApiResponse.Ok());
            }

            var firstError = result.Errors.First().Type;
            var statusCode = GetStatusCode(firstError);

            var apiErrors = result.Errors.Select(e => new ApiError(e.Code, e.Description));

            return new ObjectResult(ApiResponse.CreateError("One or more errors occurred.", apiErrors, (System.Net.HttpStatusCode)statusCode))
            {
                StatusCode = statusCode
            };
        }
        
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