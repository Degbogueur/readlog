namespace Readlog.Api.Requests.Books;

public sealed record CreateBookRequest(
    string Title,
    string Author,
    string? Isbn,
    string? Description,
    DateOnly? PublishedDate);
