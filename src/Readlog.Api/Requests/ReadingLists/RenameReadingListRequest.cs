using System.ComponentModel.DataAnnotations;

namespace Readlog.Api.Requests.ReadingLists;

/// <summary>
/// Request to rename an existing reading list
/// </summary>
/// <param name="Name">New reading list name (required, max 100 characters)</param>
public sealed record RenameReadingListRequest(
    [Required]
    [StringLength(100, MinimumLength = 1)]
    string Name);
