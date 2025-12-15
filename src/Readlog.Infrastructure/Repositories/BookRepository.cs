using Microsoft.EntityFrameworkCore;
using Readlog.Domain.Abstractions;
using Readlog.Domain.Entities;
using Readlog.Infrastructure.Data;

namespace Readlog.Infrastructure.Repositories;

public class BookRepository(
    ApplicationDbContext dbContext) : IBookRepository
{
    public async Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Books
            .FirstOrDefaultAsync(b  => b.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Books
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);
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
