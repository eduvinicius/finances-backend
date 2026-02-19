namespace MyFinances.Domain.Exceptions
{
    public class BadRequestException(string message) : MyFinancesException(message, StatusCodes.Status400BadRequest)
    {
    }
}
