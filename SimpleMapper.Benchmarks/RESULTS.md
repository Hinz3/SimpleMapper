# AutoMapper vs SimpleMapper Benchmark Results

Run date: 2026-05-29

Command:

```powershell
dotnet run -c Release --project SimpleMapper.Benchmarks
```

Environment:

```text
BenchmarkDotNet v0.13.12
Windows 11 (10.0.26200.8457)
Intel Core i5-9600KF CPU 3.70GHz (Coffee Lake), 6 logical / 6 physical cores
.NET SDK 10.0.300
Host runtime: .NET 10.0.8 (10.0.826.23019), X64 RyuJIT AVX2
```

Summary:

- SimpleMapper is now slightly slower than AutoMapper at 1,000 items, but faster at 10,000 and 50,000 items.
- At 10,000 items, SimpleMapper is about 3.34x faster in mean runtime.
- At 50,000 items, SimpleMapper is about 1.07x faster in mean runtime.
- SimpleMapper now allocates less managed memory than AutoMapper across all measured collection sizes.

| Method                   | ItemCount | Mean        | Error      | StdDev     | Median      | Ratio | RatioSD | Gen0     | Gen1     | Gen2     | Allocated  | Alloc Ratio |
|------------------------- |---------- |------------:|-----------:|-----------:|------------:|------:|--------:|---------:|---------:|---------:|-----------:|------------:|
| AutoMapper_Collections   | 1000      |    18.84 us |   0.654 us |   1.927 us |    19.85 us |  1.00 |    0.00 |  15.4114 |   2.5635 |        - |    70.9 KB |        1.00 |
| SimpleMapper_Collections | 1000      |    22.02 us |   0.440 us |   0.524 us |    22.02 us |  1.36 |    0.11 |  13.5803 |   2.2583 |        - |   62.55 KB |        0.88 |
| SimpleMapper_Collections | 10000     |   324.12 us |   6.415 us |   8.342 us |   324.47 us |  0.31 |    0.04 | 110.1074 |  79.1016 |        - |  625.05 KB |        0.78 |
| AutoMapper_Collections   | 10000     | 1,083.00 us |  37.015 us | 109.138 us | 1,098.53 us |  1.00 |    0.00 | 139.6484 |  97.6563 |  39.0625 |  803.19 KB |        1.00 |
| SimpleMapper_Collections | 50000     | 6,522.06 us | 114.226 us | 106.847 us | 6,500.64 us |  1.00 |    0.08 | 531.2500 | 328.1250 |  93.7500 | 3125.09 KB |        0.83 |
| AutoMapper_Collections   | 50000     | 6,950.70 us | 210.214 us | 619.820 us | 6,998.06 us |  1.00 |    0.00 | 625.0000 | 437.5000 | 164.0625 | 3758.78 KB |        1.00 |

Generated BenchmarkDotNet artifacts:

- `BenchmarkDotNet.Artifacts/results/CollectionMappingBenchmarks-report-github.md`
- `BenchmarkDotNet.Artifacts/results/CollectionMappingBenchmarks-report.csv`
- `BenchmarkDotNet.Artifacts/results/CollectionMappingBenchmarks-report.html`
