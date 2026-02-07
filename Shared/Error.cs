namespace competitions.Shared;

public class Error<T> where T : struct, Enum
{
    public T Code { get; }
    public string Message { get; }
    
    public Error(T code, string message)
    {
        Code = code;
        Message = message;
    }
}