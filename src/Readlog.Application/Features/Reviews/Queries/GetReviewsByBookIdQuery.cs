using Readlog.Application.Abstractions;
using Readlog.Application.Features.Reviews.DTOs;
using Readlog.Application.Shared;
using Readlog.Domain.Abstractions;

namespace Readlog.Application.Features.Reviews.Queries;

public sealed record GetReviewsByBookIdQuery(
    Guid BookId,
    string? SortBy = null,
    bool SortDescending = false,
    int Page = 1,
    int PageSize = 10) : IQuery<PagedResult<ReviewResponse>>;

public sealed class GetReviewsByBookIdQueryHandler(
    IReviewRepository reviewRepository) : IQueryHandler<GetReviewsByBookIdQuery, PagedResult<ReviewResponse>>
{
    public async Task<Result<PagedResult<ReviewResponse>>> Handle(GetReviewsByBookIdQuery request, CancellationToken cancellationToken)
    {
        var (reviews, totalCount) = await reviewRepository.GetByBookIdAsync(
            request.BookId,
            request.SortBy,
            request.SortDescending,
            request.Page,
            request.PageSize,
            cancellationToken);

        var items = reviews.Select(review => new ReviewResponse(
            review.Id,
            review.BookId,
            review.Rating.Value,
            review.Title,
            review.Content,
            review.CreatedAt,
            review.CreatedBy,
            review.UpdatedAt)).ToList();

        var pagedResult = new PagedResult<ReviewResponse>(
            items,
            totalCount,
            request.Page,
            request.PageSize);

        return Result.Success(pagedResult);
    }
}