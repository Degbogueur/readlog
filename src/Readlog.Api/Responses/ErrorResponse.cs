namespace Readlog.Api.Responses;

public sealed record ErrorResponse(
    string Code,
    string Message,
    string[]? Details = null
);
