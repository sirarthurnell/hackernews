using Hackernews.Api.Clients;
using Hackernews.Api.Models;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace Hackernews.Api.Services;

public sealed class StoryRefreshService(
    IServiceScopeFactory scopeFactory,
    IStoryCache cache,
    IOptions<HackerNewsOptions> options,
    ILogger<StoryRefreshService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(options.Value.RefreshInterval);

        do
        {
            try
            {
                await RefreshAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Failed to refresh best stories, keeping previous snapshot");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RefreshAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<IHackerNewsClient>();

        var ids = await client.GetBestStoryIdsAsync(ct);
        var items = new ConcurrentBag<HackerNewsItem>();

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = options.Value.MaxConcurrentRequests,
            CancellationToken = ct
        };

        await Parallel.ForEachAsync(ids, parallelOptions, async (id, token) =>
        {
            if (await client.GetItemAsync(id, token) is { } item)
            {
                items.Add(item);
            }
        });

        var stories = items
            .OrderByDescending(item => item.Score)
            .Select(Story.From)
            .ToArray();

        cache.Update(stories);
        logger.LogInformation("Refreshed {Count} best stories", stories.Length);
    }
}