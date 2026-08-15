var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/users", async (IHttpClientFactory httpClientFactory) =>
{
    var baseUrl = builder.Configuration["Services:Users:BaseUrl"];

    var client = httpClientFactory.CreateClient();
    var result = await client.GetStringAsync(baseUrl + "/users");
    return result;
});

app.MapGet("/user/{id}", async (IHttpClientFactory httpClientFactory, int id) =>
{
    var baseUrl = builder.Configuration["Services:Users:BaseUrl"];

    var client = httpClientFactory.CreateClient();
    var result = await client.GetStringAsync(baseUrl + "/user/" + id);
    return result;
});

app.Run();
