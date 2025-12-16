using Readlog.Domain.Enums;

namespace Readlog.Api.Requests.ReadingLists;

public sealed record UpdateBookStatusRequest(ReadingStatus Status);
