using Microsoft.Extensions.DependencyInjection;
using SimpleMapper.Configuration;
using SimpleMapper.Interfaces;

namespace SimpleMapper;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds a singleton <see cref="IMapper"/> built from a preconfigured <see cref="MapperConfiguration"/>.
    /// </summary>
    /// <param name="services">The service collection to register the mapper with.</param>
    /// <param name="configuration">The mapper configuration containing the mappings to register.</param>
    /// <returns>The original service collection.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services"/> or <paramref name="configuration"/> is <see langword="null"/>.
    /// </exception>
    public static IServiceCollection AddMapper(this IServiceCollection services, MapperConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddSingleton<IMapper>(_ => configuration.BuildMapper());
        return services;
    }

    /// <summary>
    /// Adds a singleton <see cref="IMapper"/> configured with the supplied mapping definitions.
    /// </summary>
    /// <param name="services">The service collection to register the mapper with.</param>
    /// <param name="configure">A callback that defines the mappings to register.</param>
    /// <returns>The original service collection.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services"/> or <paramref name="configure"/> is <see langword="null"/>.
    /// </exception>
    public static IServiceCollection AddMapper(this IServiceCollection services, Action<MapperConfiguration> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var config = new MapperConfiguration();
        configure(config);
        return services.AddMapper(config);
    }
}
