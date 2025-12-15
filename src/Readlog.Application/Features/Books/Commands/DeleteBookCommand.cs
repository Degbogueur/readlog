using FluentValidation;
using Readlog.Application.Abstractions;
using Readlog.Application.Shared;
using Readlog.Domain.Abstractions;

namespace Readlog.Application.Features.Books.Commands;

public sealed record DeleteBookCommand(Guid Id) : ICommand;

public sealed class DeleteBookCommandValidator : BaseValidator<DeleteBookCommand>
{
    public DeleteBookCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Book ID is required.");
    }
}

public sealed class DeleteBookCommandHandler(
    IBookRepository bookRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteBookCommand>
{
    public async Task<Result> Handle(DeleteBookCommand request, CancellationToken cancellationToken)
    {
        var book = await bookRepository.GetByIdAsync(request.Id, cancellationToken);

        if (book is null)
            return Result.Failure(Error.NotFound("Book", request.Id));

        bookRepository.Delete(book);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
