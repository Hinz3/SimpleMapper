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

        if (!configuration.TryGet<TSource, TDestination>(out var map))
        {
            throw new InvalidOperationException($"No mapping is configured for '{typeof(TSource).Name}' -> '{typeof(TDestination).Name}'.");
        }

        var destination = new TDestination();
        map.ApplyConvention(source, destination);
        foreach (var assignment in map.Assignments)
        {
            assignment(source, destination);
        }

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

        var list = new List<TDestination>();
        foreach (var item in source)
        {
            list.Add(Map<TSource, TDestination>(item));
        }

        return list;
    }
}
