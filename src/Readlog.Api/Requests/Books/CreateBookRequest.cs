using System.ComponentModel.DataAnnotations;

namespace Readlog.Api.Requests.Books;

/// <summary>
/// Request to create a new book
/// </summary>
/// <param name="Title">Book title (required, max 200 characters)</param>
/// <param name="Author">Author name (required, max 150 characters)</param>
/// <param name="Isbn">ISBN-10 or ISBN-13 (optional)</param>
/// <param name="Description">Book description (optional, max 2000 characters)</param>
/// <param name="PublishedDate">Publication date (optional, cannot be in the future)</param>
public sealed record CreateBookRequest(
    [Required]
    [StringLength(200)]
    string Title,

    [Required]
    [StringLength(150)]
    string Author,

    string? Isbn = null,

    [StringLength(2000)]
    string? Description = null,

    DateOnly? PublishedDate = null);
