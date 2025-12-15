using FluentValidation;
using Readlog.Application.Abstractions;
using Readlog.Application.Features.ReadingLists.DTOs;
using Readlog.Application.Shared;
using Readlog.Domain.Abstractions;
using Readlog.Domain.Entities;

namespace Readlog.Application.Features.ReadingLists.Commands;

public sealed record CreateReadingListCommand(string Name) : ICommand<ReadingListResponse>;

public sealed class CreateReadingListCommandValidator : BaseValidator<CreateReadingListCommand>
{
    public CreateReadingListCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");
    }
}

public sealed class CreateReadingListCommandHandler(
    IReadingListRepository readingListRepository, 
    IUnitOfWork unitOfWork) : ICommandHandler<CreateReadingListCommand, ReadingListResponse>
{
    public async Task<Result<ReadingListResponse>> Handle(CreateReadingListCommand request, CancellationToken cancellationToken)
    {
        var readingList = ReadingList.Create(request.Name);

        await readingListRepository.AddAsync(readingList, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new ReadingListResponse(
            readingList.Id,
            readingList.Name,
            readingList.CreatedAt,
            readingList.CreatedBy,
            []
        );

        return Result.Success(response);
    }
}
