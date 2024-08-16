using Elastic.Apm.DiagnosticSource;
using IoT.AlertManagementApi.Models;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Diagnostics;
using System.Text.Json;

var activitySource = new ActivitySource("IoT.AlertManagementApi");

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddRabbitMQClient("bus");

builder.Services.AddElasticApmForAspNetCore(new HttpDiagnosticsSubscriber());

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/", (AlertModel alert, [FromServices] IConnection connection) =>
{
    var activity = activitySource.StartActivity("PostMessage");
    activity?.SetTag("DeviceId", alert.DeviceId);
    activity?.SetTag("AlertType", alert.AlertType);
    activity?.SetTag("Message", alert.Message);

    var queueName = "alerts";
    using var channel = connection.CreateModel();
    channel.QueueDeclare(queueName, exclusive: false, durable: true);
    channel.BasicPublish(exchange: "", queueName, null, body: JsonSerializer.SerializeToUtf8Bytes(alert));

    return TypedResults.Ok("Message sent to RabbitMQ Queue");
})
.WithName("Post Message")
.WithOpenApi();

app.Run();