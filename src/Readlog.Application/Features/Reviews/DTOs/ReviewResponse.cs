namespace Readlog.Application.Features.Reviews.DTOs;

public sealed record ReviewResponse(
    Guid Id,
    Guid BookId,
    int Rating,
    string Title,
    string Content,
    DateTime CreatedAt,
    Guid CreatedBy,
    DateTime? UpdatedAt);
