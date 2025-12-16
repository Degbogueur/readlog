using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Readlog.Api.Extensions;
using Readlog.Api.Requests.Books;
using Readlog.Api.Responses;
using Readlog.Application.Features.Books.Commands;
using Readlog.Application.Features.Books.DTOs;
using Readlog.Application.Features.Books.Queries;

namespace Readlog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BooksController(
    ISender sender) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<BookResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllBooksQuery();
        var result = await sender.Send(query, cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet("{id:guid}", Name = "GetBookById")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BookResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetBookByIdQuery(id);
        var result = await sender.Send(query, cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost]
    [ProducesResponseType(typeof(BookResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBookRequest request, CancellationToken cancellationToken)
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

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(BookResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBookRequest request, CancellationToken cancellationToken)
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

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteBookCommand(id);
        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
            return result.ToActionResult();

        return NoContent();
    }
}
