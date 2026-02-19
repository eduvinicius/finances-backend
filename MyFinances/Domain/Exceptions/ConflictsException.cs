namespace MyFinances.Domain.Exceptions
{
    public class ConflictException(string message) : MyFinancesException(message, StatusCodes.Status409Conflict)
    {
    }
}
