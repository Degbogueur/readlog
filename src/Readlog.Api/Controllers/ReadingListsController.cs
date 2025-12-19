using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Readlog.Api.Extensions;
using Readlog.Api.Requests.ReadingLists;
using Readlog.Application.Features.ReadingLists.Commands;
using Readlog.Application.Features.ReadingLists.DTOs;
using Readlog.Application.Features.ReadingLists.Queries;
using Readlog.Application.Shared;

namespace Readlog.Api.Controllers;

/// <summary>
/// Manages user reading lists
/// </summary>
[ApiController]
[Route("api/reading-lists")]
[Authorize]
[Produces("application/json")]
public class ReadingListsController(
    ISender sender) : ControllerBase
{
    /// <summary>
    /// Get all reading lists for the current user
    /// </summary>
    /// <param name="search">Search term for list name</param>
    /// <param name="sortBy">Sort field (name, createdat)</param>
    /// <param name="sortDescending">Sort in descending order</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 10)</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Paginated list of reading lists</returns>
    /// <response code="200">Returns the user's reading lists</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ReadingListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserReadingLists(
        [FromQuery] string? search,
        [FromQuery] string? sortBy,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10, 
        CancellationToken cancellationToken = default)
    {
        var query = new GetUserReadingListsQuery(search, sortBy, sortDescending, page, pageSize);
        var result = await sender.Send(query, cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Get a specific reading list by ID
    /// </summary>
    /// <param name="id">Reading list ID</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The requested reading list</returns>
    /// <response code="200">Returns the reading list</response>
    /// <response code="403">Not authorized to view this reading list</response>
    /// <response code="404">Reading list not found</response>
    [HttpGet("{id:guid}", Name = "GetReadingListById")]
    [ProducesResponseType(typeof(ReadingListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var query = new GetReadingListByIdQuery(id);
        var result = await sender.Send(query, cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Create a new reading list
    /// </summary>
    /// <param name="request">Reading list details</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The created reading list</returns>
    /// <response code="201">Reading list successfully created</response>
    /// <response code="400">Invalid reading list data</response>
    [HttpPost]
    [ProducesResponseType(typeof(ReadingListResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateReadingListRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateReadingListCommand(request.Name);
        var result = await sender.Send(command, cancellationToken);

        return result.ToCreatedResult("GetReadingListById", response => new { id = response.Id });
    }

    /// <summary>
    /// Rename a reading list
    /// </summary>
    /// <param name="id">Reading list ID</param>
    /// <param name="request">Renamed reading list details</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The renamed reading list</returns>
    /// <response code="200">Reading list successfully renamed</response>
    /// <response code="400">Invalid reading list data</response>
    /// <response code="403">Not authorized to rename this reading list</response>
    /// <response code="404">Reading list not found</response>
    [HttpPut("{id:guid}/rename")]
    [ProducesResponseType(typeof(ReadingListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Rename(
        Guid id, 
        [FromBody] RenameReadingListRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new RenameReadingListCommand(id, request.Name);
        var result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Delete a reading list
    /// </summary>
    /// <param name="id">Reading list ID</param>
    /// <param name="cancellationToken"></param>
    /// <response code="204">Reading list successfully deleted</response>
    /// <response code="403">Not authorized to delete this reading list</response>
    /// <response code="404">Reading list not found</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var command = new DeleteReadingListCommand(id);
        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
            return result.ToActionResult();

        return NoContent();
    }

    /// <summary>
    /// Add a book to a reading list
    /// </summary>
    /// <param name="id">Reading list ID</param>
    /// <param name="request">Book details to add</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The updated reading list</returns>
    /// <response code="200">Book successfully added</response>
    /// <response code="400">Invalid book or list data</response>
    /// <response code="403">Not authorized to modify this reading list</response>
    /// <response code="404">Reading list or book not found</response>
    /// <response code="409">Book already in the reading list</response>
    [HttpPost("{id:guid}/books")]
    [ProducesResponseType(typeof(ReadingListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddBook(
        Guid id, 
        [FromBody] AddBookToReadingListRequest request, 
        CancellationToken cancellationToken = default)
    {
        var command = new AddBookToReadingListCommand(id, request.BookId, request.Status);
        var result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Remove a book from a reading list
    /// </summary>
    /// <param name="id">Reading list ID</param>
    /// <param name="bookId">Book ID</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The updated reading list</returns>
    /// <response code="200">Book successfully removed</response>
    /// <response code="400">Invalid book or list data</response>
    /// <response code="403">Not authorized to modify this reading list</response>
    /// <response code="404">Reading list or book not found</response>
    [HttpDelete("{id:guid}/books/{bookId:guid}")]
    [ProducesResponseType(typeof(ReadingListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveBook(Guid id, Guid bookId, CancellationToken cancellationToken = default)
    {
        var command = new RemoveBookFromReadingListCommand(id, bookId);
        var result = await sender.Send(command, cancellationToken);
        
        return result.ToActionResult();
    }

    /// <summary>
    /// Update a book's reading status in a reading list
    /// </summary>
    /// <param name="id">Reading list ID</param>
    /// <param name="bookId">Book ID</param>
    /// <param name="request">New reading status</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The updated reading list</returns>
    /// <response code="200">Status successfully updated</response>
    /// <response code="400">Invalid book or list data</response>
    /// <response code="403">Not authorized to modify this reading list</response>
    /// <response code="404">Reading list or book not found</response>
    [HttpPut("{id:guid}/books/{bookId:guid}/status")]
    [ProducesResponseType(typeof(ReadingListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBookStatus(
        Guid id, 
        Guid bookId, 
        [FromBody] UpdateBookStatusRequest request, 
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateBookStatusCommand(id, bookId, request.Status);
        var result = await sender.Send(command, cancellationToken);
        
        return result.ToActionResult();
    }
}
