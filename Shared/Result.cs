namespace leagues.Shared;

public readonly struct Unit 
{
    public static readonly Unit Value = new Unit();
}

public class Result<V, E> where E : struct, Enum
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public V? Value { get; }
    public E? Error { get; }

    private Result(bool isSuccess, V? value, E? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<V, E> Success(V value) => new Result<V, E>(true, value, null);
    public static Result<V, E> Failure(E error) => new Result<V, E>(false, default, error);

    public static implicit operator Result<V, E>(V value) =>
        Success(value);

    // Allows: return AppError.SomeError;
    public static implicit operator Result<V, E>(E error) =>
        Failure(error);
}
