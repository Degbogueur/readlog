using Readlog.Domain.Enums;

namespace Readlog.Application.Features.ReadingLists.DTOs;

public sealed record ReadingListItemResponse(
    Guid Id,
    Guid BookId,
    ReadingStatus Status,
    DateTime AddedAt);
