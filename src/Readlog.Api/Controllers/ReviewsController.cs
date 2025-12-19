using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Readlog.Api.Extensions;
using Readlog.Api.Requests.Reviews;
using Readlog.Application.Features.Reviews.Commands;
using Readlog.Application.Features.Reviews.DTOs;
using Readlog.Application.Features.Reviews.Queries;
using Readlog.Application.Shared;

namespace Readlog.Api.Controllers;

/// <summary>
/// Manages book reviews
/// </summary>
[ApiController]
[Route("api/books/{bookId:guid}/reviews")]
[Authorize]
[Produces("application/json")]
public class ReviewsController(
    ISender sender) : ControllerBase
{
    /// <summary>
    /// Get all reviews for a book
    /// </summary>
    /// <param name="bookId">Book ID</param>
    /// <param name="sortBy">Sort field (rating, title, createdat)</param>
    /// <param name="sortDescending">Sort in descending order</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 10)</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Paginated list of reviews</returns>
    /// <response code="200">Returns the list of reviews</response>
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

    /// <summary>
    /// Get a specific review for a book by its ID
    /// </summary>
    /// <param name="bookId">Book ID</param>
    /// <param name="id">Review ID</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The requested review</returns>
    /// <response code="200">Returns the review</response>
    /// <response code="404">Review not found</response>
    [HttpGet("{id:guid}", Name = "GetReviewById")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid bookId, Guid id, CancellationToken cancellationToken)
    {
        var query = new GetReviewByIdQuery(id);
        var result = await sender.Send(query, cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Create a review for a book
    /// </summary>
    /// <remarks>
    /// Each user can only create one review per book.
    /// </remarks>
    /// <param name="bookId">Book ID</param>
    /// <param name="request">Review details</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The created review</returns>
    /// <response code="201">Review successfully created</response>
    /// <response code="400">Invalid review data</response>
    /// <response code="404">Book not found</response>
    /// <response code="409">User has already reviewed this book</response>
    [HttpPost]
    [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        Guid bookId, 
        [FromBody] CreateReviewRequest request, 
        CancellationToken cancellationToken = default)
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

    /// <summary>
    /// Update your review for a book
    /// </summary>
    /// <param name="bookId">Book ID</param>
    /// <param name="id">Review ID</param>
    /// <param name="request">Updated review details</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The updated review</returns>
    /// <response code="200">Review successfully updated</response>
    /// <response code="400">Invalid review data</response>
    /// <response code="403">Not authorized to update this review</response>
    /// <response code="404">Review not found</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid bookId, 
        Guid id, 
        [FromBody] UpdateReviewRequest request, 
        CancellationToken cancellationToken = default)
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

    /// <summary>
    /// Delete your review for a book
    /// </summary>
    /// <param name="bookId">Book ID</param>
    /// <param name="id">Review ID</param>
    /// <param name="cancellationToken"></param>
    /// <response code="204">Review successfully deleted</response>
    /// <response code="403">Not authorized to delete this review</response>
    /// <response code="404">Review not found</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid bookId, 
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteReviewCommand(id);
        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
            return result.ToActionResult();

        return NoContent();
    }
}
