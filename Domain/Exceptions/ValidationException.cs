namespace MyFinances.Domain.Exceptions
{
    public class ValidationException(string message) : MyFinancesException(message, StatusCodes.Status400BadRequest)
    {
    }
}
