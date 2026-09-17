using System.Text.Json;
using DirectoryService.Contracts.Departments.Events;
using DirectoryService.Contracts.Messaging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DirectoryService.Consumer;

public sealed class DepartmentCreatedConsumer : BackgroundService
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<DepartmentCreatedConsumer> _logger;
    private readonly IHostApplicationLifetime _applicationLifetime;

    public DepartmentCreatedConsumer(
        IOptions<RabbitMqOptions> options,
        ILogger<DepartmentCreatedConsumer> logger,
        IHostApplicationLifetime applicationLifetime)
    {
        _options = options.Value;
        _logger = logger;
        _applicationLifetime = applicationLifetime;
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
        var channelOptions = new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true);
        await using IChannel channel = await connection.CreateChannelAsync(
            channelOptions,
            cancellationToken: stoppingToken);

        await DeclareTopologyAsync(channel, stoppingToken);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (_, eventArgs) =>
            HandleMessageAsync(channel, eventArgs, stoppingToken);

        await channel.BasicConsumeAsync(
            DirectoryEventsTopology.DepartmentCreatedQueue,
            autoAck: false,
            consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation(
            "Waiting for messages from queue {QueueName} bound to {ExchangeName} with {RoutingKey}",
            DirectoryEventsTopology.DepartmentCreatedQueue,
            DirectoryEventsTopology.ExchangeName,
            DirectoryEventsTopology.DepartmentCreatedRoutingKey);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task DeclareTopologyAsync(
        IChannel channel,
        CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(
            DirectoryEventsTopology.ExchangeName,
            ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            DirectoryEventsTopology.DeadLetterExchangeName,
            ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            DirectoryEventsTopology.DepartmentCreatedQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            DirectoryEventsTopology.DepartmentCreatedQueue,
            DirectoryEventsTopology.ExchangeName,
            DirectoryEventsTopology.DepartmentCreatedRoutingKey,
            cancellationToken: cancellationToken);

        var retryQueueArguments = new Dictionary<string, object?>
        {
            ["x-message-ttl"] = DirectoryEventsTopology.DepartmentCreatedRetryDelayMilliseconds,
            ["x-dead-letter-exchange"] = DirectoryEventsTopology.ExchangeName,
            ["x-dead-letter-routing-key"] = DirectoryEventsTopology.DepartmentCreatedRoutingKey,
        };

        await channel.QueueDeclareAsync(
            DirectoryEventsTopology.DepartmentCreatedRetryQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: retryQueueArguments,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            DirectoryEventsTopology.DepartmentCreatedDeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            DirectoryEventsTopology.DepartmentCreatedDeadLetterQueue,
            DirectoryEventsTopology.DeadLetterExchangeName,
            DirectoryEventsTopology.DepartmentCreatedDeadLetterRoutingKey,
            cancellationToken: cancellationToken);
    }

    private async Task HandleMessageAsync(
        IChannel channel,
        BasicDeliverEventArgs eventArgs,
        CancellationToken cancellationToken)
    {
        try
        {
            DepartmentCreatedIntegrationEvent integrationEvent;
            try
            {
                integrationEvent = JsonSerializer.Deserialize<DepartmentCreatedIntegrationEvent>(
                    eventArgs.Body.Span)
                    ?? throw new JsonException("Message body is empty.");
            }
            catch (JsonException exception)
            {
                _logger.LogWarning(
                    exception,
                    "Permanent failure for malformed department event: MessageId={MessageId}, Result={Result}",
                    eventArgs.BasicProperties.MessageId,
                    "permanent failure");

                await PublishThenAckAsync(
                    channel,
                    eventArgs,
                    DirectoryEventsTopology.DeadLetterExchangeName,
                    DirectoryEventsTopology.DepartmentCreatedDeadLetterRoutingKey,
                    GetRetryCount(eventArgs.BasicProperties.Headers),
                    cancellationToken);

                _logger.LogWarning(
                    "Malformed department event moved to DLQ: MessageId={MessageId}, Result={Result}",
                    eventArgs.BasicProperties.MessageId,
                    "moved to DLQ");
                return;
            }

            int retryCount = GetRetryCount(eventArgs.BasicProperties.Headers);
            int attempt = retryCount + 1;

            _logger.LogInformation(
                "Processing department event: EventId={EventId}, DepartmentId={DepartmentId}, Name={Name}, Attempt={Attempt}",
                integrationEvent.EventId,
                integrationEvent.DepartmentId,
                integrationEvent.Name,
                attempt);

            if (integrationEvent.Name.StartsWith("INVALID-", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "Permanent failure: EventId={EventId}, DepartmentId={DepartmentId}, Name={Name}, Attempt={Attempt}, Result={Result}",
                    integrationEvent.EventId,
                    integrationEvent.DepartmentId,
                    integrationEvent.Name,
                    attempt,
                    "permanent failure");

                await PublishThenAckAsync(
                    channel,
                    eventArgs,
                    DirectoryEventsTopology.DeadLetterExchangeName,
                    DirectoryEventsTopology.DepartmentCreatedDeadLetterRoutingKey,
                    retryCount,
                    cancellationToken);

                _logger.LogWarning(
                    "Moved to DLQ: EventId={EventId}, DepartmentId={DepartmentId}, Name={Name}, Attempt={Attempt}, Result={Result}",
                    integrationEvent.EventId,
                    integrationEvent.DepartmentId,
                    integrationEvent.Name,
                    attempt,
                    "moved to DLQ");
                return;
            }

            if (integrationEvent.Name.StartsWith("TRANSIENT-", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "Transient failure: EventId={EventId}, DepartmentId={DepartmentId}, Name={Name}, Attempt={Attempt}, Result={Result}",
                    integrationEvent.EventId,
                    integrationEvent.DepartmentId,
                    integrationEvent.Name,
                    attempt,
                    "transient failure");

                if (retryCount < DirectoryEventsTopology.MaxRetryCount)
                {
                    int nextRetryCount = retryCount + 1;
                    await PublishThenAckAsync(
                        channel,
                        eventArgs,
                        exchange: string.Empty,
                        routingKey: DirectoryEventsTopology.DepartmentCreatedRetryQueue,
                        retryCount: nextRetryCount,
                        cancellationToken);

                    _logger.LogWarning(
                        "Scheduled for retry: EventId={EventId}, DepartmentId={DepartmentId}, Name={Name}, Attempt={Attempt}, RetryCount={RetryCount}, Result={Result}",
                        integrationEvent.EventId,
                        integrationEvent.DepartmentId,
                        integrationEvent.Name,
                        attempt,
                        nextRetryCount,
                        "scheduled for retry");
                    return;
                }

                _logger.LogWarning(
                    "Retry limit exceeded: EventId={EventId}, DepartmentId={DepartmentId}, Name={Name}, Attempt={Attempt}, RetryCount={RetryCount}, Result={Result}",
                    integrationEvent.EventId,
                    integrationEvent.DepartmentId,
                    integrationEvent.Name,
                    attempt,
                    retryCount,
                    "retry limit exceeded");

                await PublishThenAckAsync(
                    channel,
                    eventArgs,
                    DirectoryEventsTopology.DeadLetterExchangeName,
                    DirectoryEventsTopology.DepartmentCreatedDeadLetterRoutingKey,
                    retryCount,
                    cancellationToken);

                _logger.LogWarning(
                    "Moved to DLQ: EventId={EventId}, DepartmentId={DepartmentId}, Name={Name}, Attempt={Attempt}, RetryCount={RetryCount}, Result={Result}",
                    integrationEvent.EventId,
                    integrationEvent.DepartmentId,
                    integrationEvent.Name,
                    attempt,
                    retryCount,
                    "moved to DLQ");
                return;
            }

            _logger.LogInformation(
                "Successfully processed department event: EventId={EventId}, DepartmentId={DepartmentId}, Name={Name}, Attempt={Attempt}, Result={Result}",
                integrationEvent.EventId,
                integrationEvent.DepartmentId,
                integrationEvent.Name,
                attempt,
                "success");

            await channel.BasicAckAsync(
                eventArgs.DeliveryTag,
                multiple: false,
                cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Connection disposal requeues an unacknowledged delivery during shutdown.
        }
        catch (Exception exception)
        {
            // Do not ACK when the retry/DLQ publish was not confirmed. Stopping the host closes
            // the connection, so RabbitMQ safely requeues the still-unacknowledged delivery.
            _logger.LogCritical(
                exception,
                "Department event handling failed before ACK; the host will stop and RabbitMQ will requeue the unacknowledged delivery");
            _applicationLifetime.StopApplication();
        }
    }

    private async Task PublishThenAckAsync(
        IChannel channel,
        BasicDeliverEventArgs eventArgs,
        string exchange,
        string routingKey,
        int retryCount,
        CancellationToken cancellationToken)
    {
        var headers = eventArgs.BasicProperties.Headers is null
            ? new Dictionary<string, object?>()
            : new Dictionary<string, object?>(eventArgs.BasicProperties.Headers);
        headers[DirectoryEventsTopology.RetryCountHeader] = retryCount;

        var properties = new BasicProperties
        {
            ContentType = eventArgs.BasicProperties.ContentType ?? "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            MessageId = eventArgs.BasicProperties.MessageId,
            Type = eventArgs.BasicProperties.Type,
            CorrelationId = eventArgs.BasicProperties.CorrelationId,
            Headers = headers,
        };

        // With publisher confirmation tracking enabled, awaiting BasicPublishAsync waits for
        // the broker ACK and throws on publisher NACK or mandatory unroutable return.
        await channel.BasicPublishAsync(
            exchange,
            routingKey,
            mandatory: true,
            properties,
            eventArgs.Body,
            cancellationToken);

        await channel.BasicAckAsync(
            eventArgs.DeliveryTag,
            multiple: false,
            cancellationToken: cancellationToken);
    }

    private int GetRetryCount(IDictionary<string, object?>? headers)
    {
        if (headers is null ||
            !headers.TryGetValue(DirectoryEventsTopology.RetryCountHeader, out object? value))
        {
            return 0;
        }

        return value switch
        {
            byte number => number,
            sbyte number => number,
            short number => number,
            ushort number => number,
            int number => number,
            uint number when number <= int.MaxValue => (int)number,
            long number when number is >= 0 and <= int.MaxValue => (int)number,
            ulong number when number <= int.MaxValue => (int)number,
            _ => throw new InvalidDataException(
                $"Header '{DirectoryEventsTopology.RetryCountHeader}' has unsupported value '{value}'."),
        };
    }
}
