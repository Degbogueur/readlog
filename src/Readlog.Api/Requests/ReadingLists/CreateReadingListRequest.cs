using System.ComponentModel.DataAnnotations;

namespace Readlog.Api.Requests.ReadingLists;

/// <summary>
/// Request to create a new reading list
/// </summary>
/// <param name="Name">Reading list name (required, max 100 characters)</param>
public sealed record CreateReadingListRequest(
    [Required]
    [StringLength(100, MinimumLength = 1)]
    string Name);
