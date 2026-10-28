namespace App.Core.Exceptions;

/// <summary>זיהוי נכשל (למשל מייל או סיסמה שגויים)</summary>
public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message) : base(message) { }
}
