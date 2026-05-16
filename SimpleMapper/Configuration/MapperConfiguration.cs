using SimpleMapper.Runtime;
using SimpleMapper.Interfaces;
using System.Linq.Expressions;

namespace SimpleMapper.Configuration;

/// <summary>
/// Stores map definitions and builds mapper instances from them.
/// </summary>
public sealed class MapperConfiguration
{
    private readonly Dictionary<(Type Source, Type Destination), object> _maps = [];

    /// <summary>
    /// Creates or retrieves the fluent builder for a source and destination type pair.
    /// </summary>
    /// <typeparam name="TSource">The source type to map from.</typeparam>
    /// <typeparam name="TDestination">The destination type to map to.</typeparam>
    /// <returns>A builder that can configure explicit mappings, conventions, ignores, and reverse mapping.</returns>
    public TypeMapBuilder<TSource, TDestination> CreateMap<TSource, TDestination>()
        where TSource : class
        where TDestination : class
    {
        if (!_maps.TryGetValue((typeof(TSource), typeof(TDestination)), out var existing))
        {
            existing = new TypeMapDefinition<TSource, TDestination>();
            _maps[(typeof(TSource), typeof(TDestination))] = existing;
        }

        return new TypeMapBuilder<TSource, TDestination>(this, (TypeMapDefinition<TSource, TDestination>)existing);
    }

    /// <summary>
    /// Adds a reusable mapping configuration by instantiating a configuration type and applying it to the map registry.
    /// </summary>
    /// <typeparam name="TSource">The source type to map from.</typeparam>
    /// <typeparam name="TDestination">The destination type to map to.</typeparam>
    /// <typeparam name="TMapConfig">The configuration type that defines the mapping.</typeparam>
    public void AddMap<TSource, TDestination, TMapConfig>()
        where TSource : class
        where TDestination : class
        where TMapConfig : IMapperConfiguration<TSource, TDestination>, new()
    {
        var builder = CreateMap<TSource, TDestination>();
        var config = new TMapConfig();
        config.Configure(builder);
    }

    internal void EnableReverseFor<TSource, TDestination>(TypeMapDefinition<TSource, TDestination> forward)
        where TSource : class
        where TDestination : class
    {
        var reverseBuilder = CreateMap<TDestination, TSource>();
        foreach (var item in forward.Reversible)
        {
            reverseBuilder.Map(
                BuildMemberExpression<TSource>(item.SourceName),
                BuildMemberExpression<TDestination>(item.DestinationName));
        }

        foreach (var conventionPair in forward.ConventionPairs)
        {
            reverseBuilder.Convention(
                BuildMemberExpression<TSource>(conventionPair.SourceName),
                BuildMemberExpression<TDestination>(conventionPair.DestinationName));
        }
    }

    internal bool TryGet<TSource, TDestination>(out TypeMapDefinition<TSource, TDestination> definition)
        where TSource : class
        where TDestination : class
    {
        if (_maps.TryGetValue((typeof(TSource), typeof(TDestination)), out var existing))
        {
            definition = (TypeMapDefinition<TSource, TDestination>)existing;
            return true;
        }

        definition = null!;
        return false;
    }

    /// <summary>
    /// Builds a mapper instance that executes the currently registered map definitions.
    /// </summary>
    /// <returns>An <see cref="IMapper"/> backed by this configuration.</returns>
    public IMapper BuildMapper()
    {
        return new Mapper(this);
    }

    private static Expression<Func<TModel, object?>> BuildMemberExpression<TModel>(string memberName)
    {
        var param = Expression.Parameter(typeof(TModel), "x");
        var member = Expression.PropertyOrField(param, memberName);
        var boxed = Expression.Convert(member, typeof(object));
        return Expression.Lambda<Func<TModel, object?>>(boxed, param);
    }
}
