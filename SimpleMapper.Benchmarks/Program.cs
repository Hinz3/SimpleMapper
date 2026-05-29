using AutoMapper;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.Logging.Abstractions;
using AutoMapperConfiguration = AutoMapper.MapperConfiguration;
using SimpleMapperConfiguration = SimpleMapper.Configuration.MapperConfiguration;

BenchmarkRunner.Run<CollectionMappingBenchmarks>();

[MemoryDiagnoser]
[MarkdownExporterAttribute.GitHub]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class CollectionMappingBenchmarks
{
    private IMapper _autoMapper = null!;
    private SimpleMapper.Interfaces.IMapper _simpleMapper = null!;
    private List<SourceModel> _sourceItems = null!;

    [Params(1_000, 10_000, 50_000)]
    public int ItemCount { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _sourceItems = CreateSourceItems(ItemCount);
        _autoMapper = CreateAutoMapper();
        _simpleMapper = CreateSimpleMapper();
        ValidateEquivalentResults();
    }

    [Benchmark(Baseline = true)]
    public List<DestinationModel> AutoMapper_Collections()
    {
        return _autoMapper.Map<List<DestinationModel>>(_sourceItems);
    }

    [Benchmark]
    public List<DestinationModel> SimpleMapper_Collections()
    {
        return _simpleMapper.Map<SourceModel, DestinationModel>(_sourceItems);
    }

    private void ValidateEquivalentResults()
    {
        var autoMapperResult = AutoMapper_Collections();
        var simpleMapperResult = SimpleMapper_Collections();

        if (autoMapperResult.Count != simpleMapperResult.Count)
        {
            throw new InvalidOperationException("Benchmark setup produced different result counts.");
        }

        if (autoMapperResult.Count == 0)
        {
            return;
        }

        var firstAutoMapperResult = autoMapperResult[0];
        var firstSimpleMapperResult = simpleMapperResult[0];

        if (firstAutoMapperResult.DisplayName != firstSimpleMapperResult.DisplayName ||
            firstAutoMapperResult.Age != firstSimpleMapperResult.Age ||
            firstAutoMapperResult.Score != firstSimpleMapperResult.Score ||
            firstAutoMapperResult.City != firstSimpleMapperResult.City ||
            firstAutoMapperResult.IsActive != firstSimpleMapperResult.IsActive)
        {
            throw new InvalidOperationException("Benchmark setup produced different mapped values.");
        }
    }

    private static IMapper CreateAutoMapper()
    {
        var configuration = new AutoMapperConfiguration(
            config =>
            {
                config.CreateMap<SourceModel, DestinationModel>()
                    .ForMember(destination => destination.DisplayName, options => options.MapFrom(source => source.Name));
            },
            NullLoggerFactory.Instance);

        configuration.AssertConfigurationIsValid();
        return configuration.CreateMapper();
    }

    private static SimpleMapper.Interfaces.IMapper CreateSimpleMapper()
    {
        var configuration = new SimpleMapperConfiguration();
        configuration.CreateMap<SourceModel, DestinationModel>()
            .Map(destination => destination.DisplayName, source => source.Name)
            .Convention(destination => destination.Age, source => source.Age)
            .Convention(destination => destination.Score, source => source.Score)
            .Convention(destination => destination.City, source => source.City)
            .Convention(destination => destination.IsActive, source => source.IsActive);

        return configuration.BuildMapper();
    }

    private static List<SourceModel> CreateSourceItems(int count)
    {
        var items = new List<SourceModel>(count);

        for (var i = 0; i < count; i++)
        {
            items.Add(new SourceModel
            {
                Name = $"User {i}",
                Age = 18 + (i % 53),
                Score = i * 1.25m,
                City = $"City-{i % 100}",
                IsActive = i % 2 == 0
            });
        }

        return items;
    }
}

public sealed class SourceModel
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public decimal Score { get; set; }
    public string City { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public sealed class DestinationModel
{
    public string DisplayName { get; set; } = string.Empty;
    public int Age { get; set; }
    public decimal Score { get; set; }
    public string City { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
