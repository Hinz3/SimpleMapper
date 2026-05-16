namespace SimpleMapper.Interfaces;

/// <summary>
/// Maps configured source types to destination types.
/// </summary>
public interface IMapper
{
    /// <summary>
    /// Maps a single source instance to a new destination instance using the registered mapping definition.
    /// </summary>
    /// <typeparam name="TSource">The source type to read values from.</typeparam>
    /// <typeparam name="TDestination">The destination type to create and populate.</typeparam>
    /// <param name="source">The source instance to map.</param>
    /// <returns>A new destination instance populated from the source.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no mapping is registered for the source and destination types.</exception>
    TDestination? Map<TSource, TDestination>(TSource source)
        where TSource : class
        where TDestination : class, new();

    /// <summary>
    /// Maps each source item in a sequence to a new destination instance using the registered mapping definition.
    /// </summary>
    /// <typeparam name="TSource">The source type to read values from.</typeparam>
    /// <typeparam name="TDestination">The destination type to create and populate.</typeparam>
    /// <param name="source">The source sequence to map.</param>
    /// <returns>A list containing one mapped destination instance for each source item.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no mapping is registered for the source and destination types.</exception>
    List<TDestination> Map<TSource, TDestination>(IEnumerable<TSource> source)
        where TSource : class
        where TDestination : class, new();
}
