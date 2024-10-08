using Elastic.Apm.DiagnosticSource;
using IoT.DeviceDataCollector;
using IoT.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHttpClient(IoTServices.DeviceManagementApi.ToString(), c => c.BaseAddress = new Uri($"https://{IoTServices.DeviceManagementApi.ToUniqueId()}"));

builder.Services.AddElasticApm(new HttpDiagnosticsSubscriber());
builder.Services.AddHttpClient();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
