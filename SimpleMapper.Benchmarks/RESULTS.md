# AutoMapper vs SimpleMapper Collection Benchmark Results

Run date: 2026-05-30

Command:

```powershell
dotnet run -c Release --project SimpleMapper.Benchmarks
```

Scenario coverage:

- Source shapes: `List<T>`, `T[]`, and iterator-backed `IEnumerable<T>`.
- Item counts: `1,000`, `10,000`, and `50,000`.
- Destination shape: `List<DestinationModel>` for both AutoMapper and SimpleMapper.

Environment:

```text
BenchmarkDotNet v0.13.12
Windows 11 (10.0.26200.8457)
Intel Core i5-9600KF CPU 3.70GHz (Coffee Lake), 6 logical / 6 physical cores
.NET SDK 10.0.300
Host runtime: .NET 10.0.8 (10.0.826.23019), X64 RyuJIT AVX2
```

Summary:

- For counted inputs (`List<T>` and `T[]`), SimpleMapper remains materially faster at `10,000` and `50,000` items while allocating about `22%` less memory.
- For iterator-backed `IEnumerable<T>`, both mappers allocate the same amount because neither can pre-size the destination list from a known count.
- At `10,000` iterator items, the two libraries are effectively tied: AutoMapper `1,216.10 us` vs SimpleMapper `1,225.77 us`.
- At `50,000` iterator items, AutoMapper is slightly faster: `7,092.55 us` vs `7,377.08 us`.
- At `1,000` items, AutoMapper is faster across all source shapes, but SimpleMapper still allocates less memory for counted inputs.

| Method                   | ItemCount | SourceShape | Mean        | Error      | StdDev     | Ratio | RatioSD | Gen0     | Gen1     | Gen2     | Allocated  | Alloc Ratio |
|------------------------- |---------- |------------ |------------:|-----------:|-----------:|------:|--------:|---------:|---------:|---------:|-----------:|------------:|
| AutoMapper_Collections   | 1000      | List        |    20.25 us |   0.370 us |   0.309 us |  1.00 |    0.00 |  15.4114 |   2.5635 |        - |    70.9 KB |        1.00 |
| SimpleMapper_Collections | 1000      | List        |    24.17 us |   0.323 us |   0.286 us |  1.19 |    0.02 |  13.5803 |   2.2583 |        - |   62.55 KB |        0.88 |
|                          |           |             |             |            |            |       |         |          |          |          |            |             |
| AutoMapper_Collections   | 1000      | Array       |    19.46 us |   0.275 us |   0.258 us |  1.00 |    0.00 |  15.4114 |   2.5635 |        - |    70.9 KB |        1.00 |
| SimpleMapper_Collections | 1000      | Array       |    24.05 us |   0.110 us |   0.103 us |  1.24 |    0.01 |  13.6108 |   2.2583 |        - |   62.59 KB |        0.88 |
|                          |           |             |             |            |            |       |         |          |          |          |            |             |
| AutoMapper_Collections   | 1000      | Iterator    |    23.98 us |   0.187 us |   0.175 us |  1.00 |    0.00 |  15.4419 |   2.5635 |        - |   70.97 KB |        1.00 |
| SimpleMapper_Collections | 1000      | Iterator    |    29.67 us |   0.206 us |   0.182 us |  1.24 |    0.01 |  15.4419 |   2.5635 |        - |   70.97 KB |        1.00 |
|                          |           |             |             |            |            |       |         |          |          |          |            |             |
| SimpleMapper_Collections | 10000     | List        |   310.10 us |   3.509 us |   3.282 us |  0.26 |    0.01 | 111.8164 |  78.1250 |        - |  625.05 KB |        0.78 |
| AutoMapper_Collections   | 10000     | List        | 1,174.04 us |  23.220 us |  22.805 us |  1.00 |    0.00 | 138.6719 |  97.6563 |  39.0625 |  803.19 KB |        1.00 |
|                          |           |             |             |            |            |       |         |          |          |          |            |             |
| SimpleMapper_Collections | 10000     | Array       |   311.45 us |   4.355 us |   3.861 us |  0.26 |    0.01 | 110.8398 |  79.5898 |        - |  625.09 KB |        0.78 |
| AutoMapper_Collections   | 10000     | Array       | 1,176.50 us |  21.967 us |  24.417 us |  1.00 |    0.00 | 140.6250 |  97.6563 |  39.0625 |  803.19 KB |        1.00 |
|                          |           |             |             |            |            |       |         |          |          |          |            |             |
| AutoMapper_Collections   | 10000     | Iterator    | 1,216.10 us |  22.738 us |  22.332 us |  1.00 |    0.00 | 138.6719 |  95.7031 |  39.0625 |  803.26 KB |        1.00 |
| SimpleMapper_Collections | 10000     | Iterator    | 1,225.77 us |  15.486 us |  14.486 us |  1.01 |    0.02 | 138.6719 |  97.6563 |  39.0625 |  803.26 KB |        1.00 |
|                          |           |             |             |            |            |       |         |          |          |          |            |             |
| SimpleMapper_Collections | 50000     | List        | 6,500.76 us | 128.675 us | 290.442 us |  0.92 |    0.05 | 546.8750 | 382.8125 | 101.5625 | 3125.09 KB |        0.83 |
| AutoMapper_Collections   | 50000     | List        | 7,048.28 us | 134.054 us | 276.844 us |  1.00 |    0.00 | 625.0000 | 429.6875 | 164.0625 | 3758.78 KB |        1.00 |
|                          |           |             |             |            |            |       |         |          |          |          |            |             |
| SimpleMapper_Collections | 50000     | Array       | 6,611.85 us | 132.009 us | 244.688 us |  0.94 |    0.04 | 546.8750 | 382.8125 | 101.5625 | 3125.12 KB |        0.83 |
| AutoMapper_Collections   | 50000     | Array       | 7,048.73 us | 140.083 us | 319.041 us |  1.00 |    0.00 | 625.0000 | 445.3125 | 164.0625 | 3758.78 KB |        1.00 |
|                          |           |             |             |            |            |       |         |          |          |          |            |             |
| AutoMapper_Collections   | 50000     | Iterator    | 7,092.55 us | 140.897 us | 306.299 us |  1.00 |    0.00 | 625.0000 | 460.9375 | 164.0625 | 3758.85 KB |        1.00 |
| SimpleMapper_Collections | 50000     | Iterator    | 7,377.08 us | 145.312 us | 345.350 us |  1.04 |    0.05 | 632.8125 | 460.9375 | 164.0625 | 3758.85 KB |        1.00 |

Generated BenchmarkDotNet artifacts:

- `BenchmarkDotNet.Artifacts/results/CollectionMappingBenchmarks-report-github.md`
- `BenchmarkDotNet.Artifacts/results/CollectionMappingBenchmarks-report.csv`
- `BenchmarkDotNet.Artifacts/results/CollectionMappingBenchmarks-report.html`
