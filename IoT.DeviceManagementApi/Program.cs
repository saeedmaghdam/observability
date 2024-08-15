using IoT.DeviceManagementApi.Models;
using IoT.ServiceDefaults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient(IoTServices.AlertManagementApi.ToString(), c => c.BaseAddress = new Uri($"https://{IoTServices.AlertManagementApi.ToUniqueId()}"));

builder.AddRedisClient("db");

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/register", async (string deviceId,
    string deviceTitle,
    [FromServices] IConnectionMultiplexer connectionMultiplexer,
    [FromServices] IHttpClientFactory httpFactory) =>
{
    var db = connectionMultiplexer.GetDatabase(0);
    db.StringSet(deviceId, deviceTitle);

    using var client = httpFactory.CreateClient(IoTServices.AlertManagementApi.ToString());
    _ = await client.PostAsJsonAsync("/", new { DeviceId = deviceId, AlertType = "Device Registered", Message = "Device has been registered" });

    return TypedResults.Ok("Successfully registered device");
})
.WithName("RegisterDevice")
.WithOpenApi();

app.MapGet("/all", ([FromServices] IConnectionMultiplexer connectionMultiplexer) =>
{
    var db = connectionMultiplexer.GetDatabase(0);
    var devices = db.Multiplexer.GetEndPoints().SelectMany(e => db.Multiplexer.GetServer(e).Keys().Select(key => key.ToString()));

    return TypedResults.Ok(devices);
})
.WithName("GetDevices")
.WithOpenApi();

app.MapPost("/{deviceId}/data", async ([FromRoute] string deviceId,
    DeviceData data,
    [FromServices] IConnectionMultiplexer connectionMultiplexer,
    [FromServices] IHttpClientFactory httpFactory) =>
{
    var db = connectionMultiplexer.GetDatabase(0);
    var deviceTitle = db.StringGet(deviceId);

    using var client = httpFactory.CreateClient(IoTServices.AlertManagementApi.ToString());
    _ = await client.PostAsJsonAsync("/", new { DeviceId = deviceId, AlertType = "Device Data Registered", Message = $"Data {data.Value} is registered for device {deviceTitle}" });

    return TypedResults.Ok("Successfully registered device");
})
.WithName("PostData")
.WithOpenApi();

app.Run();