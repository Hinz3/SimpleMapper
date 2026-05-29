using SimpleMapper.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleMapper.Interfaces;

namespace SimpleMapper.Tests;

public class MapperTests
{
    [Fact]
    public void Map_UsesExplicitMapping_ForDifferentPropertyNames()
    {
        var mapper = CreateMapper(config =>
        {
            config.CreateMap<SourceModel, DestinationModel>()
                .Map(destination => destination.FullName, source => source.Name);
        });

        var result = mapper.Map<SourceModel, DestinationModel>(new SourceModel { Name = "Ada Lovelace" });
        Assert.Equal("Ada Lovelace", result?.FullName);
    }

    [Fact]
    public void Map_UsesConventionFallback_ForSameNameProperties()
    {
        var mapper = CreateMapper(config =>
        {
            config.CreateMap<SourceModel, DestinationModel>()
                .Convention(destination => destination.Age, source => source.Age)
                .Convention(destination => destination.Summary, source => source.Summary);
        });

        var result = mapper.Map<SourceModel, DestinationModel>(new SourceModel { Age = 36, Summary = "Scientist" });
        Assert.Equal(36, result?.Age);
        Assert.Equal("Scientist", result?.Summary);
    }

    [Fact]
    public void Map_ExplicitMapping_OverridesConventionMapping()
    {
        var mapper = CreateMapper(config =>
        {
            config.CreateMap<SourceModel, DestinationModel>()
                .Convention(destination => destination.Summary, source => source.Summary)
                .Map(destination => destination.Summary, source => source.OverrideSummary);
        });

        var result = mapper.Map<SourceModel, DestinationModel>(new SourceModel
        {
            Summary = "Convention value",
            OverrideSummary = "Explicit value"
        });

        Assert.Equal("Explicit value", result?.Summary);
    }

    [Fact]
    public void Map_Ignore_SkipsConfiguredDestinationProperty()
    {
        var mapper = CreateMapper(config =>
        {
            config.CreateMap<SourceModel, DestinationModel>()
                .Convention(destination => destination.Age, source => source.Age)
                .Ignore(destination => destination.Age);
        });

        var result = mapper.Map<SourceModel, DestinationModel>(new SourceModel { Age = 99 });
        Assert.Equal(0, result?.Age);
    }

    [Fact]
    public async Task Map_ThrowsWhenNoMapIsRegistered()
    {
        var mapper = CreateMapper(_ => { });
        await Assert.ThrowsAsync<InvalidOperationException>(async () => mapper.Map<SourceModel, DestinationModel>(new SourceModel()));
    }

    [Fact]
    public async Task Map_ThrowsWhenSourceIsNull()
    {
        var mapper = CreateMapper(config => config.CreateMap<SourceModel, DestinationModel>());
        await Assert.ThrowsAsync<ArgumentNullException>(async () => mapper.Map<SourceModel, DestinationModel>((SourceModel)null!));
    }

    [Fact]
    public void Map_AppliesReverse_WhenReverseableConfigured()
    {
        var mapper = CreateMapper(config =>
        {
            config.CreateMap<SourceModel, DestinationModel>()
                .Map(destination => destination.FullName, source => source.Name)
                .Convention(destination => destination.Age, source => source.Age)
                .Reverseable();
        });

        var result = mapper.Map<DestinationModel, SourceModel>(new DestinationModel { FullName = "Reverse Name", Age = 42 });
        Assert.Equal("Reverse Name", result?.Name);
        Assert.Equal(42, result?.Age);
    }

    [Fact]
    public void Map_Reverse_SkipsNonReversibleExplicitExpressions()
    {
        var mapper = CreateMapper(config =>
        {
            config.CreateMap<SourceModel, DestinationModel>()
                .Map(destination => destination.FullName, source => $"{source.Name}-computed")
                .Reverseable();
        });

        var result = mapper.Map<DestinationModel, SourceModel>(new DestinationModel { FullName = "Should not map back" });
        Assert.Null(result?.Name);
    }

