using FluentAssertions;
using Readlog.Api.Requests.Books;
using Readlog.Api.Tests.Extensions;
using Readlog.Api.Tests.Fixtures;
using Readlog.Application.Features.Books.DTOs;
using Readlog.Application.Shared;
using System.Net;
using System.Net.Http.Json;

namespace Readlog.Api.Tests.Controllers;

[Collection("Integration")]
public class BooksControllerTests(IntegrationTestFixture fixture) : IAsyncLifetime
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
    public async Task CreateBook_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateBookRequest(
            "Clean Code",
            "Robert C. Martin",
            "978-0-13-235088-4",
            "A handbook of agile software craftsmanship",
            new DateOnly(2008, 8, 1));

        // Act
        var response = await authenticatedClient.PostAsJsonAsync("/api/books", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var book = await response.Content.ReadFromJsonAsync<BookResponse>();
        book.Should().NotBeNull();
        book!.Id.Should().NotBeEmpty();
        book.Title.Should().Be("Clean Code");
        book.Author.Should().Be("Robert C. Martin");
        book.Isbn.Should().Be("9780132350884");

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain(book.Id.ToString());
    }

    [Fact]
    public async Task CreateBook_WithMinimalData_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateBookRequest("Title", "Author");

        // Act
        var response = await authenticatedClient.PostAsJsonAsync("/api/books", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var book = await response.Content.ReadFromJsonAsync<BookResponse>();
        book!.Isbn.Should().BeNull();
        book.Description.Should().BeNull();
        book.PublishedDate.Should().BeNull();
    }

    [Fact]
    public async Task CreateBook_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new CreateBookRequest("Title", "Author");

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/books", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("", "Author")]
    [InlineData("Title", "")]
    public async Task CreateBook_WithInvalidData_ShouldReturnBadRequest(string title, string author)
    {
        // Arrange
        var request = new CreateBookRequest(title, author);

        // Act
        var response = await authenticatedClient.PostAsJsonAsync("/api/books", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetBookById_WithExistingBook_ShouldReturnBook()
    {
        // Arrange
        var createResponse = await authenticatedClient.PostAsJsonAsync("/api/books",
            new CreateBookRequest("Test Book", "Test Author"));
        var createdBook = await createResponse.Content.ReadFromJsonAsync<BookResponse>();

        // Act
        var response = await authenticatedClient.GetAsync($"/api/books/{createdBook!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var book = await response.Content.ReadFromJsonAsync<BookResponse>();
        book.Should().NotBeNull();
        book!.Id.Should().Be(createdBook.Id);
        book.Title.Should().Be("Test Book");
    }

    [Fact]
    public async Task GetBookById_WithNonExistingBook_ShouldReturnNotFound()
    {
        // Act
        var response = await authenticatedClient.GetAsync($"/api/books/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllBooks_ShouldReturnPagedResult()
    {
        // Arrange
        await authenticatedClient.PostAsJsonAsync("/api/books", new CreateBookRequest("Book 1", "Author 1"));
        await authenticatedClient.PostAsJsonAsync("/api/books", new CreateBookRequest("Book 2", "Author 2"));
        await authenticatedClient.PostAsJsonAsync("/api/books", new CreateBookRequest("Book 3", "Author 3"));

        // Act
        var response = await authenticatedClient.GetAsync("/api/books");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<BookResponse>>();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.Page.Should().Be(1);
    }

    [Fact]
    public async Task GetAllBooks_WithSearch_ShouldFilterResults()
    {
        // Arrange
        await authenticatedClient.PostAsJsonAsync("/api/books", new CreateBookRequest("Clean Code", "Robert Martin"));
        await authenticatedClient.PostAsJsonAsync("/api/books", new CreateBookRequest("The Hobbit", "J.R.R. Tolkien"));
        await authenticatedClient.PostAsJsonAsync("/api/books", new CreateBookRequest("Clean Architecture", "Robert Martin"));

        // Act
        var response = await authenticatedClient.GetAsync("/api/books?search=clean");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<BookResponse>>();
        result!.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(b =>
            b.Title.Contains("Clean", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetAllBooks_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        for (int i = 1; i <= 15; i++)
        {
            await authenticatedClient.PostAsJsonAsync("/api/books",
                new CreateBookRequest($"Book {i:D2}", $"Author {i}"));
        }

        // Act
        var response = await authenticatedClient.GetAsync("/api/books?page=2&pageSize=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<BookResponse>>();
        result!.Items.Should().HaveCount(5);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(15);
        result.TotalPages.Should().Be(3);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllBooks_WithSorting_ShouldReturnSortedResults()
    {
        // Arrange
        await authenticatedClient.PostAsJsonAsync("/api/books", new CreateBookRequest("Zebra Book", "Author"));
        await authenticatedClient.PostAsJsonAsync("/api/books", new CreateBookRequest("Alpha Book", "Author"));
        await authenticatedClient.PostAsJsonAsync("/api/books", new CreateBookRequest("Middle Book", "Author"));

        // Act
        var response = await authenticatedClient.GetAsync("/api/books?sortBy=title&sortDescending=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<BookResponse>>();
        result!.Items[0].Title.Should().Be("Alpha Book");
        result.Items[1].Title.Should().Be("Middle Book");
        result.Items[2].Title.Should().Be("Zebra Book");
    }

    [Fact]
    public async Task UpdateBook_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var createResponse = await authenticatedClient.PostAsJsonAsync("/api/books",
            new CreateBookRequest("Old Title", "Old Author"));
        var createdBook = await createResponse.Content.ReadFromJsonAsync<BookResponse>();

        var updateRequest = new UpdateBookRequest(
            "New Title",
            "New Author",
            "978-3-16-148410-0",
            "New Description",
            new DateOnly(2023, 1, 1));

        // Act
        var response = await authenticatedClient.PutAsJsonAsync(
            $"/api/books/{createdBook!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedBook = await response.Content.ReadFromJsonAsync<BookResponse>();
        updatedBook!.Title.Should().Be("New Title");
        updatedBook.Author.Should().Be("New Author");
        updatedBook.Isbn.Should().Be("9783161484100");
    }

    [Fact]
    public async Task UpdateBook_WithNonExistingBook_ShouldReturnNotFound()
    {
        // Arrange
        var updateRequest = new UpdateBookRequest("Title", "Author");

        // Act
        var response = await authenticatedClient.PutAsJsonAsync(
            $"/api/books/{Guid.NewGuid()}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteBook_WithExistingBook_ShouldReturnNoContent()
    {
        // Arrange
        var createResponse = await authenticatedClient.PostAsJsonAsync("/api/books",
            new CreateBookRequest("To Delete", "Author"));
        var createdBook = await createResponse.Content.ReadFromJsonAsync<BookResponse>();

        // Act
        var response = await authenticatedClient.DeleteAsync($"/api/books/{createdBook!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify book is soft deleted (not found)
        var getResponse = await authenticatedClient.GetAsync($"/api/books/{createdBook.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteBook_WithNonExistingBook_ShouldReturnNotFound()
    {
        // Act
        var response = await authenticatedClient.DeleteAsync($"/api/books/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
