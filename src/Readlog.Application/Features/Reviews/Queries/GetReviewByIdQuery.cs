using Readlog.Application.Abstractions;
using Readlog.Application.Features.Reviews.DTOs;
using Readlog.Application.Shared;
using Readlog.Domain.Abstractions;

namespace Readlog.Application.Features.Reviews.Queries;

public sealed record GetReviewByIdQuery(Guid Id) : IQuery<ReviewResponse>;

public sealed class GetReviewByIdQueryHandler(
    IReviewRepository reviewRepository) : IQueryHandler<GetReviewByIdQuery, ReviewResponse>
{
    public async Task<Result<ReviewResponse>> Handle(GetReviewByIdQuery request, CancellationToken cancellationToken)
    {
        var review = await reviewRepository.GetByIdAsync(request.Id, cancellationToken);

        if (review is null)
            return Result.Failure<ReviewResponse>(Error.NotFound("Review", request.Id));

        var response = new ReviewResponse(
            review.Id,
            review.BookId,
            review.Rating.Value,
            review.Title,
            review.Content,
            review.CreatedAt,
            review.CreatedBy,
            review.UpdatedAt
        );

        return Result.Success(response);
    }
}
