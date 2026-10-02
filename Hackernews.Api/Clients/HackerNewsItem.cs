namespace Hackernews.Api.Clients;

public sealed record HackerNewsItem(
    long Id,
    string? Type,
    string? Title,
    string? Url,
    string? By,
    long Time,
    int Score,
    int Descendants);