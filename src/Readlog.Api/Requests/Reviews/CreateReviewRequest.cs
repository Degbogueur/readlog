namespace Readlog.Api.Requests.Reviews;

public sealed record CreateReviewRequest(
    int Rating,
    string Title,
    string Content);
