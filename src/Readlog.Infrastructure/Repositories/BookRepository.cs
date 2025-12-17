using Microsoft.EntityFrameworkCore;
using Readlog.Application.Extensions;
using Readlog.Domain.Abstractions;
using Readlog.Domain.Entities;
using Readlog.Infrastructure.Data;
using System.Linq.Expressions;

namespace Readlog.Infrastructure.Repositories;

public class BookRepository(
    ApplicationDbContext dbContext) : IBookRepository
{
    private static readonly Dictionary<string, Expression<Func<Book, object>>> SortMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["title"] = b => b.Title,
        ["author"] = b => b.Author,
        ["publishedDate"] = b => b.PublishedDate!,
        ["createdAt"] = b => b.CreatedAt
    };

    public async Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Books
            .FirstOrDefaultAsync(b  => b.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<Book> Items, int TotalCount)> GetAllAsync(
        string? search,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Books.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            string pattern = $"%{search}%";
            query = query.Where(b => 
                EF.Functions.Like(b.Title, pattern) ||
                EF.Functions.Like(b.Author, pattern) ||
                EF.Functions.Like(b.Description ?? "", pattern) ||
                (b.Isbn != null && EF.Functions.Like(b.Isbn.Value ?? "", pattern)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = query.ApplySort(
            sortBy,
            sortDescending,
            SortMappings,
            b => b.CreatedAt);

        var items = await query
            .ApplyPagination(page, pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Book book, CancellationToken cancellationToken = default)
    {
        await dbContext.Books.AddAsync(book, cancellationToken);
    }

    public void Update(Book book)
    {
        dbContext.Books.Update(book);
    }

    public void Delete(Book book)
    {
        dbContext.Books.Remove(book);
    }
}
