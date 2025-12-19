using FluentValidation;
using Readlog.Application.Abstractions;
using Readlog.Application.Features.Books.DTOs;
using Readlog.Application.Shared;
using Readlog.Domain.Abstractions;

namespace Readlog.Application.Features.Books.Commands;

public sealed record UpdateBookCommand(
    Guid Id,
    string Title,
    string Author,
    string? Isbn = null,
    string? Description = null,
    DateOnly? PublishedDate = null) : ICommand<BookResponse>;

public sealed class UpdateBookCommandValidator : BaseValidator<UpdateBookCommand>
{
    public UpdateBookCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Book ID is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("Author name is required.")
            .MaximumLength(150).WithMessage("Author name must not exceed 150 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.PublishedDate)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("Published date cannot be in the future.")
            .When(x => x.PublishedDate is not null);
    }
}

public sealed class UpdateBookCommandHandler(
    IBookRepository bookRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateBookCommand, BookResponse>
{
    public async Task<Result<BookResponse>> Handle(UpdateBookCommand request, CancellationToken cancellationToken)
    {
        var book = await bookRepository.GetByIdAsync(request.Id, cancellationToken);

        if (book is null)
            return Result.Failure<BookResponse>(Error.NotFound("Book", request.Id));

        book.Update(
            request.Title,
            request.Author,
            request.Isbn,
            request.Description,
            request.PublishedDate);

        bookRepository.Update(book);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new BookResponse(
            book.Id,
            book.Title,
            book.Author,
            book.Isbn?.Value,
            book.Description,
            book.PublishedDate,
            book.CreatedAt,
            book.CreatedBy);

        return Result.Success(response);
    }
}
