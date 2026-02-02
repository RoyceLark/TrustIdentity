```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.6584/25H2/2025Update/HudsonValley2)
12th Gen Intel Core i7-12800H 2.40GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 10.0.102
  [Host]     : .NET 10.0.2 (10.0.2, 10.0.225.61305), X64 RyuJIT x86-64-v3
  Job-YFEFPZ : .NET 10.0.2 (10.0.2, 10.0.225.61305), X64 RyuJIT x86-64-v3

IterationCount=10  WarmupCount=3  

```
| Method            | Mean       | Error    | StdDev   | Gen0   | Allocated |
|------------------ |-----------:|---------:|---------:|-------:|----------:|
| SignJwtRsa256     | 1,023.1 μs | 42.33 μs | 22.14 μs | 1.9531 |  27.79 KB |
| SignJwtES256      |   312.3 μs | 15.69 μs | 10.38 μs | 1.9531 |  24.52 KB |
| ValidateJwtRsa256 |   984.6 μs | 45.92 μs | 30.38 μs | 1.9531 |  38.88 KB |
