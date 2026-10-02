using System.ComponentModel.DataAnnotations;

namespace Hackernews.Api.Clients;

public sealed class HackerNewsOptions
{
    public const string SectionName = "HackerNews";

    [Required, Url]
    public string BaseUrl { get; set; } = "https://hacker-news.firebaseio.com/v0/";

    [Range(typeof(TimeSpan), "00:00:05", "01:00:00")]
    public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromMinutes(1);

    [Range(1, 50)]
    public int MaxConcurrentRequests { get; set; } = 10;
}