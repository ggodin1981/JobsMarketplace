namespace JobsMarketplace.Api.Exceptions;

public class RequestValidationException(string message) : ApiException(message, StatusCodes.Status400BadRequest);

