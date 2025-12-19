using FluentAssertions;
using Readlog.Api.Requests.Books;
using Readlog.Api.Requests.ReadingLists;
using Readlog.Api.Tests.Extensions;
using Readlog.Api.Tests.Fixtures;
using Readlog.Application.Features.Books.DTOs;
using Readlog.Application.Features.ReadingLists.DTOs;
using Readlog.Application.Shared;
using Readlog.Domain.Enums;
using System.Net;
using System.Net.Http.Json;

namespace Readlog.Api.Tests.Controllers;

[Collection("Integration")]
public class ReadingListsControllerTests(IntegrationTestFixture fixture) : IAsyncLifetime
{
    private HttpClient authenticatedClient = null!;

    public async Task InitializeAsync()
    {
        await fixture.ResetDatabaseAsync();
        authenticatedClient = await fixture.CreateAuthenticatedClientAsync();
    }

    public Task DisposeAsync()
    {
        authenticatedClient.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CreateReadingList_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateReadingListRequest("My Favorites");

        // Act
        var response = await authenticatedClient.PostAsJsonAsync("/api/reading-lists", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var readingList = await response.Content.ReadFromJsonAsync<ReadingListResponse>();
        readingList.Should().NotBeNull();
        readingList!.Name.Should().Be("My Favorites");
        readingList.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateReadingList_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateReadingListRequest(string.Empty);

        // Act
        var response = await authenticatedClient.PostAsJsonAsync("/api/reading-lists", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetUserReadingLists_ShouldReturnOnlyOwnLists()
    {
        // Arrange
        await authenticatedClient.PostAsJsonAsync("/api/reading-lists",
            new CreateReadingListRequest("List 1"));
        await authenticatedClient.PostAsJsonAsync("/api/reading-lists",
            new CreateReadingListRequest("List 2"));

        // Create list with different user
        var otherClient = await fixture.CreateAuthenticatedClientAsync();
        await otherClient.PostAsJsonAsync("/api/reading-lists",
            new CreateReadingListRequest("Other User's List"));

        // Act
        var response = await authenticatedClient.GetAsync("/api/reading-lists");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<ReadingListResponse>>();
        result!.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(rl =>
            rl.Name.Should().NotBe("Other User's List"));
    }

    [Fact]
    public async Task GetUserReadingLists_WithSearch_ShouldFilterResults()
    {
        // Arrange
        await authenticatedClient.PostAsJsonAsync("/api/reading-lists",
            new CreateReadingListRequest("Fantasy Books"));
        await authenticatedClient.PostAsJsonAsync("/api/reading-lists",
            new CreateReadingListRequest("Science Fiction"));
        await authenticatedClient.PostAsJsonAsync("/api/reading-lists",
            new CreateReadingListRequest("Fantasy Movies")); // Not a match for "books"

        // Act
        var response = await authenticatedClient.GetAsync("/api/reading-lists?search=fantasy");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<ReadingListResponse>>();
        result!.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetReadingListById_OwnList_ShouldReturnList()
    {
        // Arrange
        var createResponse = await authenticatedClient.PostAsJsonAsync("/api/reading-lists",
            new CreateReadingListRequest("My List"));
        var created = await createResponse.Content.ReadFromJsonAsync<ReadingListResponse>();

        // Act
        var response = await authenticatedClient.GetAsync($"/api/reading-lists/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var readingList = await response.Content.ReadFromJsonAsync<ReadingListResponse>();
        readingList!.Name.Should().Be("My List");
    }

    [Fact]
    public async Task GetReadingListById_OtherUsersList_ShouldReturnUnauthorized()
    {
        // Arrange
        var otherClient = await fixture.CreateAuthenticatedClientAsync();
        var createResponse = await otherClient.PostAsJsonAsync("/api/reading-lists",
            new CreateReadingListRequest("Private List"));
        var created = await createResponse.Content.ReadFromJsonAsync<ReadingListResponse>();

        // Act
        var response = await authenticatedClient.GetAsync($"/api/reading-lists/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AddBookToReadingList_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var listResponse = await authenticatedClient.PostAsJsonAsync("/api/reading-lists",
            new CreateReadingListRequest("My List"));
        var list = await listResponse.Content.ReadFromJsonAsync<ReadingListResponse>();

        var bookResponse = await authenticatedClient.PostAsJsonAsync("/api/books",
            new CreateBookRequest("Test Book", "Author"));
        var book = await bookResponse.Content.ReadFromJsonAsync<BookResponse>();

        var request = new AddBookToReadingListRequest(book!.Id, ReadingStatus.WantToRead);

        // Act
        var response = await authenticatedClient.PostAsJsonAsync(
            $"/api/reading-lists/{list!.Id}/books", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedList = await response.Content.ReadFromJsonAsync<ReadingListResponse>();
        updatedList!.Items.Should().HaveCount(1);
        updatedList.Items[0].BookId.Should().Be(book.Id);
        updatedList.Items[0].Status.Should().Be(ReadingStatus.WantToRead);
    }

    [Fact]
    public async Task AddBookToReadingList_DuplicateBook_ShouldReturnConflict()
    {
        // Arrange
        var listResponse = await authenticatedClient.PostAsJsonAsync("/api/reading-lists",
            new CreateReadingListRequest("My List"));
        var list = await listResponse.Content.ReadFromJsonAsync<ReadingListResponse>();

        var bookResponse = await authenticatedClient.PostAsJsonAsync("/api/books",
            new CreateBookRequest("Test Book", "Author"));
        var book = await bookResponse.Content.ReadFromJsonAsync<BookResponse>();

        var request = new AddBookToReadingListRequest(book!.Id);
        await authenticatedClient.PostAsJsonAsync($"/api/reading-lists/{list!.Id}/books", request);

        // Act - Add same book again
        var response = await authenticatedClient.PostAsJsonAsync(
            $"/api/reading-lists/{list.Id}/books", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AddBookToReadingList_NonExistingBook_ShouldReturnNotFound()
    {
        // Arrange
        var listResponse = await authenticatedClient.PostAsJsonAsync("/api/reading-lists",
            new CreateReadingListRequest("My List"));
        var list = await listResponse.Content.ReadFromJsonAsync<ReadingListResponse>();

        var request = new AddBookToReadingListRequest(Guid.NewGuid());

        // Act
        var response = await authenticatedClient.PostAsJsonAsync(
            $"/api/reading-lists/{list!.Id}/books", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddBookToReadingList_OtherUsersList_ShouldReturnUnauthorized()
    {
        // Arrange
        var otherClient = await fixture.CreateAuthenticatedClientAsync();
        var listResponse = await otherClient.PostAsJsonAsync("/api/reading-lists",
            new CreateReadingListRequest("Private List"));
        var list = await listResponse.Content.ReadFromJsonAsync<ReadingListResponse>();

        var bookResponse = await authenticatedClient.PostAsJsonAsync("/api/books",
            new CreateBookRequest("Test Book", "Author"));
        var book = await bookResponse.Content.ReadFromJsonAsync<BookResponse>();

        var request = new AddBookToReadingListRequest(book!.Id);

        // Act
        var response = await authenticatedClient.PostAsJsonAsync(
            $"/api/reading-lists/{list!.Id}/books", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateBookStatus_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var listResponse = await authenticatedClient.PostAsJsonAsync("/api/reading-lists",
            new CreateReadingListRequest("My List"));
        var list = await listResponse.Content.ReadFromJsonAsync<ReadingListResponse>();

        var bookResponse = await authenticatedClient.PostAsJsonAsync("/api/books",
            new CreateBookRequest("Test Book", "Author"));
        var book = await bookResponse.Content.ReadFromJsonAsync<BookResponse>();

        await authenticatedClient.PostAsJsonAsync($"/api/reading-lists/{list!.Id}/books",
            new AddBookToReadingListRequest(book!.Id, ReadingStatus.WantToRead));

        var updateRequest = new UpdateBookStatusRequest(ReadingStatus.Finished);

        // Act
        var response = await authenticatedClient.PutAsJsonAsync(
            $"/api/reading-lists/{list.Id}/books/{book.Id}/status", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedList = await response.Content.ReadFromJsonAsync<ReadingListResponse>();
        updatedList!.Items[0].Status.Should().Be(ReadingStatus.Finished);
    }

    [Fact]
    public async Task RemoveBookFromReadingList_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var listResponse = await authenticatedClient.PostAsJsonAsync("/api/reading-lists",
            new CreateReadingListRequest("My List"));
        var list = await listResponse.Content.ReadFromJsonAsync<ReadingListResponse>();

        var bookResponse = await authenticatedClient.PostAsJsonAsync("/api/books",
            new CreateBookRequest("Test Book", "Author"));
        var book = await bookResponse.Content.ReadFromJsonAsync<BookResponse>();

        await authenticatedClient.PostAsJsonAsync($"/api/reading-lists/{list!.Id}/books",
            new AddBookToReadingListRequest(book!.Id));

        // Act
        var response = await authenticatedClient.DeleteAsync(
            $"/api/reading-lists/{list.Id}/books/{book.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedList = await response.Content.ReadFromJsonAsync<ReadingListResponse>();
        updatedList!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task RenameReadingList_OwnList_ShouldReturnOk()
    {
        // Arrange
        var createResponse = await authenticatedClient.PostAsJsonAsync("/api/reading-lists",
            new CreateReadingListRequest("Old Name"));
        var list = await createResponse.Content.ReadFromJsonAsync<ReadingListResponse>();

        var updateRequest = new RenameReadingListRequest("New Name");

        // Act
        var response = await authenticatedClient.PutAsJsonAsync(
            $"/api/reading-lists/{list!.Id}/rename", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedList = await response.Content.ReadFromJsonAsync<ReadingListResponse>();
        updatedList!.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task DeleteReadingList_OwnList_ShouldReturnNoContent()
    {
        // Arrange
        var createResponse = await authenticatedClient.PostAsJsonAsync("/api/reading-lists",
            new CreateReadingListRequest("To Delete"));
        var list = await createResponse.Content.ReadFromJsonAsync<ReadingListResponse>();

        // Act
        var response = await authenticatedClient.DeleteAsync($"/api/reading-lists/{list!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it's deleted
        var getResponse = await authenticatedClient.GetAsync($"/api/reading-lists/{list.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteReadingList_OtherUsersList_ShouldReturnUnauthorized()
    {
        // Arrange
        var otherClient = await fixture.CreateAuthenticatedClientAsync();
        var createResponse = await otherClient.PostAsJsonAsync("/api/reading-lists",
            new CreateReadingListRequest("Private List"));
        var list = await createResponse.Content.ReadFromJsonAsync<ReadingListResponse>();

        // Act
        var response = await authenticatedClient.DeleteAsync($"/api/reading-lists/{list!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
