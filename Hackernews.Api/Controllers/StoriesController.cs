using Hackernews.Api.Models;
using Hackernews.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hackernews.Api.Controllers;

[ApiController]
[Route("api/stories")]
public sealed class StoriesController(IBestStoriesService stories) : ControllerBase
{
    [HttpGet("best")]
    public async Task<ActionResult<IReadOnlyList<Story>>> GetBest(
        [FromQuery] int n, CancellationToken ct)
        => Ok(await stories.GetBestAsync(n, ct));
}