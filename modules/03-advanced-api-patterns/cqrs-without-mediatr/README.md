# CQRS Without MediatR - Custom Dispatcher in .NET 10

Companion code for the article [Build Your Own CQRS Dispatcher in .NET 10 (No MediatR)](https://codewithmukesh.com/blog/cqrs-without-mediatr/).

## What's inside

- **CqrsCustom.Api** - ASP.NET Core 10 Web API using a custom `FrozenDictionary`-backed dispatcher with MediatR-12-compatible interface shapes, plus four pipeline behaviors (Logging, FluentValidation, HybridCache, EF Core Transaction).
- **CqrsCustom.Benchmarks** - BenchmarkDotNet console project that compares the custom dispatcher against MediatR 12.4.1, the naive reflection approach, and a raw method call baseline.

## Run the API

```bash
cd CqrsCustom.Api
dotnet run
```

Open `https://localhost:<port>/scalar/v1` for the Scalar API docs.

## Run the benchmarks

```bash
cd CqrsCustom.Benchmarks
dotnet run -c Release
```

See [BENCHMARKS.md](BENCHMARKS.md) for the latest results.

## License

MIT - free to use, modify, and ship in production.
