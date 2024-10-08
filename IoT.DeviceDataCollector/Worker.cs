using Elastic.Apm.Api;
using IoT.ServiceDefaults;
using System.Diagnostics;
using System.Net.Http.Json;

namespace IoT.DeviceDataCollector;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IHttpClientFactory _clientFactory;
    private readonly ITracer _tracer;

    private readonly ActivitySource ActivitySource = new("IoT.DeviceDataCollector");

    public Worker(ILogger<Worker> logger, IHttpClientFactory clientFactory, ITracer tracer)
    {
        _logger = logger;
        _clientFactory = clientFactory;
        _tracer = tracer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await DelayRandomly(500, 5000, stoppingToken);
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            var devices = await GetDevicesAsync();
            using var activity = ActivitySource.StartActivity("CollectData", ActivityKind.Server);
            activity!.SetTag("span.type", "TEST3");

            //var transaction = _tracer.CurrentTransaction.StartSpan("CollectData", "Worker");
            //var span = transaction.StartSpan("GetDevices", "DeviceDataCollector");
            foreach (var deviceId in devices)
            {
                //var span2 = transaction.StartSpan("SendData", "DeviceDataCollector");
                using var sendDataActivity = ActivitySource.StartActivity("SendData");
                sendDataActivity?.SetTag("DeviceId", deviceId);
                sendDataActivity?.SetTag("Worker", "DeviceDataCollector");

                await SendData(deviceId);
                await DelayRandomly(500, 2500, stoppingToken);

                //sendDataActivity.Stop();
                //span2.End();
            }

            //span.End();
            //transaction.End();
        }
    }

    private async Task SendData(string deviceId)
    {
        var data = new
        {
            Value = new Random().Next(10, 99)
        };

        using var client = _clientFactory.CreateClient(IoTServices.DeviceManagementApi.ToString());
        var response = await client.PostAsJsonAsync($"/{deviceId}/data", data);
        if (response.IsSuccessStatusCode)
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Data sent to {deviceId}", deviceId);
        }
        else
        {
            if (_logger.IsEnabled(LogLevel.Error))
                _logger.LogError("Failed to send data to {deviceId}", deviceId);
        }
    }

    private async Task<IEnumerable<string>> GetDevicesAsync()
    {
        var devices = new List<string>();
        using var client = _clientFactory.CreateClient(IoTServices.DeviceManagementApi.ToString());
        var response = await client.GetAsync("/all");
        if (response.IsSuccessStatusCode)
        {
            devices = await response.Content.ReadFromJsonAsync<List<string>>();
        }
        else
        {
            if (_logger.IsEnabled(LogLevel.Error))
                _logger.LogError("Failed to get devices");
        }

        return devices;
    }

    private async Task DelayRandomly(int min, int max, CancellationToken stoppingToken)
    {
        var delay = new Random().Next(min, max);
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("Delaying for {delay} ms", delay);

        await Task.Delay(delay, stoppingToken);
    }
}
