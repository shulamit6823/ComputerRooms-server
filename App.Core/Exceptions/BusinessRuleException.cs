namespace App.Core.Exceptions;

public class BusinessRuleException : AppException
{
    public BusinessRuleException(string message) : base(message) { }
}
