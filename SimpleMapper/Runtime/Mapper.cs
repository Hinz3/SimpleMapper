using SimpleMapper.Configuration;
using SimpleMapper.Interfaces;

namespace SimpleMapper.Runtime;

internal sealed class Mapper(MapperConfiguration configuration) : IMapper
{

    /// <summary>
    /// Maps a single source instance to a new destination instance.
    /// </summary>
    /// <typeparam name="TSource">The source type to read values from.</typeparam>
    /// <typeparam name="TDestination">The destination type to create and populate.</typeparam>
    /// <param name="source">The source instance to map.</param>
    /// <returns>A new destination instance populated by applying convention mappings first and explicit mappings second.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no mapping is registered for the source and destination types.</exception>
    public TDestination Map<TSource, TDestination>(TSource source)
        where TSource : class
        where TDestination : class, new()
    {
        ArgumentNullException.ThrowIfNull(source);

        var map = GetMapOrThrow<TSource, TDestination>();
        var destination = new TDestination();
        map.GetCompiledMap()(source, destination);
        return destination;
    }

    /// <summary>
    /// Maps each source instance in a sequence to a new destination instance.
    /// </summary>
    /// <typeparam name="TSource">The source type to read values from.</typeparam>
    /// <typeparam name="TDestination">The destination type to create and populate.</typeparam>
    /// <param name="source">The source sequence to map.</param>
    /// <returns>A list containing mapped destination instances in the same order as the source sequence.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no mapping is registered for the source and destination types.</exception>
    public List<TDestination> Map<TSource, TDestination>(IEnumerable<TSource> source)
        where TSource : class
        where TDestination : class, new()
    {
        ArgumentNullException.ThrowIfNull(source);

        var map = GetMapOrThrow<TSource, TDestination>();
        var apply = map.GetCompiledMap();
        var list = source switch
        {
            ICollection<TSource> collection => new List<TDestination>(collection.Count),
            IReadOnlyCollection<TSource> collection => new List<TDestination>(collection.Count),
            _ => new List<TDestination>()
        };

        foreach (var item in source)
        {
            ArgumentNullException.ThrowIfNull(item);

            var destination = new TDestination();
            apply(item, destination);
            list.Add(destination);
        }

        return list;
    }

    private TypeMapDefinition<TSource, TDestination> GetMapOrThrow<TSource, TDestination>()
        where TSource : class
        where TDestination : class, new()
    {
        if (configuration.TryGet<TSource, TDestination>(out var map))
        {
            return map;
        }

        throw new InvalidOperationException($"No mapping is configured for '{typeof(TSource).Name}' -> '{typeof(TDestination).Name}'.");
    }
}
