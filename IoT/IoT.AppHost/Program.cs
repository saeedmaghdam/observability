var builder = DistributedApplication.CreateBuilder(args);

var bus = builder.AddRabbitMQ("bus", password: builder.AddParameter("RMQ-PASSWORD")).WithManagementPlugin();
var db = builder.AddRedis("db");

var deviceManagementApi = builder.AddProject<Projects.IoT_DeviceManagementApi>("iot-devicemanagementapi");
var alertManagementApi = builder.AddProject<Projects.IoT_AlertManagementApi>("iot-alertmanagementapi");
var deviceDataCollector = builder.AddProject<Projects.IoT_DeviceDataCollector>("iot-devicedatacollector");
var alertDispatcher = builder.AddProject<Projects.IoT_AlertDispatcher>("iot-alertdispatcher");

deviceManagementApi = deviceManagementApi
    .WithReference(bus)
    .WithReference(db)
    .WithReference(alertManagementApi);

alertManagementApi = alertManagementApi
    .WithReference(bus);

deviceDataCollector = deviceDataCollector
    .WithReference(db)
    .WithReference(deviceManagementApi);

alertDispatcher = alertDispatcher
    .WithReference(bus);

builder.Build().Run();
