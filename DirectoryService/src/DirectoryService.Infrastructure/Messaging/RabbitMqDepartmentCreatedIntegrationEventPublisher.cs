using System.Text.Json;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments.Events;
using DirectoryService.Contracts.Messaging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace DirectoryService.Infrastructure.Messaging;

public sealed class RabbitMqDepartmentCreatedIntegrationEventPublisher :
    IDepartmentCreatedIntegrationEventPublisher,
    IAsyncDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly SemaphoreSlim _channelLock = new(1, 1);
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqDepartmentCreatedIntegrationEventPublisher(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
    }

    public async Task PublishAsync(
        DepartmentCreatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        byte[] body = JsonSerializer.SerializeToUtf8Bytes(integrationEvent);

        await _channelLock.WaitAsync(cancellationToken);
        try
        {
            await EnsureConnectedAsync(cancellationToken);

            var properties = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent,
                MessageId = integrationEvent.EventId.ToString(),
                Type = nameof(DepartmentCreatedIntegrationEvent),
            };

            await _channel!.BasicPublishAsync(
                DirectoryEventsTopology.ExchangeName,
                DirectoryEventsTopology.DepartmentCreatedRoutingKey,
                mandatory: false,
                properties,
                body,
                cancellationToken);
        }
        finally
        {
            _channelLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _channelLock.WaitAsync();
        try
        {
            if (_channel is not null)
            {
                await _channel.DisposeAsync();
            }

            if (_connection is not null)
            {
                await _connection.DisposeAsync();
            }
        }
        finally
        {
            _channelLock.Release();
            _channelLock.Dispose();
        }
    }

    private async Task EnsureConnectedAsync(CancellationToken cancellationToken)
    {
        if (_connection is { IsOpen: true } && _channel is { IsOpen: true })
        {
            return;
        }

        if (_channel is not null)
        {
            await _channel.DisposeAsync();
        }

        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost,
            AutomaticRecoveryEnabled = true,
        };

        _connection = await factory.CreateConnectionAsync(
            "directory-service-producer",
            cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.ExchangeDeclareAsync(
            DirectoryEventsTopology.ExchangeName,
            ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);
    }
}
