using Readlog.Application.Abstractions;
using Readlog.Application.Features.Books.DTOs;
using Readlog.Application.Shared;
using Readlog.Domain.Abstractions;

namespace Readlog.Application.Features.Books.Queries;

public sealed record GetBookByIdQuery(Guid Id) : IQuery<BookResponse>;

public sealed class GetBookByIdQueryHandler(
    IBookRepository bookRepository) : IQueryHandler<GetBookByIdQuery, BookResponse>
{
    public async Task<Result<BookResponse>> Handle(GetBookByIdQuery request, CancellationToken cancellationToken)
    {
        var book = await bookRepository.GetByIdAsync(request.Id, cancellationToken);

        if (book is null)
            return Result.Failure<BookResponse>(Error.NotFound("Book", request.Id));

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
