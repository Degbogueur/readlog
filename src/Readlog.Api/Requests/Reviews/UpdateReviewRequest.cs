namespace Readlog.Api.Requests.Reviews;

public sealed record UpdateReviewRequest(
    int Rating,
    string Title,
    string Content);