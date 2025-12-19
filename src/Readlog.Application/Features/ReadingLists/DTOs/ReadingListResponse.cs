namespace Readlog.Application.Features.ReadingLists.DTOs;

/// <summary>
/// Reading list details
/// </summary>
/// <param name="Id">Unique identifier</param>
/// <param name="Name">Reading list name</param>
/// <param name="CreatedAt">Creation timestamp</param>
/// <param name="CreatedBy">Creator unique identifier</param>
/// <param name="Items">Books in the reading list</param>
public sealed record ReadingListResponse(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    Guid CreatedBy,
    IReadOnlyList<ReadingListItemResponse> Items);