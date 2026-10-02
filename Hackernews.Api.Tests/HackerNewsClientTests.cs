using System.Net;
using System.Text;
using Hackernews.Api.Clients;
using Hackernews.Api.Models;

namespace Hackernews.Api.Tests;

public class HackerNewsClientTests
{
    [Fact]
    public async Task GetItemAsync_reads_story_and_maps_to_response_shape()
    {
        const string json = """
            {
              "by": "ismaildonmez",
              "descendants": 572,
              "id": 21233041,
              "score": 1716,
              "time": 1570887781,
              "title": "A uBlock Origin update was rejected from the Chrome Web Store",
              "type": "story",
              "url": "https://github.com/uBlockOrigin/uBlock-issues/issues/745"
            }
            """;

        var item = await CreateClient(json).GetItemAsync(21233041);
        var story = Story.From(item!);

        Assert.Equal("A uBlock Origin update was rejected from the Chrome Web Store", story.Title);
        Assert.Equal("https://github.com/uBlockOrigin/uBlock-issues/issues/745", story.Uri);
        Assert.Equal("ismaildonmez", story.PostedBy);
        Assert.Equal(new DateTimeOffset(2019, 10, 12, 13, 43, 1, TimeSpan.Zero), story.Time);
        Assert.Equal(1716, story.Score);
        Assert.Equal(572, story.CommentCount);
    }

    [Fact]
    public async Task GetItemAsync_returns_null_when_item_does_not_exist()
    {
        var item = await CreateClient("null").GetItemAsync(1);

        Assert.Null(item);
    }

    private static HackerNewsClient CreateClient(string json)
    {
        var handler = new StubHandler(json);
        return new HackerNewsClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://hn.test/v0/")
        });
    }

    private sealed class StubHandler(string json) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
    }
}