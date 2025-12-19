using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Readlog.Api.Extensions;
using Readlog.Api.Requests.Books;
using Readlog.Application.Features.Books.Commands;
using Readlog.Application.Features.Books.DTOs;
using Readlog.Application.Features.Books.Queries;
using Readlog.Application.Shared;

namespace Readlog.Api.Controllers;

/// <summary>
/// Manages books in the system
/// </summary>
[ApiController]
[Route("api/books")]
[Authorize]
[Produces("application/json")]
public class BooksController(
    ISender sender) : ControllerBase
{
    /// <summary>
    /// Get all books with pagination, search, and sorting
    /// </summary>
    /// <param name="search">Search term for title, author, or description</param>
    /// <param name="sortBy">Sort field (title, author, publisheddate, createdat)</param>
    /// <param name="sortDescending">Sort in descending order</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 10, max: 50)</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Paginated list of books</returns>
    /// <response code="200">Returns the list of books</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<BookResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? sortBy,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllBooksQuery(search, sortBy, sortDescending, page, pageSize);
        var result = await sender.Send(query, cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Get a specific book by ID
    /// </summary>
    /// <param name="id">Book ID</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The requested book</returns>
    /// <response code="200">Returns the book</response>
    /// <response code="404">Book not found</response>
    [HttpGet("{id:guid}", Name = "GetBookById")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BookResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        var query = new GetBookByIdQuery(id);
        var result = await sender.Send(query, cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Create a new book
    /// </summary>
    /// <param name="request">Book details</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The created book</returns>
    /// <response code="201">Book successfully created</response>
    /// <response code="400">Invalid book data</response>
    [HttpPost]
    [ProducesResponseType(typeof(BookResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateBookRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateBookCommand(
            request.Title,
            request.Author,
            request.Isbn,
            request.Description,
            request.PublishedDate
        );

        var result = await sender.Send(command, cancellationToken);

        return result.ToCreatedResult("GetBookById", response => new { id = response.Id });
    }

    /// <summary>
    /// Update an existing book
    /// </summary>
    /// <param name="id">Book ID</param>
    /// <param name="request">Updated book details</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The updated book</returns>
    /// <response code="200">Book successfully updated</response>
    /// <response code="400">Invalid book data</response>
    /// <response code="404">Book not found</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(BookResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateBookRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateBookCommand(
            id,
            request.Title,
            request.Author,
            request.Isbn,
            request.Description,
            request.PublishedDate
        );

        var result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Delete a book
    /// </summary>
    /// <param name="id">Book ID</param>
    /// <param name="cancellationToken"></param>
    /// <response code="204">Book successfully deleted</response>
    /// <response code="404">Book not found</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteBookCommand(id);
        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
            return result.ToActionResult();

        return NoContent();
    }
}
