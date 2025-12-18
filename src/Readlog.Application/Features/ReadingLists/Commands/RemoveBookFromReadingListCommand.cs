using FluentValidation;
using Readlog.Application.Abstractions;
using Readlog.Application.Features.ReadingLists.DTOs;
using Readlog.Application.Shared;
using Readlog.Domain.Abstractions;

namespace Readlog.Application.Features.ReadingLists.Commands;

public sealed record RemoveBookFromReadingListCommand(
    Guid ReadingListId,
    Guid BookId) : ICommand<ReadingListResponse>;

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
    IUnitOfWork unitOfWork) : ICommandHandler<RemoveBookFromReadingListCommand, ReadingListResponse>
{
    public async Task<Result<ReadingListResponse>> Handle(
        RemoveBookFromReadingListCommand request,
        CancellationToken cancellationToken)
    {
        var readingList = await readingListRepository.GetByIdAsync(request.ReadingListId, cancellationToken);

        if (readingList is null)
            return Result.Failure<ReadingListResponse>(
                Error.NotFound("Reading list", request.ReadingListId));

        var userId = currentUserService.UserId;

        if (readingList.CreatedBy != userId)
            return Result.Failure<ReadingListResponse>(
                Error.Unauthorized("You can only modify your own reading lists."));

        readingList.RemoveBook(request.BookId);

        //readingListRepository.Update(readingList);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new ReadingListResponse(
            readingList.Id,
            readingList.Name,
            readingList.CreatedAt,
            readingList.CreatedBy,
            readingList.Items.Select(rli => new ReadingListItemResponse(
                rli.Id,
                rli.BookId,
                rli.Status,
                rli.AddedAt)).ToList());

        return Result.Success(response);
    }
}
