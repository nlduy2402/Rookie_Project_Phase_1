namespace RetailSystem.API.Shared
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public string TraceId { get; set; } = string.Empty;

        public static ApiResponse<T> SuccessResult(T data, string traceId, string message = "Success")
            => new ApiResponse<T> { Success = true, Data = data, TraceId = traceId, Message = message };
    }
}
