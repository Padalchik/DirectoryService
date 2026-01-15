namespace DirectoryService.Application.Abstractions;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken);

    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan ttl,
        CancellationToken cancellationToken);

    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken);
}