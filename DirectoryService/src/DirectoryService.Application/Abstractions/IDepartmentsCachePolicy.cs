namespace DirectoryService.Application.Abstractions;

public interface IDepartmentsCachePolicy
{
    TimeSpan Ttl { get; }

    string Prefix { get; }
}