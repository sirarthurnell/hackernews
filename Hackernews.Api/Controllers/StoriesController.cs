using Hackernews.Api.Models;
using Hackernews.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hackernews.Api.Controllers;

[ApiController]
[Route("api/stories")]
public sealed class StoriesController(IStoryCache cache) : ControllerBase
{
    [HttpGet("best")]
    public ActionResult<IEnumerable<Story>> GetBest([FromQuery] int n)
    {
        IReadOnlyList<Story> stories = cache.Stories ?? [];
        return Ok(stories.Take(n));
    }
}