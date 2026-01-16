using DirectoryService.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.Cache;

public class DepartmentsCachePolicy : IDepartmentsCachePolicy
{
    public TimeSpan Ttl { get; }

    public string Prefix { get; }

    public DepartmentsCachePolicy(IOptions<DepartmentsCacheOptions> options, string prefix)
    {
        Prefix = prefix;
        Ttl = TimeSpan.FromMinutes(options.Value.TtlMinutes);
    }
}