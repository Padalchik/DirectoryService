using DirectoryService.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.Cache;

public class DepartmentsCachePolicy : IDepartmentsCachePolicy
{
    public TimeSpan Ttl { get; }

    public string Prefix { get; }

    public DepartmentsCachePolicy(IOptions<DepartmentsCacheOptions> options)
    {
        Prefix = options.Value.Prefix;
        Ttl = TimeSpan.FromMinutes(options.Value.TtlMinutes);
    }
}