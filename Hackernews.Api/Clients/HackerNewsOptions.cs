namespace Hackernews.Api.Clients;

public sealed class HackerNewsOptions
{
    public const string SectionName = "HackerNews";
    public string BaseUrl { get; set; } = "https://hacker-news.firebaseio.com/v0/";
    public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromMinutes(1);
}