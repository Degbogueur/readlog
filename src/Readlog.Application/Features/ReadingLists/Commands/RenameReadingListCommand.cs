using FluentValidation;
using Readlog.Application.Abstractions;
using Readlog.Application.Features.ReadingLists.DTOs;
using Readlog.Application.Shared;
using Readlog.Domain.Abstractions;

namespace Readlog.Application.Features.ReadingLists.Commands;

public sealed record RenameReadingListCommand(Guid Id, string Name) : ICommand<ReadingListResponse>;

public sealed class RenameReadingListCommandValidator : BaseValidator<RenameReadingListCommand>
{
    public RenameReadingListCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Reading list ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");
    }
}

public sealed class RenameReadingListCommandHandler(
    IReadingListRepository readingListRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork) : ICommandHandler<RenameReadingListCommand, ReadingListResponse>
{
    public async Task<Result<ReadingListResponse>> Handle(RenameReadingListCommand request, CancellationToken cancellationToken)
    {
        var readingList = await readingListRepository.GetByIdAsync(request.Id, cancellationToken);

        if (readingList is null)
            return Result.Failure<ReadingListResponse>(
                Error.NotFound("Reading list", request.Id));

        var userId = currentUserService.UserId;

        if (readingList.CreatedBy != userId)
            return Result.Failure<ReadingListResponse>(
                Error.Forbidden("You can only modify your own reading lists."));

        readingList.Rename(request.Name);

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
