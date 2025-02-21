﻿
public enum StatusCodeEnum
{
    None = 200,
    Invalid = 400,
    Unauthorized = 401,
    Forbidden = 403,
    NotFound = 404,
    InternalServerError = 500,
    BadGateway = 502,
    ServiceUnavailable = 503,
    GatewayTimeout = 504,
    Conflict = 409,
    PreconditionFailed = 412,
    UnprocessableEntity = 422,
    TooManyRequests = 429
}

public enum GiftSettingStatus
{
    Inactive = 0,        
    InProgress = 1,      
    Completed = 2,      
    Error = 3         
}

[Flags]
public enum Permission
{
    None = 0,
    View = 1,
    Edit = 2,
    Delete = 4,
    Create = 8
}
public enum UserRole
{

    Employee = 1,
    Manager = 2,
    Admin = 3,
    Assistant = 4,

}

public enum TaskStatus
{
    Sending = 1,
    Failed = 2,
    Completed = 3

}
public class VerificationCacheEntry
{
    public string Code { get; set; }
    public string UserId { get; set; }
    public string ChatId { get; set; }
    public DateTime Expiry { get; set; }
    public int Attempts { get; set; }
}

public class ApiResponse<T>
{
    public bool IsOk { get; set; } 
    public int StatusCode { get; set; } 
    public string Message { get; set; } 
    public T Data { get; set; }


    public static ApiResponse<T> Success(string message = "Operation successful", StatusCodeEnum statusCode = StatusCodeEnum.None)
    {
        return new ApiResponse<T> { IsOk = true, StatusCode = (int)statusCode, Message = message };
    }

    public static ApiResponse<T> Success(T data, string message = "Operation successful", StatusCodeEnum statusCode = StatusCodeEnum.None)
    {
        return new ApiResponse<T> { IsOk = true, StatusCode = (int)statusCode, Data = data, Message = message };
    }

    public static ApiResponse<T> Fail(string message, StatusCodeEnum statusCode)
    {
        return new ApiResponse<T> { IsOk = false, StatusCode = (int)statusCode, Message = message };
    }
}