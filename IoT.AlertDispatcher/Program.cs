using Elastic.Apm.DiagnosticSource;
using IoT.AlertDispatcher;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.AddRabbitMQClient("bus");

builder.Services.AddElasticApm(new HttpDiagnosticsSubscriber());
builder.Services.AddHttpClient();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
