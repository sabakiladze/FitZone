namespace FitZone.Web.Services;

/// <summary>API-ს პასუხი ხელსაყრელი ფორმით: წარმატებაა თუ არა, სტატუსი, შეცდომების ტექსტები.</summary>
public class ApiResult
{
    public bool Success { get; init; }
    public int StatusCode { get; init; }
    public List<string> Errors { get; init; } = new();

    public string ErrorText => string.Join(" ", Errors);
}

public class ApiResult<T> : ApiResult
{
    public T? Data { get; init; }
}

/// <summary>API-მ 401 დააბრუნა მაშინ, როცა მომხმარებელი ტოკენით იყო შესული (ტოკენი ვადაგასულია).</summary>
public class UnauthorizedApiException : Exception
{
    public UnauthorizedApiException() : base("Your session has expired. Please log in again.") { }
}
