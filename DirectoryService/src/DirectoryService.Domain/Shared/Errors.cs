using System.Collections;

namespace DirectoryService.Domain.Shared;

public class Errors : IEnumerable<Error>
{
    private readonly List<Error> _errors;

    public Errors(IEnumerable<Error> errors)
    {
        _errors = errors.ToList();
    }

    public Errors(Error error)
    {
        _errors = new List<Error>() { error };
    }

    public static implicit operator Errors(List<Error> errors) => new(errors);

    public static implicit operator Errors(Error error)
        => new([error]);

    public IEnumerator<Error> GetEnumerator()
    {
        return _errors.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}