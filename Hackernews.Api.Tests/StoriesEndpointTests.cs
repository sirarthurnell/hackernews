using System.Net;
using System.Net.Http.Json;
using Hackernews.Api.Models;
using Hackernews.Api.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Hackernews.Api.Tests;

public class StoriesEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public StoriesEndpointTests(WebApplicationFactory<Program> factory)
    {
        // Keep the tests offline: no background refresh against the real API.
        _factory = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            var refresher = services.Single(d => d.ImplementationType == typeof(StoryRefreshService));
            services.Remove(refresher);
        }));
    }

    [Fact]
    public async Task Returns_503_until_first_load_completes()
    {
        var response = await _factory.CreateClient().GetAsync("/api/stories/best?n=5");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.NotNull(response.Headers.RetryAfter);
    }

    [Fact]
    public async Task Returns_first_n_stories_from_cache()
    {
        SeedCache("First", "Second", "Third");

        var stories = await _factory.CreateClient()
            .GetFromJsonAsync<Story[]>("/api/stories/best?n=2");

        Assert.Equal(new[] { "First", "Second" }, stories!.Select(s => s.Title));
    }

    [Theory]
    [InlineData("?n=0")]
    [InlineData("?n=201")]
    [InlineData("?n=abc")]
    [InlineData("")]
    public async Task Returns_400_for_invalid_n(string query)
    {
        SeedCache("First");

        var response = await _factory.CreateClient().GetAsync($"/api/stories/best{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private void SeedCache(params string[] titles)
    {
        var stories = titles
            .Select((title, index) => new Story(
                title, null, "someone", DateTimeOffset.UnixEpoch, 100 - index, 0))
            .ToArray();

        _factory.Services.GetRequiredService<IStoryCache>().Update(stories);
    }
}