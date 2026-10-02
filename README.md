# Hacker News Best Stories API

ASP.NET Core API that returns the best *n* stories from Hacker News, sorted by score.

## Running it

Requires the .NET 10 SDK.

```bash
dotnet run --project Hackernews.Api
```

The API listens on `http://localhost:5204`.

```bash
curl "http://localhost:5204/api/stories/best?n=10"
```

To run the tests:

```bash
dotnet test
```

## Endpoint

`GET /api/stories/best?n={n}`

`n` is required and must be between 1 and 200.

```json
[
  {
    "title": "A uBlock Origin update was rejected from the Chrome Web Store",
    "uri": "https://github.com/uBlockOrigin/uBlock-issues/issues/745",
    "postedBy": "ismaildonmez",
    "time": "2019-10-12T13:43:01+00:00",
    "score": 1716,
    "commentCount": 572
  }
]
```

| Status | When |
|--------|------|
| 200 | Stories returned, highest score first |
| 400 | `n` is missing, not a number, or out of range |
| 503 | The first load from Hacker News has not finished yet (includes `Retry-After`) |

Errors are returned as Problem Details.

## How it works

Incoming requests never call Hacker News. A background service loads the best stories on a timer, sorts them by score and publishes the result as an in-memory snapshot. The endpoint only reads the first *n* items of that snapshot.

This means the load on Hacker News is constant (one pass over the list per refresh interval) no matter how many requests the API receives, and serving a request is a memory read.

Other details:

- Item requests are throttled, so a refresh never has more than `MaxConcurrentRequests` calls in flight.
- The HTTP client uses the standard resilience handler: retries with backoff, timeouts and a circuit breaker.
- If a refresh fails, the previous snapshot stays in place and the error is logged.
- Swapping the snapshot is a single reference assignment, so readers need no locks.

## Configuration

In `appsettings.json`, under `HackerNews`:

| Setting | Default | Description |
|---------|---------|-------------|
| `BaseUrl` | `https://hacker-news.firebaseio.com/v0/` | Hacker News API base URL |
| `RefreshInterval` | `00:01:00` | Time between refreshes |
| `MaxConcurrentRequests` | `10` | Max parallel item requests during a refresh |

Settings are validated at startup.

## Assumptions

- "Best stories" means the list returned by `beststories.json` (up to 200 IDs). That list is not strictly ordered by score, so all items are loaded and sorted here.
- Data up to one refresh interval old is acceptable. Scores change all the time, so some staleness is unavoidable anyway.
- If any item fails to load after retries, the whole refresh is discarded. I preferred a slightly older complete snapshot over a newer incomplete one.
- `uri` can be `null`: stories like "Ask HN" have no URL.

## With more time

- Distributed cache (Redis) so several instances share one snapshot and only one of them refreshes. Right now each instance refreshes on its own.
- Health checks, with readiness tied to the first successful load, so a load balancer does not route traffic to a cold instance.
- Rate limiting on the incoming side.
- Only re-fetch items whose data is likely to have changed, or use the Hacker News `updates` endpoint, to reduce calls per refresh.
- `ETag` / `Cache-Control` headers so clients and proxies can cache responses.