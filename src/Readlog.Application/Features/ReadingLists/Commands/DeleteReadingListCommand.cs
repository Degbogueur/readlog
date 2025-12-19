using FluentValidation;
using Readlog.Application.Abstractions;
using Readlog.Application.Shared;
using Readlog.Domain.Abstractions;

namespace Readlog.Application.Features.ReadingLists.Commands;

public sealed record DeleteReadingListCommand(Guid Id) : ICommand;

public sealed class DeleteReadingListCommandValidator : BaseValidator<DeleteReadingListCommand>
{
    public DeleteReadingListCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Reading list ID is required.");
    }
}

public sealed class DeleteReadingListCommandHandler(
    IReadingListRepository readingListRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteReadingListCommand>
{
    public async Task<Result> Handle(DeleteReadingListCommand request, CancellationToken cancellationToken)
    {
        var readingList = await readingListRepository.GetByIdAsync(request.Id, cancellationToken);

        if (readingList is null)
            return Result.Failure(Error.NotFound("Reading list", request.Id));

        var userId = currentUserService.UserId;

        if (readingList.CreatedBy != userId)
            return Result.Failure(Error.Forbidden("You can only delete your own reading lists."));

        readingListRepository.Delete(readingList);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}