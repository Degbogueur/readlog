using FluentValidation;
using Readlog.Application.Abstractions;
using Readlog.Application.Features.Books.DTOs;
using Readlog.Application.Shared;
using Readlog.Domain.Abstractions;
using Readlog.Domain.Entities;

namespace Readlog.Application.Features.Books.Commands;

public sealed record CreateBookCommand(
    string Title,
    string Author,
    string? Isbn = null,
    string? Description = null,
    DateOnly? PublishedDate = null) : ICommand<BookResponse>;

public sealed class CreateBookCommandValidator : BaseValidator<CreateBookCommand>
{
    public CreateBookCommandValidator()
    {
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

public sealed class CreateBookCommandHandler(
    IBookRepository bookRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateBookCommand, BookResponse>
{
    public async Task<Result<BookResponse>> Handle(CreateBookCommand request, CancellationToken cancellationToken)
    {
        var book = Book.Create(
            request.Title,
            request.Author,
            request.Isbn,
            request.Description,
            request.PublishedDate);

        await bookRepository.AddAsync(book, cancellationToken);
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
