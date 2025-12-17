using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Readlog.Api.Extensions;
using Readlog.Api.Requests.ReadingLists;
using Readlog.Api.Responses;
using Readlog.Application.Features.ReadingLists.Commands;
using Readlog.Application.Features.ReadingLists.DTOs;
using Readlog.Application.Features.ReadingLists.Queries;
using Readlog.Application.Shared;

namespace Readlog.Api.Controllers;

[ApiController]
[Route("api/reading-lists")]
[Authorize]
public class ReadingListsController(
    ISender sender) : ControllerBase
{
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

    [HttpGet("{id:guid}", Name = "GetReadingListById")]
    [ProducesResponseType(typeof(ReadingListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetReadingListByIdQuery(id);
        var result = await sender.Send(query, cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReadingListResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateReadingListRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateReadingListCommand(request.Name);
        var result = await sender.Send(command, cancellationToken);

        return result.ToCreatedResult("GetReadingListById", response => new { id = response.Id });
    }

    [HttpPut("{id:guid}/rename")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Rename(Guid id, [FromBody] RenameReadingListRequest request, CancellationToken cancellationToken)
    {
        var command = new RenameReadingListCommand(id, request.Name);
        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
            return result.ToActionResult();

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteReadingListCommand(id);
        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
            return result.ToActionResult();

        return NoContent();
    }

    [HttpPost("{id:guid}/books")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddBook(Guid id, [FromBody] AddBookToReadingListRequest request, CancellationToken cancellationToken)
    {
        var command = new AddBookToReadingListCommand(id, request.BookId, request.Status);
        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
            return result.ToActionResult();

        return NoContent();
    }

    [HttpDelete("{id:guid}/books/{bookId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveBook(Guid id, Guid bookId, CancellationToken cancellationToken)
    {
        var command = new RemoveBookFromReadingListCommand(id, bookId);
        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
            return result.ToActionResult();

        return NoContent();
    }

    [HttpPut("{id:guid}/books/{bookId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateBookStatus(Guid id, Guid bookId, [FromBody] UpdateBookStatusRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateBookStatusCommand(id, bookId, request.Status);
        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
            return result.ToActionResult();

        return NoContent();
    }
}
