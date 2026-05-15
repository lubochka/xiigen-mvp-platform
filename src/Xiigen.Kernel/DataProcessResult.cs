namespace Xiigen.Kernel;

public sealed record DataProcessError(string Code, string Message, IReadOnlyDictionary<string, object?>? Details = null);

public sealed class DataProcessResult
{
    private DataProcessResult(bool isSuccess, DataProcessError? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public DataProcessError? Error { get; }

    public static DataProcessResult Success() => new(true, null);

    public static DataProcessResult Failure(
        string code,
        string message,
        IReadOnlyDictionary<string, object?>? details = null) =>
        new(false, new DataProcessError(code, message, details));
}

public sealed class DataProcessResult<T>
{
    private DataProcessResult(bool isSuccess, T? data, DataProcessError? error, bool fromCache)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
        FromCache = fromCache;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Data { get; }
    public DataProcessError? Error { get; }
    public bool FromCache { get; }

    public static DataProcessResult<T> Success(T data, bool fromCache = false) =>
        new(true, data, null, fromCache);

    public static DataProcessResult<T> Failure(
        string code,
        string message,
        IReadOnlyDictionary<string, object?>? details = null) =>
        new(false, default, new DataProcessError(code, message, details), false);
}
