using FluentValidation;
using Readlog.Application.Abstractions;
using Readlog.Application.Shared;
using Readlog.Domain.Abstractions;

namespace Readlog.Application.Features.ReadingLists.Commands;

public sealed record RenameReadingListCommand(Guid Id, string Name) : ICommand;

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
    IUnitOfWork unitOfWork) : ICommandHandler<RenameReadingListCommand>
{
    public async Task<Result> Handle(RenameReadingListCommand request, CancellationToken cancellationToken)
    {
        var readingList = await readingListRepository.GetByIdAsync(request.Id, cancellationToken);

        if (readingList is null)
            return Result.Failure(Error.NotFound("Reading list", request.Id));

        var userId = currentUserService.UserId;

        if (readingList.CreatedBy != userId)
            return Result.Failure(Error.Unauthorized("You can only modify your own reading lists."));

        readingList.Rename(request.Name);

        readingListRepository.Update(readingList);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
