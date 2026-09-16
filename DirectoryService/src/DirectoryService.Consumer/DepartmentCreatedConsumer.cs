using System.Text.Json;
using DirectoryService.Contracts.Departments.Events;
using DirectoryService.Contracts.Messaging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DirectoryService.Consumer;

public sealed class DepartmentCreatedConsumer : BackgroundService
{
    public const string QUEUE_NAME = "directory.department-created.consumer";

    private readonly RabbitMqOptions _options;
    private readonly ILogger<DepartmentCreatedConsumer> _logger;

    public DepartmentCreatedConsumer(
        IOptions<RabbitMqOptions> options,
        ILogger<DepartmentCreatedConsumer> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost,
            AutomaticRecoveryEnabled = true,
        };

        await using IConnection connection = await factory.CreateConnectionAsync(
            "directory-service-department-created-consumer",
            stoppingToken);
        await using IChannel channel = await connection.CreateChannelAsync(
            cancellationToken: stoppingToken);

        await channel.ExchangeDeclareAsync(
            DirectoryEventsTopology.EXCHANGE_NAME,
            ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            QUEUE_NAME,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            QUEUE_NAME,
            DirectoryEventsTopology.EXCHANGE_NAME,
            DirectoryEventsTopology.DEPARTMENT_CREATED_ROUTING_KEY,
            cancellationToken: stoppingToken);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            try
            {
                var integrationEvent = JsonSerializer.Deserialize<DepartmentCreatedIntegrationEvent>(
                    eventArgs.Body.Span)
                    ?? throw new JsonException("Message body is empty.");

                _logger.LogInformation(
                    "Department created event received: EventId={EventId}, DepartmentId={DepartmentId}, Name={Name}",
                    integrationEvent.EventId,
                    integrationEvent.DepartmentId,
                    integrationEvent.Name);

                await channel.BasicAckAsync(
                    deliveryTag: eventArgs.DeliveryTag,
                    multiple: false,
                    cancellationToken: stoppingToken);

                await channel.BasicAckAsync(
                    eventArgs.DeliveryTag,
                    multiple: false,
                    cancellationToken: stoppingToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogError(exception, "Department created event could not be processed");

                await channel.BasicNackAsync(
                    eventArgs.DeliveryTag,
                    multiple: false,
                    requeue: false,
                    cancellationToken: stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(
            QUEUE_NAME,
            autoAck: false,
            consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation(
            "Waiting for messages from queue {QueueName} bound to {ExchangeName} with {RoutingKey}",
            QUEUE_NAME,
            DirectoryEventsTopology.EXCHANGE_NAME,
            DirectoryEventsTopology.DEPARTMENT_CREATED_ROUTING_KEY);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
