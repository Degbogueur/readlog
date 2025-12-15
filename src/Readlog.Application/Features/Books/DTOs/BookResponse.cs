namespace Readlog.Application.Features.Books.DTOs;

public sealed record BookResponse(
    Guid Id,
    string Title,
    string Author,
    string? Isbn,
    string? Description,
    DateOnly? PublishedDate,
    DateTime CreatedAt,
    Guid CreatedBy);
