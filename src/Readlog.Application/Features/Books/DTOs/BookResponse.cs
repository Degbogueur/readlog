namespace Readlog.Application.Features.Books.DTOs;

/// <summary>
/// Book details
/// </summary>
/// <param name="Id">Unique identifier</param>
/// <param name="Title">Book title</param>
/// <param name="Author">Author name</param>
/// <param name="Isbn">ISBN (normalized, without dashes)</param>
/// <param name="Description">Book description</param>
/// <param name="PublishedDate">Publication date</param>
/// <param name="CreatedAt">Creation timestamp</param>
/// <param name="CreatedBy">Creator unique identifier</param>
public sealed record BookResponse(
    Guid Id,
    string Title,
    string Author,
    string? Isbn,
    string? Description,
    DateOnly? PublishedDate,
    DateTime CreatedAt,
    Guid CreatedBy);
