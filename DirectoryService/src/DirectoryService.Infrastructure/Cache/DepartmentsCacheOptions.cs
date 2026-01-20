namespace DirectoryService.Infrastructure.Cache;

public class DepartmentsCacheOptions
{
    public string Prefix { get; init; } = "departments";

    public int TtlMinutes { get; init; } = 5;
}