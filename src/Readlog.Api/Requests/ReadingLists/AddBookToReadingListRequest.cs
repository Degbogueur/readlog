using Readlog.Domain.Enums;

namespace Readlog.Api.Requests.ReadingLists;

public sealed record AddBookToReadingListRequest(
    Guid BookId,
    ReadingStatus Status = ReadingStatus.WantToRead);