using FluentValidation;
using Readlog.Application.Abstractions;
using Readlog.Application.Shared;
using Readlog.Domain.Abstractions;

namespace Readlog.Application.Features.Reviews.Commands;

public sealed record DeleteReviewCommand(Guid Id) : ICommand;

public sealed class DeleteReviewCommandValidator : BaseValidator<DeleteReviewCommand>
{
    public DeleteReviewCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Review ID is required.");
    }
}

public sealed class DeleteReviewCommandHandler(
    IReviewRepository reviewRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteReviewCommand>
{
    public async Task<Result> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await reviewRepository.GetByIdAsync(request.Id, cancellationToken);

        if (review is null)
            return Result.Failure(Error.NotFound("Review", request.Id));

        var userId = currentUserService.UserId;

        if (review.CreatedBy != userId)
            return Result.Failure(Error.Unauthorized("You can only delete your own reviews."));

        reviewRepository.Delete(review);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
