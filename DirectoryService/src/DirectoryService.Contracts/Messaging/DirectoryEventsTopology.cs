namespace DirectoryService.Contracts.Messaging;

public static class DirectoryEventsTopology
{
    public const string EXCHANGE_NAME = "directory.events";
    public const string DEPARTMENT_CREATED_ROUTING_KEY = "department.created";
}
