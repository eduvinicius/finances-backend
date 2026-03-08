namespace MyFinances.Domain.Exceptions
{
    public class ForbiddenException(string message = "Você não tem permissão para acessar este recurso") : MyFinancesException(message, StatusCodes.Status403Forbidden)
    {
    }
}
