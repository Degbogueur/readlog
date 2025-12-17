namespace Readlog.Api.Requests.Books;

public sealed record UpdateBookRequest(
    string Title,
    string Author,
    string? Isbn,
    string? Description,
    DateOnly? PublishedDate);