using FluentValidation;
using Readlog.Application.Abstractions;
using Readlog.Application.Shared;
using Readlog.Domain.Abstractions;

namespace Readlog.Application.Features.ReadingLists.Commands;

public sealed record RemoveBookFromReadingListCommand(
    Guid ReadingListId,
    Guid BookId) : ICommand;

public sealed class RemoveBookFromReadingListCommandValidator : BaseValidator<RemoveBookFromReadingListCommand>
{
    public RemoveBookFromReadingListCommandValidator()
    {
        RuleFor(x => x.ReadingListId)
            .NotEmpty().WithMessage("Reading list ID is required.");

        RuleFor(x => x.BookId)
            .NotEmpty().WithMessage("Book ID is required.");
    }
}

public sealed class RemoveBookFromReadingListCommandHandler(
    IReadingListRepository readingListRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork) : ICommandHandler<RemoveBookFromReadingListCommand>
{
    public async Task<Result> Handle(RemoveBookFromReadingListCommand request, CancellationToken cancellationToken)
    {
        var readingList = await readingListRepository.GetByIdAsync(request.ReadingListId, cancellationToken);

        if (readingList is null)
            return Result.Failure(Error.NotFound("Reading list", request.ReadingListId));

        var userId = currentUserService.UserId;

        if (readingList.CreatedBy != userId)
            return Result.Failure(Error.Unauthorized("You can only modify your own reading lists."));

        readingList.RemoveBook(request.BookId);

        readingListRepository.Update(readingList);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
