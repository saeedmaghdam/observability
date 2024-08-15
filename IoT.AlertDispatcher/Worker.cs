using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;

namespace IoT.AlertDispatcher;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IModel _channel;

    private readonly ActivitySource ActivitySource = new("IoT.AlertDispatcher");

    private const string _queueName = "alerts";

    public Worker(ILogger<Worker> logger, IConnection connection)
    {
        _logger = logger;

        _channel = connection.CreateModel();
        _channel.QueueDeclare(_queueName, exclusive: false, durable: true);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (ch, ea) =>
        {
            var activity = ActivitySource.StartActivity("ProcessAlert");
            activity?.SetTag("Alert", Encoding.UTF8.GetString(ea.Body.ToArray()));

            var body = ea.Body.ToArray();
            _logger.LogInformation("Received: {Alert}", Encoding.UTF8.GetString(body));

            _channel.BasicAck(ea.DeliveryTag, false);
        };

        // this consumer tag identifies the subscription
        // when it has to be cancelled
        _ = _channel.BasicConsume(_queueName, false, consumer);

        return Task.CompletedTask;
    }
}
