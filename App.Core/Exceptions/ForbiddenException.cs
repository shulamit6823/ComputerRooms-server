namespace App.Core.Exceptions;

/// <summary>המשתמש מזוהה אבל אינו רשאי לבצע את הפעולה (למשל פנייה של מורה אחרת)</summary>
public class ForbiddenException : AppException
{
    public ForbiddenException(string message) : base(message) { }
}
