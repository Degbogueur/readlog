using Readlog.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Readlog.Api.Requests.ReadingLists;

/// <summary>
/// Request to add a book to a reading list
/// </summary>
/// <param name="BookId">ID of the book to add (required)</param>
/// <param name="Status">Initial reading status (optional, defaults to WantToRead)</param>
/// <remarks>
/// Available statuses:
/// - WantToRead (0): Book is on the wishlist
/// - Reading (1): Currently reading the book
/// - Finished (2): Completed reading the book
/// - Abandoned (3): Stopped reading the book
/// </remarks>
public sealed record AddBookToReadingListRequest(
    [Required]
    Guid BookId,

    ReadingStatus Status = ReadingStatus.WantToRead);