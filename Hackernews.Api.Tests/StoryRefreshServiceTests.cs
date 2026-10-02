using Hackernews.Api.Clients;
using Hackernews.Api.Models;
using Hackernews.Api.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Hackernews.Api.Tests;

public class StoryRefreshServiceTests
{
    private readonly FakeHackerNewsClient _client = new();
    private readonly StoryCache _cache = new();

    [Fact]
    public async Task RefreshAsync_orders_stories_by_score_descending()
    {
        _client.Add(Item(1, score: 10), Item(2, score: 300), Item(3, score: 50));

        await CreateService().RefreshAsync(CancellationToken.None);

        Assert.Equal(new[] { 300, 50, 10 }, _cache.Stories!.Select(s => s.Score));
    }

    [Fact]
    public async Task RefreshAsync_skips_items_that_no_longer_exist()
    {
        _client.Add(Item(1, score: 10));
        _client.Ids.Add(999);

        await CreateService().RefreshAsync(CancellationToken.None);

        Assert.Single(_cache.Stories!);
    }

    [Fact]
    public async Task RefreshAsync_keeps_previous_snapshot_when_hacker_news_fails()
    {
        var previous = new[] { new Story("Old", null, "someone", DateTimeOffset.UnixEpoch, 1, 0) };
        _cache.Update(previous);
        _client.Failure = new HttpRequestException("boom");

        await Assert.ThrowsAsync<HttpRequestException>(
            () => CreateService().RefreshAsync(CancellationToken.None));

        Assert.Same(previous, _cache.Stories);
    }

    private StoryRefreshService CreateService()
    {
        var provider = new ServiceCollection()
            .AddSingleton<IHackerNewsClient>(_client)
            .BuildServiceProvider();

        return new StoryRefreshService(
            provider.GetRequiredService<IServiceScopeFactory>(),
            _cache,
            Options.Create(new HackerNewsOptions()),
            NullLogger<StoryRefreshService>.Instance);
    }

    private static HackerNewsItem Item(long id, int score)
        => new(id, "story", $"Story {id}", null, "someone", 0, score, 0);
}