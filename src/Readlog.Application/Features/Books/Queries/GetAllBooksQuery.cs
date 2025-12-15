using Readlog.Application.Abstractions;
using Readlog.Application.Features.Books.DTOs;
using Readlog.Application.Shared;
using Readlog.Domain.Abstractions;
using System.Collections.Generic;

namespace Readlog.Application.Features.Books.Queries;

public sealed record GetAllBooksQuery : IQuery<IReadOnlyList<BookResponse>>;

public sealed class GetAllBooksQueryHandler(
    IBookRepository bookRepository) : IQueryHandler<GetAllBooksQuery, IReadOnlyList<BookResponse>>
{
    public async Task<Result<IReadOnlyList<BookResponse>>> Handle(GetAllBooksQuery request, CancellationToken cancellationToken)
    {
        var books = await bookRepository.GetAllAsync(cancellationToken);

        var response = books.Select(book => new BookResponse(
            book.Id,
            book.Title,
            book.Author,
            book.Isbn?.Value,
            book.Description,
            book.PublishedDate,
            book.CreatedAt,
            book.CreatedBy)).ToList();

        return Result.Success<IReadOnlyList<BookResponse>>(response);
    }
}
