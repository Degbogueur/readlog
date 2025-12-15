using Readlog.Application.Abstractions;
using Readlog.Application.Features.ReadingLists.DTOs;
using Readlog.Application.Shared;
using Readlog.Domain.Abstractions;

namespace Readlog.Application.Features.ReadingLists.Queries;

public sealed record GetUserReadingListsQuery : IQuery<IReadOnlyList<ReadingListResponse>>;

public sealed class GetUserReadingListsQueryHandler(
    IReadingListRepository readingListRepository,
    ICurrentUserService currentUserService) : IQueryHandler<GetUserReadingListsQuery, IReadOnlyList<ReadingListResponse>>
{
    public async Task<Result<IReadOnlyList<ReadingListResponse>>> Handle(GetUserReadingListsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        var readingLists = await readingListRepository.GetByUserIdAsync(userId, cancellationToken);

        var response = readingLists.Select(readingList =>
        {
            var items = readingList.Items.Select(item => new ReadingListItemResponse(
                item.Id,
                item.BookId,
                item.Status,
                item.AddedAt
            )).ToList();

            return new ReadingListResponse(
                readingList.Id,
                readingList.Name,
                readingList.CreatedAt,
                readingList.CreatedBy,
                items
            );
        }).ToList();

        return Result.Success<IReadOnlyList<ReadingListResponse>>(response);
    }
}