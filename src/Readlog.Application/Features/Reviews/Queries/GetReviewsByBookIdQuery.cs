using Readlog.Application.Abstractions;
using Readlog.Application.Features.Reviews.DTOs;
using Readlog.Application.Shared;
using Readlog.Domain.Abstractions;

namespace Readlog.Application.Features.Reviews.Queries;

public sealed record GetReviewsByBookIdQuery(Guid BookId) : IQuery<IReadOnlyList<ReviewResponse>>;

public sealed class GetReviewsByBookIdQueryHandler(
    IReviewRepository reviewRepository) : IQueryHandler<GetReviewsByBookIdQuery, IReadOnlyList<ReviewResponse>>
{
    public async Task<Result<IReadOnlyList<ReviewResponse>>> Handle(GetReviewsByBookIdQuery request, CancellationToken cancellationToken)
    {
        var reviews = await reviewRepository.GetByBookIdAsync(request.BookId, cancellationToken);

        var response = reviews.Select(review => new ReviewResponse(
            review.Id,
            review.BookId,
            review.Rating.Value,
            review.Title,
            review.Content,
            review.CreatedAt,
            review.CreatedBy,
            review.UpdatedAt
        )).ToList();

        return Result.Success<IReadOnlyList<ReviewResponse>>(response);
    }
}