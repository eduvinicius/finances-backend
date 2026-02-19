namespace MyFinances.Domain.Exceptions
{
    public abstract class MyFinancesException(string message, int statusCode) : Exception(message)
    {
        public int StatusCode { get; } = statusCode;
    }
}
