namespace Readlog.Application.Features.Reviews.DTOs;

/// <summary>
/// Book review details
/// </summary>
/// <param name="Id">Unique identifier</param>
/// <param name="BookId">Associated book identifier</param>
/// <param name="Rating">Rating from 1 to 5</param>
/// <param name="Title">Review title</param>
/// <param name="Content">Review content</param>
/// <param name="CreatedAt">Creation timestamp</param>
/// <param name="CreatedBy">Creator unique identifier</param>
/// <param name="UpdatedAt">Last update timestamp</param>
public sealed record ReviewResponse(
    Guid Id,
    Guid BookId,
    int Rating,
    string Title,
    string Content,
    DateTime CreatedAt,
    Guid CreatedBy,
    DateTime? UpdatedAt);
