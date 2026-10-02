namespace Hackernews.Api.Clients;

public interface IHackerNewsClient
{
    Task<IReadOnlyList<long>> GetBestStoryIdsAsync(CancellationToken ct = default);
    Task<HackerNewsItem?> GetItemAsync(long id, CancellationToken ct = default);
}

public sealed class HackerNewsClient(HttpClient http) : IHackerNewsClient
{
    public async Task<IReadOnlyList<long>> GetBestStoryIdsAsync(CancellationToken ct = default)
        => await http.GetFromJsonAsync<long[]>("beststories.json", ct) ?? [];

    public Task<HackerNewsItem?> GetItemAsync(long id, CancellationToken ct = default)
        => http.GetFromJsonAsync<HackerNewsItem>($"item/{id}.json", ct);
}