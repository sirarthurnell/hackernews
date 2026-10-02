using Hackernews.Api.Clients;

namespace Hackernews.Api.Models;

public sealed record Story(
    string? Title,
    string? Uri,
    string? PostedBy,
    DateTimeOffset Time,
    int Score,
    int CommentCount)
{
    public static Story From(HackerNewsItem item) => new(
        item.Title,
        item.Url,
        item.By,
        DateTimeOffset.FromUnixTimeSeconds(item.Time),
        item.Score,
        item.Descendants);
}