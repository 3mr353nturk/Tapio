namespace Tapio.Application.Common.Exceptions;

public class AppException : Exception
{
    public string Code { get; }

    public AppException(string code, string message) : base(message)
    {
        Code = code;
    }
}

public class NotFoundException : AppException
{
    public NotFoundException(string message = "Kayıt bulunamadı.") : base("NOT_FOUND", message) { }
}

public class ValidationException : AppException
{
    public ValidationException(string message) : base("VALIDATION_ERROR", message) { }
}

public class UnauthorizedAppException : AppException
{
    public UnauthorizedAppException(string message = "Yetkisiz erişim.") : base("UNAUTHORIZED", message) { }
}
