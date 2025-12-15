using FluentValidation;
using Readlog.Application.Abstractions;
using Readlog.Application.Features.Reviews.DTOs;
using Readlog.Application.Shared;
using Readlog.Domain.Abstractions;

namespace Readlog.Application.Features.Reviews.Commands;

public sealed record UpdateReviewCommand(
    Guid Id,
    int Rating,
    string Title,
    string Content) : ICommand<ReviewResponse>;

public sealed class UpdateReviewCommandValidator : BaseValidator<UpdateReviewCommand>
{
    public UpdateReviewCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Review ID is required.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.")
            .MaximumLength(5000).WithMessage("Content must not exceed 5000 characters.");
    }
}

public sealed class UpdateReviewCommandHandler(
    IReviewRepository reviewRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateReviewCommand, ReviewResponse>
{
    public async Task<Result<ReviewResponse>> Handle(UpdateReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await reviewRepository.GetByIdAsync(request.Id, cancellationToken);

        if (review is null)
            return Result.Failure<ReviewResponse>(Error.NotFound("Review", request.Id));

        var userId = currentUserService.UserId;

        if (review.CreatedBy != userId)
            return Result.Failure<ReviewResponse>(Error.Unauthorized("You can only update your own reviews."));

        review.Update(request.Rating, request.Title, request.Content);

        reviewRepository.Update(review);
        await unitOfWork.SaveChangesAsync(cancellationToken);

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