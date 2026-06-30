namespace JobsMarketplace.Api.Exceptions;

public class NotFoundException(string message) : ApiException(message, StatusCodes.Status404NotFound);

