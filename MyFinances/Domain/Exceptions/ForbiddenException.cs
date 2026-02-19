namespace MyFinances.Domain.Exceptions
{
    public class ForbiddenException(string message = "You don't have permission to access this resource") : MyFinancesException(message, StatusCodes.Status403Forbidden)
    {
    }
}
