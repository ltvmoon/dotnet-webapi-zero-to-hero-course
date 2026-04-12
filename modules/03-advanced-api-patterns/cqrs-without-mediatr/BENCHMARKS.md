# Dispatcher Benchmarks

Real BenchmarkDotNet results for the dispatcher comparison in the article.

## Environment

- BenchmarkDotNet v0.15.4
- Windows 11 (10.0.26200.8117)
- Intel Core Ultra 9 275HX 2.70 GHz, 24 logical / 24 physical cores
- .NET SDK 10.0.201
- Runtime: .NET 10.0.5, X64 RyuJIT x86-64-v3
- GC: Concurrent Server

## Workload

A trivial `Ping(string Message) -> string` request. The handler just returns `request.Message`. Goal: isolate dispatcher overhead.

## Results

| Dispatcher                    | Mean       | Ratio     | Gen0   | Allocated |
|-------------------------------|-----------:|----------:|-------:|----------:|
| Raw method call (baseline)    |   0.054 ns |      1.00 |      - |       0 B |
| FrozenDictionary dispatcher   |  11.476 ns |    214.51 | 0.0004 |      24 B |
| MediatR 12.4.1                |  50.411 ns |    942.27 | 0.0035 |     200 B |
| Reflection dispatcher         | 148.535 ns |  2,776.37 | 0.0064 |     288 B |

## Key takeaways

- FrozenDictionary is 4.4x faster than MediatR 12.4.1 (11.5 ns vs 50.4 ns)
- FrozenDictionary allocates 8.3x less than MediatR (24 B vs 200 B)
- FrozenDictionary is 12.9x faster than the naive reflection dispatcher (11.5 ns vs 148.5 ns)
- The naive reflection dispatcher is 2.9x slower than MediatR - people building their own to escape MediatR end up slower than MediatR if they use reflection

## How to reproduce

```bash
cd CqrsCustom.Benchmarks
dotnet run -c Release
```
