namespace Readlog.Api.Requests.Books;

public sealed record CreateBookRequest(
    string Title,
    string Author,
    string? Isbn = null,
    string? Description = null,
    DateOnly? PublishedDate = null);
