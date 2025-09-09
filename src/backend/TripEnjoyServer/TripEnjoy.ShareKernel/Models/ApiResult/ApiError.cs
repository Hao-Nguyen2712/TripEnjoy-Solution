namespace TripEnjoy.ShareKernel.Models.ApiResult
{
        public class ApiError
        {
                /// <summary>
                /// Error code - can be used for client-side error handling
                /// </summary>
                public string Code { get; set; } = string.Empty;

                /// <summary>
                /// Detailed error description
                /// </summary>
                public string Detail { get; set; } = string.Empty;

                /// <summary>
                /// Field or property that caused the error (if applicable)
                /// </summary>
                public string Field { get; set; } = string.Empty;

                /// <summary>
                /// Initializes a new instance of the <see cref="ApiError"/> class with default (empty) property values.
                /// </summary>
                public ApiError() { }

                /// <summary>
                /// Initializes a new <see cref="ApiError"/> with the specified error code, detail message, and optional field name.
                /// </summary>
                /// <param name="code">Machine-readable error code used for client-side handling.</param>
                /// <param name="detail">Human-readable description of the error.</param>
                /// <param name="field">Optional name of the field or property that caused the error.</param>
                public ApiError(string code, string detail, string field = "")
                {
                        Code = code;
                        Detail = detail;
                        Field = field;
                }
        }
}
