using Readlog.Application.Abstractions;
using Readlog.Application.Features.Books.DTOs;
using Readlog.Application.Shared;
using Readlog.Domain.Abstractions;

namespace Readlog.Application.Features.Books.Queries;

public sealed record GetAllBooksQuery(
    string? Search = null,
    string? SortBy = null,
    bool SortDescending = false,
    int Page = 1,
    int PageSize = 10) : IQuery<PagedResult<BookResponse>>;

public sealed class GetAllBooksQueryHandler(
    IBookRepository bookRepository) : IQueryHandler<GetAllBooksQuery, PagedResult<BookResponse>>
{
    public async Task<Result<PagedResult<BookResponse>>> Handle(GetAllBooksQuery request, CancellationToken cancellationToken)
    {
        var (books, totalCount) = await bookRepository.GetAllAsync(
            request.Search,
            request.SortBy,
            request.SortDescending,
            request.Page,
            request.PageSize,
            cancellationToken);

        var items = books.Select(book => new BookResponse(
            book.Id,
            book.Title,
            book.Author,
            book.Isbn?.Value,
            book.Description,
            book.PublishedDate,
            book.CreatedAt,
            book.CreatedBy)).ToList();

        var pagedResult = new PagedResult<BookResponse>(
            items,
            totalCount,
            request.Page,
            request.PageSize);

        return Result.Success(pagedResult);
    }
}
