using SimpleMapper.Configuration;

namespace SimpleMapper.Interfaces;

/// <summary>
/// Defines a reusable mapping configuration between a source type and a destination type.
/// </summary>
/// <typeparam name="TFrom">The source type to map from.</typeparam>
/// <typeparam name="TTo">The destination type to map to.</typeparam>
public interface IMapperConfiguration<TFrom, TTo>
    where TFrom : class
    where TTo : class
{
    /// <summary>
    /// Configures how values are transferred from the source type to the destination type.
    /// </summary>
    /// <param name="map">The fluent builder used to define the mapping.</param>
    void Configure(TypeMapBuilder<TFrom, TTo> map);
}
