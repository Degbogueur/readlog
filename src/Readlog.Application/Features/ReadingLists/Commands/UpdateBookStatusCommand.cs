using FluentValidation;
using Readlog.Application.Abstractions;
using Readlog.Application.Features.ReadingLists.DTOs;
using Readlog.Application.Shared;
using Readlog.Domain.Abstractions;
using Readlog.Domain.Enums;

namespace Readlog.Application.Features.ReadingLists.Commands;

public sealed record UpdateBookStatusCommand(
    Guid ReadingListId,
    Guid BookId,
    ReadingStatus Status) : ICommand<ReadingListResponse>;

public sealed class UpdateBookStatusCommandValidator : BaseValidator<UpdateBookStatusCommand>
{
    public UpdateBookStatusCommandValidator()
    {
        RuleFor(x => x.ReadingListId)
            .NotEmpty().WithMessage("Reading list ID is required.");

        RuleFor(x => x.BookId)
            .NotEmpty().WithMessage("Book ID is required.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid reading status.");
    }
}

public sealed class UpdateBookStatusCommandHandler(
    IReadingListRepository readingListRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateBookStatusCommand, ReadingListResponse>
{
    public async Task<Result<ReadingListResponse>> Handle(UpdateBookStatusCommand request, CancellationToken cancellationToken)
    {
        var readingList = await readingListRepository.GetByIdAsync(request.ReadingListId, cancellationToken);

        if (readingList is null)
            return Result.Failure<ReadingListResponse>(
                Error.NotFound("Reading list", request.ReadingListId));

        var userId = currentUserService.UserId;

        if (readingList.CreatedBy != userId)
            return Result.Failure<ReadingListResponse>(
                Error.Forbidden("You can only modify your own reading lists."));

        readingList.UpdateBookStatus(request.BookId, request.Status);

        readingListRepository.Update(readingList);
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
