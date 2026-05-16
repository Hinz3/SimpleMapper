namespace SimpleMapper.Configuration;

internal sealed class TypeMapDefinition<TSource, TDestination>
    where TSource : class
    where TDestination : class
{
    private readonly List<Action<TSource, TDestination>> _assignments = [];
    private readonly HashSet<string> _ignored = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Func<TSource, object?>> _namedAccessors = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Action<TDestination, object?>> _namedSetters = new(StringComparer.Ordinal);
    private readonly List<(string SourceName, string DestinationName, Action<TSource, TDestination> Assignment)> _reversible = [];
    private readonly List<(string SourceName, string DestinationName)> _conventionPairs = [];

    public IReadOnlyList<Action<TSource, TDestination>> Assignments => _assignments;
    public IReadOnlySet<string> Ignored => _ignored;
    public IReadOnlyList<(string SourceName, string DestinationName, Action<TSource, TDestination> Assignment)> Reversible => _reversible;
    public IReadOnlyList<(string SourceName, string DestinationName)> ConventionPairs => _conventionPairs;

    public void AddConvention(string destinationName, string sourceName, Func<TSource, object?> accessor, Action<TDestination, object?> setter)
    {
        _namedAccessors[sourceName] = accessor;
        _namedSetters[destinationName] = setter;
        _conventionPairs.Add((sourceName, destinationName));
    }
    public void Ignore(string destinationName) => _ignored.Add(destinationName);

    public void AddExplicit(
        string destinationName,
        Func<TSource, object?> accessor,
        Action<TDestination, object?> setter,
        string? sourceName)
    {
        _assignments.Add((src, dst) =>
        {
            if (_ignored.Contains(destinationName))
            {
                return;
            }

            setter(dst, accessor(src));
        });

        if (sourceName is not null)
        {
            _reversible.Add((sourceName, destinationName, (src, dst) => setter(dst, accessor(src))));
        }
    }

    public void ApplyConvention(TSource src, TDestination dst)
    {
        foreach (var setter in _namedSetters)
        {
            if (_ignored.Contains(setter.Key))
            {
                continue;
            }

            if (_namedAccessors.TryGetValue(setter.Key, out var accessor))
            {
                setter.Value(dst, accessor(src));
            }
        }
    }
}
