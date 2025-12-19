using Readlog.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Readlog.Api.Requests.ReadingLists;

/// <summary>
/// Request to update a book's reading status in a reading list
/// </summary>
/// <param name="Status">New reading status (required)</param>
/// <remarks>
/// Available statuses:
/// - WantToRead (0): Book is on the wishlist
/// - Reading (1): Currently reading the book
/// - Finished (2): Completed reading the book
/// - Abandoned (3): Stopped reading the book
/// </remarks>
public sealed record UpdateBookStatusRequest(
    [Required]
    ReadingStatus Status);
