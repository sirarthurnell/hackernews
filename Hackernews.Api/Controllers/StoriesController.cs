using System.ComponentModel.DataAnnotations;
using Hackernews.Api.Models;
using Hackernews.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Hackernews.Api.Controllers;

[ApiController]
[Route("api/stories")]
public sealed class StoriesController(IStoryCache cache) : ControllerBase
{
    // The Hacker News best stories list holds at most 200 items.
    private const int MaxStories = 200;

    [HttpGet("best")]
    [ProducesResponseType<IEnumerable<Story>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public ActionResult<IEnumerable<Story>> GetBest(
        [FromQuery, BindRequired, Range(1, MaxStories)] int n)
    {
        if (cache.Stories is not { } stories)
        {
            Response.Headers.RetryAfter = "5";
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Stories are not available yet",
                detail: "The first load from Hacker News has not completed. Try again shortly.");
        }

        return Ok(stories.Take(n));
    }
}