```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.6584/25H2/2025Update/HudsonValley2)
12th Gen Intel Core i7-12800H 2.40GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 10.0.102
  [Host]     : .NET 10.0.2 (10.0.2, 10.0.225.61305), X64 RyuJIT x86-64-v3
  Job-YFEFPZ : .NET 10.0.2 (10.0.2, 10.0.225.61305), X64 RyuJIT x86-64-v3

IterationCount=10  WarmupCount=3  

```
| Method         | Mean     | Error    | StdDev   | Allocated |
|--------------- |---------:|---------:|---------:|----------:|
| HashPassword   | 60.21 ms | 1.486 ms | 0.983 ms |     561 B |
| VerifyPassword | 59.93 ms | 1.950 ms | 1.161 ms |     729 B |
