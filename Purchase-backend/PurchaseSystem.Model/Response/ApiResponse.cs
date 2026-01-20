using System.Data;

namespace PurchaseSystem.Model.Response
{
    public class ApiResponse
    {
        public bool IsSuccess { get; set; }
        public string ErrorCode { get; set; }
        public string Message { get; set; }
        public object? Data { get; set; }

        public static ApiResponse Success(object? data, string message = "Success")
        {
            return new ApiResponse
            {
                IsSuccess = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse Error(string errorMessage)
        {
            return new ApiResponse
            {
                IsSuccess = false,
                Message = errorMessage
            };
        }

        public static ApiResponse Error(string errorCode, string errorMessage)
        {
            return new ApiResponse
            {
                IsSuccess = false,
                ErrorCode = errorCode,
                Message = errorMessage
            };
        }
    }
}
