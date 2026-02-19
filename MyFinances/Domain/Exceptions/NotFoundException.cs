namespace MyFinances.Domain.Exceptions
{
    public class NotFoundException : MyFinancesException
    {
        public NotFoundException(string resource, object key)
            : base($"{resource} with ID '{key}' was not found.", StatusCodes.Status404NotFound)
        {
        }

        public NotFoundException(string message)
            : base(message, StatusCodes.Status404NotFound)
        {
        }
    }
}
