namespace PurchaseSystem.Model.Response
{
    public class ApiResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public object? Data { get; set; }

        public static ApiResponse Success(object? data)
        {
            return new ApiResponse
            {
                IsSuccess = true,
                Message = "Success",
                Data = data
            };
        }

        public static ApiResponse Error(string message, object? data)
        {
            return new ApiResponse
            {
                IsSuccess = false,
                Message = message,
                Data = data
            };
        }
    }
}
