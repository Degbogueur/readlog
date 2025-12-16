using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Readlog.Api.Extensions;
using Readlog.Api.Requests.Reviews;
using Readlog.Api.Responses;
using Readlog.Application.Features.Reviews.Commands;
using Readlog.Application.Features.Reviews.DTOs;
using Readlog.Application.Features.Reviews.Queries;
using Readlog.Application.Shared;

namespace Readlog.Api.Controllers;

[ApiController]
[Route("api/books/{bookId:guid}/reviews")]
[Authorize]
public class ReviewsController(
    ISender sender) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<ReviewResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByBookId(
        Guid bookId,
        [FromQuery] string? sortBy,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetReviewsByBookIdQuery(bookId, sortBy, sortDescending, page, pageSize);
        var result = await sender.Send(query, cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet("{id:guid}", Name = "GetReviewById")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid bookId, Guid id, CancellationToken cancellationToken)
    {
        var query = new GetReviewByIdQuery(id);
        var result = await sender.Send(query, cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(Guid bookId, [FromBody] CreateReviewRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateReviewCommand(
            bookId,
            request.Rating,
            request.Title,
            request.Content
        );

        var result = await sender.Send(command, cancellationToken);

        return result.ToCreatedResult("GetReviewById", response => new { bookId, id = response.Id });
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid bookId, Guid id, [FromBody] UpdateReviewRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateReviewCommand(
            id,
            request.Rating,
            request.Title,
            request.Content
        );

        var result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid bookId, Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteReviewCommand(id);
        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
            return result.ToActionResult();

        return NoContent();
    }
}
