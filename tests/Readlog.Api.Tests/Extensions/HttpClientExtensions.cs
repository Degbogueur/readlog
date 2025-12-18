using System.Net.Http.Json;

namespace Readlog.Api.Tests.Extensions;

public static class HttpClientExtensions
{
    public static async Task<T?> GetFromJsonAsync<T>(
        this HttpClient client,
        string requestUri)
    {
        var response = await client.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    public static async Task<HttpResponseMessage> PostAsJsonAsync<T>(
        this HttpClient client,
        string requestUri,
        T value)
    {
        return await client.PostAsJsonAsync(requestUri, value, default);
    }

    public static async Task<HttpResponseMessage> PutAsJsonAsync<T>(
        this HttpClient client,
        string requestUri,
        T value)
    {
        return await client.PutAsJsonAsync(requestUri, value, default);
    }
}
