namespace Readlog.Domain.Exceptions;

public sealed class NotFoundException(string message) : BaseException(message);
