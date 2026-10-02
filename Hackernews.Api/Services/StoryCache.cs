using Hackernews.Api.Models;

namespace Hackernews.Api.Services;

public interface IStoryCache
{
    /// <summary>
    /// Stories sorted by score, or null until the first refresh completes.
    /// </summary>
    IReadOnlyList<Story>? Stories { get; }

    void Update(IReadOnlyList<Story> stories);
}

public sealed class StoryCache : IStoryCache
{
    private volatile IReadOnlyList<Story>? _stories;

    public IReadOnlyList<Story>? Stories => _stories;

    public void Update(IReadOnlyList<Story> stories) => _stories = stories;
}