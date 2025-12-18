using FluentAssertions;
using Readlog.Api.Requests.Books;
using Readlog.Api.Requests.Reviews;
using Readlog.Api.Tests.Extensions;
using Readlog.Api.Tests.Fixtures;
using Readlog.Application.Features.Books.DTOs;
using Readlog.Application.Features.Reviews.DTOs;
using Readlog.Application.Shared;
using System.Net;
using System.Net.Http.Json;

namespace Readlog.Api.Tests.Controllers;

[Collection("Integration")]
public class ReviewsControllerTests(IntegrationTestFixture fixture) : IAsyncLifetime
{
    private HttpClient authenticatedClient = null!;
    private Guid bookId;

    public async Task InitializeAsync()
    {
        await fixture.ResetDatabaseAsync();
        authenticatedClient = await fixture.CreateAuthenticatedClientAsync();

        // Create a book for reviews
        var bookResponse = await authenticatedClient.PostAsJsonAsync("/api/books",
            new CreateBookRequest("Test Book", "Test Author"));
        var book = await bookResponse.Content.ReadFromJsonAsync<BookResponse>();
        bookId = book!.Id;
    }

    public Task DisposeAsync()
    {
        authenticatedClient.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CreateReview_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateReviewRequest(5, "Amazing!", "This book changed my life.");

        // Act
        var response = await authenticatedClient.PostAsJsonAsync(
            $"/api/books/{bookId}/reviews", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var review = await response.Content.ReadFromJsonAsync<ReviewResponse>();
        review.Should().NotBeNull();
        review!.Rating.Should().Be(5);
        review.Title.Should().Be("Amazing!");
        review.Content.Should().Be("This book changed my life.");
        review.BookId.Should().Be(bookId);
    }

    [Fact]
    public async Task CreateReview_ForNonExistingBook_ShouldReturnNotFound()
    {
        // Arrange
        var request = new CreateReviewRequest(5, "Title", "Content");

        // Act
        var response = await authenticatedClient.PostAsJsonAsync(
            $"/api/books/{Guid.NewGuid()}/reviews", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateReview_DuplicateByUser_ShouldReturnConflict()
    {
        // Arrange
        var request = new CreateReviewRequest(5, "First Review", "Content");
        await authenticatedClient.PostAsJsonAsync($"/api/books/{bookId}/reviews", request);

        var duplicateRequest = new CreateReviewRequest(4, "Second Review", "Different content");

        // Act
        var response = await authenticatedClient.PostAsJsonAsync(
            $"/api/books/{bookId}/reviews", duplicateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Theory]
    [InlineData(0, "Title", "Content")]
    [InlineData(6, "Title", "Content")]
    [InlineData(5, "", "Content")]
    [InlineData(5, "Title", "")]
    public async Task CreateReview_WithInvalidData_ShouldReturnBadRequest(
        int rating,
        string title,
        string content)
    {
        // Arrange
        var request = new CreateReviewRequest(rating, title, content);

        // Act
        var response = await authenticatedClient.PostAsJsonAsync(
            $"/api/books/{bookId}/reviews", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetReviewsByBook_ShouldReturnPagedResult()
    {
        // Arrange - Create reviews from different users
        var client1 = await fixture.CreateAuthenticatedClientAsync("user1");
        var client2 = await fixture.CreateAuthenticatedClientAsync("user2");

        await client1.PostAsJsonAsync($"/api/books/{bookId}/reviews",
            new CreateReviewRequest(5, "Review 1", "Content 1"));
        await client2.PostAsJsonAsync($"/api/books/{bookId}/reviews",
            new CreateReviewRequest(4, "Review 2", "Content 2"));

        // Act
        var response = await authenticatedClient.GetAsync($"/api/books/{bookId}/reviews");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<ReviewResponse>>();
        result!.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetReviewsByBook_WithSorting_ShouldReturnSortedResults()
    {
        // Arrange
        var client1 = await fixture.CreateAuthenticatedClientAsync("sortuser1");
        var client2 = await fixture.CreateAuthenticatedClientAsync("sortuser2");
        var client3 = await fixture.CreateAuthenticatedClientAsync("sortuser3");

        await client1.PostAsJsonAsync($"/api/books/{bookId}/reviews",
            new CreateReviewRequest(3, "Review", "Content"));
        await client2.PostAsJsonAsync($"/api/books/{bookId}/reviews",
            new CreateReviewRequest(5, "Review", "Content"));
        await client3.PostAsJsonAsync($"/api/books/{bookId}/reviews",
            new CreateReviewRequest(1, "Review", "Content"));

        // Act
        var response = await authenticatedClient.GetAsync(
            $"/api/books/{bookId}/reviews?sortBy=rating&sortDescending=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<ReviewResponse>>();
        result!.Items[0].Rating.Should().Be(5);
        result.Items[1].Rating.Should().Be(3);
        result.Items[2].Rating.Should().Be(1);
    }

    [Fact]
    public async Task UpdateReview_OwnReview_ShouldReturnOk()
    {
        // Arrange
        var createResponse = await authenticatedClient.PostAsJsonAsync(
            $"/api/books/{bookId}/reviews",
            new CreateReviewRequest(3, "Old Title", "Old Content"));
        var review = await createResponse.Content.ReadFromJsonAsync<ReviewResponse>();

        var updateRequest = new UpdateReviewRequest(5, "New Title", "New Content");

        // Act
        var response = await authenticatedClient.PutAsJsonAsync(
            $"/api/books/{bookId}/reviews/{review!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedReview = await response.Content.ReadFromJsonAsync<ReviewResponse>();
        updatedReview!.Rating.Should().Be(5);
        updatedReview.Title.Should().Be("New Title");
    }

    [Fact]
    public async Task UpdateReview_OtherUsersReview_ShouldReturnUnauthorized()
    {
        // Arrange - Create review with first user
        var otherClient = await fixture.CreateAuthenticatedClientAsync("otheruser");
        var createResponse = await otherClient.PostAsJsonAsync(
            $"/api/books/{bookId}/reviews",
            new CreateReviewRequest(5, "Title", "Content"));
        var review = await createResponse.Content.ReadFromJsonAsync<ReviewResponse>();

        // Act - Try to update with different user
        var updateRequest = new UpdateReviewRequest(1, "Hacked", "Hacked");
        var response = await authenticatedClient.PutAsJsonAsync(
            $"/api/books/{bookId}/reviews/{review!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteReview_OwnReview_ShouldReturnNoContent()
    {
        // Arrange
        var createResponse = await authenticatedClient.PostAsJsonAsync(
            $"/api/books/{bookId}/reviews",
            new CreateReviewRequest(5, "To Delete", "Content"));
        var review = await createResponse.Content.ReadFromJsonAsync<ReviewResponse>();

        // Act
        var response = await authenticatedClient.DeleteAsync(
            $"/api/books/{bookId}/reviews/{review!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteReview_OtherUsersReview_ShouldReturnUnauthorized()
    {
        // Arrange
        var otherClient = await fixture.CreateAuthenticatedClientAsync("deleteother");
        var createResponse = await otherClient.PostAsJsonAsync(
            $"/api/books/{bookId}/reviews",
            new CreateReviewRequest(5, "Title", "Content"));
        var review = await createResponse.Content.ReadFromJsonAsync<ReviewResponse>();

        // Act
        var response = await authenticatedClient.DeleteAsync(
            $"/api/books/{bookId}/reviews/{review!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
