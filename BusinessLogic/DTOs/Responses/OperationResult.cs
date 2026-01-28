namespace BusinessLogic.DTOs.Response;

public class OperationResult
{
    public bool Success { get; set; }
    public string? ErrorCode { get; set; }
    public string? Message { get; set; }

    public static OperationResult Failed(string message, string? errorCode = null)
    {
        return new OperationResult { Success = false, Message = message, ErrorCode = errorCode };
    }

    public static OperationResult Ok(string? message = null)
    {
        return new OperationResult { Success = true, Message = message };
    }
}

public class OperationResult<T> : OperationResult
{
    public T? Data { get; set; }

    public static OperationResult<T> Failed(string message, string? errorCode = null)
    {
        return new OperationResult<T> { Success = false, Message = message, ErrorCode = errorCode };
    }

    public static OperationResult<T> Ok(T data, string? message = null)
    {
        return new OperationResult<T> { Success = true, Data = data, Message = message };
    }
}
