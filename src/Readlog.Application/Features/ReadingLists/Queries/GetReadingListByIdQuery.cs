using Readlog.Application.Abstractions;
using Readlog.Application.Features.ReadingLists.DTOs;
using Readlog.Application.Shared;
using Readlog.Domain.Abstractions;

namespace Readlog.Application.Features.ReadingLists.Queries;

public sealed record GetReadingListByIdQuery(Guid Id) : IQuery<ReadingListResponse>;

public sealed class GetReadingListByIdQueryHandler(
    IReadingListRepository readingListRepository) : IQueryHandler<GetReadingListByIdQuery, ReadingListResponse>
{
    public async Task<Result<ReadingListResponse>> Handle(GetReadingListByIdQuery request, CancellationToken cancellationToken)
    {
        var readingList = await readingListRepository.GetByIdAsync(request.Id, cancellationToken);

        if (readingList is null)
            return Result.Failure<ReadingListResponse>(Error.NotFound("Reading list", request.Id));

        var items = readingList.Items.Select(item => new ReadingListItemResponse(
            item.Id,
            item.BookId,
            item.Status,
            item.AddedAt
        )).ToList();

        var response = new ReadingListResponse(
            readingList.Id,
            readingList.Name,
            readingList.CreatedAt,
            readingList.CreatedBy,
            items
        );

        return Result.Success(response);
    }
}
