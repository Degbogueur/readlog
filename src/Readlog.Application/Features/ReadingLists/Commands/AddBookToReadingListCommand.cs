using FluentValidation;
using Readlog.Application.Abstractions;
using Readlog.Application.Shared;
using Readlog.Domain.Abstractions;
using Readlog.Domain.Enums;

namespace Readlog.Application.Features.ReadingLists.Commands;

public sealed record AddBookToReadingListCommand(
    Guid ReadingListId,
    Guid BookId,
    ReadingStatus Status = ReadingStatus.WantToRead) : ICommand;

public sealed class AddBookToReadingListCommandValidator : BaseValidator<AddBookToReadingListCommand>
{
    public AddBookToReadingListCommandValidator()
    {
        RuleFor(x => x.ReadingListId)
            .NotEmpty().WithMessage("Reading list ID is required.");

        RuleFor(x => x.BookId)
            .NotEmpty().WithMessage("Book ID is required.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid reading status.");
    }
}

public sealed class AddBookToReadingListCommandHandler(
    IReadingListRepository readingListRepository,
    IBookRepository bookRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork) : ICommandHandler<AddBookToReadingListCommand>
{
    public async Task<Result> Handle(AddBookToReadingListCommand request, CancellationToken cancellationToken)
    {
        var readingList = await readingListRepository.GetByIdAsync(request.ReadingListId, cancellationToken);

        if (readingList is null)
            return Result.Failure(Error.NotFound("Reading list", request.ReadingListId));

        var userId = currentUserService.UserId;

        if (readingList.CreatedBy != userId)
            return Result.Failure(Error.Unauthorized("You can only modify your own reading lists."));

        var book = await bookRepository.GetByIdAsync(request.BookId, cancellationToken);

        if (book is null)
            return Result.Failure(Error.NotFound("Book", request.BookId));

        readingList.AddBook(request.BookId, request.Status);

        readingListRepository.Update(readingList);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
