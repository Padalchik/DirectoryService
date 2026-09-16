using DirectoryService.Contracts.Departments.Events;

namespace DirectoryService.Application.Abstractions;

public interface IDepartmentCreatedIntegrationEventPublisher
{
    Task PublishAsync(
        DepartmentCreatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken);
}
