using Readlog.Domain.Enums;

namespace Readlog.Application.Features.ReadingLists.DTOs;

/// <summary>
/// Book item in a reading list
/// </summary>
/// <param name="Id">Unique identifier</param>
/// <param name="BookId">Book identifier</param>
/// <param name="Status">Current reading status</param>
/// <param name="AddedAt">When the book was added to the list</param>
public sealed record ReadingListItemResponse(
    Guid Id,
    Guid BookId,
    ReadingStatus Status,
    DateTime AddedAt);
