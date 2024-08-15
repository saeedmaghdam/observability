using IoT.AlertDispatcher;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.AddRabbitMQClient("bus");

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
