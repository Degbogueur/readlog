using FluentValidation;
using Readlog.Application.Abstractions;
using Readlog.Application.Features.Reviews.DTOs;
using Readlog.Application.Shared;
using Readlog.Domain.Abstractions;
using Readlog.Domain.Entities;

namespace Readlog.Application.Features.Reviews.Commands;

public sealed record CreateReviewCommand(
    Guid BookId,
    int Rating,
    string Title,
    string Content) : ICommand<ReviewResponse>;

public sealed class CreateReviewCommandValidator : BaseValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        RuleFor(x => x.BookId)
            .NotEmpty().WithMessage("Book ID is required.");

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

public sealed class CreateReviewCommandHandler(
    IReviewRepository reviewRepository,
    IBookRepository bookRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateReviewCommand, ReviewResponse>
{
    public async Task<Result<ReviewResponse>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var book = await bookRepository.GetByIdAsync(request.BookId, cancellationToken);

        if (book is null)
            return Result.Failure<ReviewResponse>(Error.NotFound("Book", request.BookId));

        var userId = currentUserService.UserId;
        var existingReview = await reviewRepository.GetByBookAndUserAsync(request.BookId, userId, cancellationToken);

        if (existingReview is not null)
            return Result.Failure<ReviewResponse>(Error.Conflict("You have already reviewed this book."));

        var review = Review.Create(
            request.BookId,
            request.Rating,
            request.Title,
            request.Content
        );

        await reviewRepository.AddAsync(review, cancellationToken);
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