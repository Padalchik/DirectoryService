namespace DirectoryService.Contracts.Messaging;

public static class DirectoryEventsTopology
{
    public const string ExchangeName = "directory.events";
    public const string DepartmentCreatedRoutingKey = "department.created";

    public const string DepartmentCreatedQueue = "directory.department-created.consumer";
    public const string DepartmentCreatedRetryQueue = "directory.department-created.retry";
    public const int DepartmentCreatedRetryDelayMilliseconds = 10_000;

    public const string DeadLetterExchangeName = "directory.dead-letter";
    public const string DepartmentCreatedDeadLetterRoutingKey = "department.created.dead";
    public const string DepartmentCreatedDeadLetterQueue = "directory.department-created.dlq";

    public const string RetryCountHeader = "x-retry-count";
    public const int MaxRetryCount = 3;
}
