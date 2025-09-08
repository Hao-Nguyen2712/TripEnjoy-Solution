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

        public ApiError() { }

        public ApiError(string code, string detail, string field = "")
        {
            Code = code;
            Detail = detail;
            Field = field;
        }
    }
}
