using System.ComponentModel.DataAnnotations;

namespace Readlog.Api.Requests.Reviews;

/// <summary>
/// Request to create a book review
/// </summary>
/// <param name="Rating">Rating from 1 to 5 stars</param>
/// <param name="Title">Review title (required, max 200 characters)</param>
/// <param name="Content">Review content (required, max 5000 characters)</param>
public sealed record CreateReviewRequest(
    [Required]
    [Range(1, 5)]
    int Rating,

    [Required]
    [StringLength(200)]
    string Title,

    [Required]
    [StringLength(5000)]
    string Content);
