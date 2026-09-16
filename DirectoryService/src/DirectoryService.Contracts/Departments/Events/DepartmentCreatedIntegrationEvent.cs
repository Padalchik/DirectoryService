namespace DirectoryService.Contracts.Departments.Events;

public sealed record DepartmentCreatedIntegrationEvent(
    Guid EventId,
    Guid DepartmentId,
    string Name,
    DateTime CreatedAtUtc);
