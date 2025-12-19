namespace Readlog.Domain.Exceptions;

public sealed class ConflictException(string message) : BaseException(message);
