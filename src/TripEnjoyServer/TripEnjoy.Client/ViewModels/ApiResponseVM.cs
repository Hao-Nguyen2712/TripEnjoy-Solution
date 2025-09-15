using Newtonsoft.Json;

namespace TripEnjoy.Client.ViewModels
{
    public class ApiResponseVM<T>
    {
        public T Data { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public int StatusCode { get; set; }
        public object Errors { get; set; } // Can be more specific if error structure is known
    }
}
