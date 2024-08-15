using IoT.AlertManagementApi.Models;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddRabbitMQClient("bus");

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
    var queueName = "alerts";
    using var channel = connection.CreateModel();
    channel.QueueDeclare(queueName, exclusive: false, durable: true);
    channel.BasicPublish(exchange: "", queueName, null, body: JsonSerializer.SerializeToUtf8Bytes(alert));

    return TypedResults.Ok("Message sent to RabbitMQ Queue");
})
.WithName("Post Message")
.WithOpenApi();

app.Run();