    [Fact]
    public void MapCollection_MapsAllElements_WhenMapConfigured()
    {
        var mapper = CreateMapper(config =>
        {
            config.CreateMap<SourceModel, DestinationModel>()
                .Convention(destination => destination.Age, source => source.Age);
        });

        var result = mapper.Map<SourceModel, DestinationModel>([
            new SourceModel { Age = 21 },
            new SourceModel { Age = 34 }
        ]);

        Assert.Equal(2, result.Count);
        Assert.Equal(21, result[0].Age);
        Assert.Equal(34, result[1].Age);
    }

    [Fact]
    public async Task MapCollection_ThrowsWhenSourceCollectionIsNull()
    {
        var mapper = CreateMapper(config => config.CreateMap<SourceModel, DestinationModel>());
        await Assert.ThrowsAsync<ArgumentNullException>(async () => mapper.Map<SourceModel, DestinationModel>((IEnumerable<SourceModel>)null!));
    }

    [Fact]
    public void AddMapper_ExplicitTypedConfiguration_Works()
    {
        var services = new ServiceCollection();
        services.AddMapper(config =>
        {
            config.AddMap<SourceModel, DestinationModel, SourceToDestinationMap>();
        });

        var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();
        var result = mapper.Map<SourceModel, DestinationModel>(new SourceModel { Name = "Grace", Age = 85 });

        Assert.Equal("Grace", result?.FullName);
        Assert.Equal(85, result?.Age);
    }

    [Fact]
    public void AddMapper_PrebuiltConfiguration_Works()
    {
        var configuration = new MapperConfiguration();
        configuration.CreateMap<SourceModel, DestinationModel>()
            .Map(destination => destination.FullName, source => source.Name)
            .Convention(destination => destination.Age, source => source.Age);

        var services = new ServiceCollection();
        services.AddMapper(configuration);

        var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();
        var result = mapper.Map<SourceModel, DestinationModel>(new SourceModel { Name = "Ada", Age = 36 });

        Assert.Equal("Ada", result?.FullName);
        Assert.Equal(36, result?.Age);
    }

    [Fact]
    public void AddMapper_PrebuiltConfiguration_ThrowsWhenServicesIsNull()
    {
        IServiceCollection services = null!;
        var configuration = new MapperConfiguration();

        Assert.Throws<ArgumentNullException>(() => services.AddMapper(configuration));
    }

    [Fact]
    public void AddMapper_PrebuiltConfiguration_ThrowsWhenConfigurationIsNull()
    {
        var services = new ServiceCollection();
        MapperConfiguration configuration = null!;

        Assert.Throws<ArgumentNullException>(() => services.AddMapper(configuration));
    }

    [Fact]
    public void Source_DoesNotUseReflectionApis()
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Simplemapper"));
        var files = Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(file => !file.Contains("\\obj\\") && !file.Contains("\\bin\\"))
            .ToArray();

        var bannedTokens = new[]
        {
            "System.Reflection",
            ".GetProperties(",
            ".GetProperty(",
            "Activator.",
            "Assembly."
        };

        foreach (var file in files)
        {
            var text = File.ReadAllText(file);
            foreach (var token in bannedTokens)
            {
                Assert.DoesNotContain(token, text);
            }
        }
    }

    private static IMapper CreateMapper(Action<MapperConfiguration> configure)
    {
        var configuration = new MapperConfiguration();
        configure(configuration);
        return configuration.BuildMapper();
    }

    public class SourceModel
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public string? Summary { get; set; }
        public string? OverrideSummary { get; set; }
    }

    public class DestinationModel
    {
        public string? FullName { get; set; }
        public int Age { get; set; }
        public string? Summary { get; set; }
    }

    public sealed class SourceToDestinationMap : IMapperConfiguration<SourceModel, DestinationModel>
    {
        public void Configure(TypeMapBuilder<SourceModel, DestinationModel> map)
        {
            map.Map(destination => destination.FullName, source => source.Name)
                .Convention(destination => destination.Age, source => source.Age);
        }
    }
}
