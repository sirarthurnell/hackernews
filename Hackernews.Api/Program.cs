using Hackernews.Api.Clients;
using Hackernews.Api.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.Configure<HackerNewsOptions>(builder.Configuration.GetSection(HackerNewsOptions.SectionName));
builder.Services.AddHttpClient<IHackerNewsClient, HackerNewsClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<HackerNewsOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
});

builder.Services.AddScoped<IBestStoriesService, BestStoriesService>();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
