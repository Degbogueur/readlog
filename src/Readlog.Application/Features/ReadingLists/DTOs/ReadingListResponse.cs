namespace Readlog.Application.Features.ReadingLists.DTOs;

public sealed record ReadingListResponse(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    Guid CreatedBy,
    IReadOnlyList<ReadingListItemResponse> Items);