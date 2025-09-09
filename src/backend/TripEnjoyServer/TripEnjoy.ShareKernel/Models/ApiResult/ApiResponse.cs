using System.Net;

namespace TripEnjoy.ShareKernel.Models.ApiResult
{
        public class ApiResponse
        {
                public string Message { get; set; } = string.Empty;
                public string Status { get; set; } = "success";
                public HttpStatusCode StatusCode { get; set; }
                public object? Errors { get; set; }

                /// <summary>
                /// Initializes a new <see cref="ApiResponse"/> with the specified status, message, and HTTP status code.
                /// </summary>
                /// <param name="status">A short status label (for example "success" or "error").</param>
                /// <param name="message">A human-readable message describing the response.</param>
                /// <param name="statusCode">The HTTP status code associated with this response.</param>
                public ApiResponse(string status, string message, HttpStatusCode statusCode)
                {
                        Status = status;
                        Message = message;
                        StatusCode = statusCode;
                }

                /// <summary>
                /// Initializes a new <see cref="ApiResponse"/> set to a successful state.
                /// </summary>
                /// <remarks>
                /// Defaults: <see cref="Status"/> = "success" and <see cref="StatusCode"/> = <see cref="System.Net.HttpStatusCode.OK"/>.
                /// </remarks>
                public ApiResponse()
                {
                        Status = "success";
                        StatusCode = HttpStatusCode.OK;
                }

                /// <summary>
                /// Creates an ApiResponse representing a successful operation with HTTP 200 (OK).
                /// </summary>
                /// <param name="message">Optional message to include in the response.</param>
                /// <returns>An <see cref="ApiResponse"/> with Status set to "success" and StatusCode set to <see cref="System.Net.HttpStatusCode.OK"/>.</returns>
                public static ApiResponse Ok(string message = "")
                {
                        return new ApiResponse("success", message, HttpStatusCode.OK);
                }

