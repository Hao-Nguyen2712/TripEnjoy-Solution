using System.Net;

namespace TripEnjoy.ShareKernel.Models.ApiResult
{
    public class ApiResponse
    {
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = "success";
        public HttpStatusCode StatusCode { get; set; }
        public object? Errors { get; set; }

        public ApiResponse(string status, string message, HttpStatusCode statusCode)
        {
            Status = status;
            Message = message;
            StatusCode = statusCode;
        }

        public ApiResponse()
        {
            Status = "success";
            StatusCode = HttpStatusCode.OK;
        }

        public static ApiResponse Ok(string message = "")
        {
            return new ApiResponse("success", message, HttpStatusCode.OK);
        }

        public static ApiResponse CreateError(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            return new ApiResponse("error", message, statusCode);
        }

        public static ApiResponse CreateError(string message, IEnumerable<ApiError> errors, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            var response = new ApiResponse("error", message, statusCode);
            response.Errors = errors;
            return response;
        }

        public ApiResponse AddError(string code, string detail, string field = "")
        {
            if (Errors == null)
            {
                Errors = new List<ApiError>();
            }

            if (Errors is List<ApiError> errorList)
            {
                errorList.Add(new ApiError(code, detail, field));
            }
            return this;
        }

        public ApiResponse AddErrors(IEnumerable<ApiError> errors)
        {
            if (Errors == null)
            {
                Errors = new List<ApiError>();
            }

            if (Errors is List<ApiError> errorList)
            {
                errorList.AddRange(errors);
            }
            return this;
        }
    }
    /// <summary>ok</summary>
    /// <typeparam name="T"></typeparam>
    public class ApiResponse<T> : ApiResponse
    {
        public T? Data { get; set; }

        public ApiResponse() : base()
        {
        }

        public ApiResponse(string status, string message, T? data, HttpStatusCode statusCode)
           : base(status, message, statusCode)
        {
            Data = data;
        }

        public static ApiResponse<T> Ok(T data, string message = "")
        {
            return new ApiResponse<T>("success", message, data, HttpStatusCode.OK);
        }

        public static new ApiResponse<T> CreateError(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            return new ApiResponse<T>("error", message, default, statusCode);
        }

        public static ApiResponse<T> CreateError(string message, IEnumerable<ApiError> errors, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            var response = new ApiResponse<T>("error", message, default, statusCode);
            response.Errors = errors;
            return response;
        }

        public new ApiResponse<T> AddError(string code, string detail, string field = "")
        {
            base.AddError(code, detail, field);
            return this;
        }

        public new ApiResponse<T> AddErrors(IEnumerable<ApiError> errors)
        {
            base.AddErrors(errors);
            return this;
        }
    }
}

