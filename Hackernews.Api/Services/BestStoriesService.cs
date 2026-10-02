using Hackernews.Api.Clients;
using Hackernews.Api.Models;

namespace Hackernews.Api.Services;

public interface IBestStoriesService
{
    Task<IReadOnlyList<Story>> GetBestAsync(int count, CancellationToken ct = default);
}

public sealed class BestStoriesService(IHackerNewsClient client) : IBestStoriesService
{
    public async Task<IReadOnlyList<Story>> GetBestAsync(int count, CancellationToken ct = default)
    {
        var ids = await client.GetBestStoryIdsAsync(ct);
        var items = await Task.WhenAll(ids.Select(id => client.GetItemAsync(id, ct)));

        return items
            .OfType<HackerNewsItem>()
            .OrderByDescending(item => item.Score)
            .Take(count)
            .Select(Story.From)
            .ToList();
    }
}