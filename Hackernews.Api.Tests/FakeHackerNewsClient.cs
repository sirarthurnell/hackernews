using Hackernews.Api.Clients;

namespace Hackernews.Api.Tests;

internal sealed class FakeHackerNewsClient : IHackerNewsClient
{
    public List<long> Ids { get; } = [];
    public Dictionary<long, HackerNewsItem> Items { get; } = [];
    public Exception? Failure { get; set; }

    public void Add(params HackerNewsItem[] items)
    {
        foreach (var item in items)
        {
            Ids.Add(item.Id);
            Items[item.Id] = item;
        }
    }

    public Task<IReadOnlyList<long>> GetBestStoryIdsAsync(CancellationToken ct = default)
        => Failure is null
            ? Task.FromResult<IReadOnlyList<long>>(Ids)
            : Task.FromException<IReadOnlyList<long>>(Failure);

    public Task<HackerNewsItem?> GetItemAsync(long id, CancellationToken ct = default)
        => Task.FromResult(Items.GetValueOrDefault(id));
}