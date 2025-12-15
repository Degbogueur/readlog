namespace Readlog.Domain.Exceptions;

public sealed class DomainException(string message) : BaseException(message);
