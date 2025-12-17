using Readlog.Application.Abstractions;
using Readlog.Application.Features.ReadingLists.DTOs;
using Readlog.Application.Shared;
using Readlog.Domain.Abstractions;

namespace Readlog.Application.Features.ReadingLists.Queries;

public sealed record GetUserReadingListsQuery(
    string? Search = null,
    string? SortBy = null,
    bool SortDescending = false,
    int Page = 1,
    int PageSize = 10) : IQuery<PagedResult<ReadingListResponse>>;

public sealed class GetUserReadingListsQueryHandler(
    IReadingListRepository readingListRepository,
    ICurrentUserService currentUserService) : IQueryHandler<GetUserReadingListsQuery, PagedResult<ReadingListResponse>>
{
    public async Task<Result<PagedResult<ReadingListResponse>>> Handle(GetUserReadingListsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        var (readingLists, totalCount) = await readingListRepository.GetByUserIdAsync(
            userId,
            request.Search,
            request.SortBy,
            request.SortDescending,
            request.Page,
            request.PageSize,
            cancellationToken);

        var items = readingLists.Select(readingList =>
        {
            var listItems = readingList.Items.Select(item => new ReadingListItemResponse(
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
                listItems
            );
        }).ToList();

        var pagedResult = new PagedResult<ReadingListResponse>(
            items,
            totalCount,
            request.Page,
            request.PageSize);

        return Result.Success(pagedResult);
    }
}