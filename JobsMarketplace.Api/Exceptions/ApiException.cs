namespace JobsMarketplace.Api.Exceptions;

public class ApiException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