                /// <summary>
                /// Create an error ApiResponse with the given message and HTTP status code.
                /// </summary>
                /// <param name="message">Human-readable error message to include in the response.</param>
                /// <param name="statusCode">HTTP status code for the response. Defaults to <see cref="HttpStatusCode.BadRequest"/>.</param>
                /// <returns>An <see cref="ApiResponse"/> with <c>Status</c> set to "error", the provided message, and the specified status code.</returns>
                public static ApiResponse CreateError(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
                {
                        return new ApiResponse("error", message, statusCode);
                }

                /// <summary>
                /// Creates an error ApiResponse with the given message, attaches the provided collection of ApiError as the response's Errors, and sets the HTTP status code.
                /// </summary>
                /// <param name="message">Human-readable error message to include in the response.</param>
                /// <param name="errors">Collection of ApiError instances describing specific error details (attached directly to the response).</param>
                /// <param name="statusCode">HTTP status code for the response (defaults to <see cref="HttpStatusCode.BadRequest"/>).</param>
                /// <returns>An <see cref="ApiResponse"/> with Status set to "error", Message set to <paramref name="message"/>, Errors set to <paramref name="errors"/>, and StatusCode set to <paramref name="statusCode"/>.</returns>
                public static ApiResponse CreateError(string message, IEnumerable<ApiError> errors, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
                {
                        var response = new ApiResponse("error", message, statusCode);
                        response.Errors = errors;
                        return response;
                }

                /// <summary>
                /// Adds a new <see cref="ApiError"/> to the response's <see cref="Errors"/> collection, creating the collection as a <see cref="List{ApiError}"/> if needed.
                /// </summary>
                /// <param name="code">Error code identifier.</param>
                /// <param name="detail">Human-readable error detail.</param>
                /// <param name="field">Optional field name related to the error.</param>
                /// <returns>The same <see cref="ApiResponse"/> instance to allow method chaining.</returns>
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

                /// <summary>
                /// Adds the provided <see cref="ApiError"/> items to the response's error collection,
                /// initializing the collection as a <see cref="List{ApiError}"/> if it is null.
                /// </summary>
                /// <param name="errors">Collection of <see cref="ApiError"/> to append to the response.</param>
                /// <returns>The current <see cref="ApiResponse"/> instance.</returns>
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

                /// <summary>
                /// Initializes a new instance of <see cref="ApiResponse{T}"/> with default success metadata.
                /// </summary>
                /// <remarks>
                /// Calls the base <see cref="ApiResponse"/> parameterless constructor which sets <see cref="ApiResponse.Status"/> to "success" and <see cref="ApiResponse.StatusCode"/> to <see cref="System.Net.HttpStatusCode.OK"/>.
                /// </remarks>
                public ApiResponse() : base()
                {
                }

                /// <summary>
                /// Initializes a new <see cref="ApiResponse{T}"/> with the specified status, message, payload, and HTTP status code.
                /// </summary>
                /// <param name="status">A short status label (e.g., "success" or "error").</param>
                /// <param name="message">A human-readable message describing the result.</param>
                /// <param name="data">The response payload; may be null.</param>
                /// <param name="statusCode">The HTTP status code that corresponds to this response.</param>
                public ApiResponse(string status, string message, T? data, HttpStatusCode statusCode)
                   : base(status, message, statusCode)
                {
                        Data = data;
                }

                /// <summary>
                /// Creates a successful ApiResponse containing the provided payload and an optional message.
                /// </summary>
                /// <param name="data">The response payload to include in the ApiResponse.</param>
                /// <param name="message">An optional human-readable message (defaults to empty).</param>
                /// <returns>An <see cref="ApiResponse{T}"/> with Status = "success" and StatusCode = HttpStatusCode.OK.</returns>
                public static ApiResponse<T> Ok(T data, string message = "")
                {
                        return new ApiResponse<T>("success", message, data, HttpStatusCode.OK);
                }

                /// <summary>
                /// Creates an error <see cref="ApiResponse{T}"/> with the specified message and HTTP status code.
                /// The returned response has Status set to "error" and Data set to default(T).
                /// </summary>
                /// <param name="message">Human-readable error message.</param>
                /// <param name="statusCode">HTTP status code for the response (default: <see cref="HttpStatusCode.BadRequest"/>).</param>
                /// <returns>An <see cref="ApiResponse{T}"/> representing an error; Data is default(T).</returns>
                public static new ApiResponse<T> CreateError(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
                {
                        return new ApiResponse<T>("error", message, default, statusCode);
                }

                /// <summary>
                /// Creates an error ApiResponse&lt;T&gt; with the provided message, attached errors, and HTTP status code.
                /// </summary>
                /// <param name="message">Human-readable error message.</param>
                /// <param name="errors">Collection of <see cref="ApiError"/> instances to include in the response's Errors property.</param>
                /// <param name="statusCode">HTTP status code to set on the response. Defaults to <see cref="HttpStatusCode.BadRequest"/>.</param>
                /// <returns>An <see cref="ApiResponse{T}"/> marked as an error with the specified message, errors, and status code.</returns>
                public static ApiResponse<T> CreateError(string message, IEnumerable<ApiError> errors, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
                {
                        var response = new ApiResponse<T>("error", message, default, statusCode);
                        response.Errors = errors;
                        return response;
                }

                /// <summary>
                /// Adds an error (ApiError) to the response and returns this instance for fluent chaining.
                /// </summary>
                /// <param name="code">Machine-readable error code.</param>
                /// <param name="detail">Human-readable error message or detail.</param>
                /// <param name="field">Optional field name related to the error.</param>
                /// <returns>The same <see cref="ApiResponse{T}"/> instance with the new error added.</returns>
                public new ApiResponse<T> AddError(string code, string detail, string field = "")
                {
                        base.AddError(code, detail, field);
                        return this;
                }

                /// <summary>
                /// Adds the provided collection of <see cref="ApiError"/> items to the response's Errors collection and returns this instance for fluent chaining.
                /// </summary>
                /// <param name="errors">Errors to add to the response.</param>
                /// <returns>The same <see cref="ApiResponse{T}"/> instance after the errors have been appended.</returns>
                public new ApiResponse<T> AddErrors(IEnumerable<ApiError> errors)
                {
                        base.AddErrors(errors);
                        return this;
                }
        }
}

