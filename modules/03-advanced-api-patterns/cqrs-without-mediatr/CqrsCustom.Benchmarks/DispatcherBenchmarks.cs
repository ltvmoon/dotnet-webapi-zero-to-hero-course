using BenchmarkDotNet.Attributes;
using CqrsCustom.Benchmarks.Custom;
using Microsoft.Extensions.DependencyInjection;

namespace CqrsCustom.Benchmarks;

[MemoryDiagnoser]
public class DispatcherBenchmarks
{
    private PingHandler _rawHandler = null!;
    private ReflectionDispatcher _reflection = null!;
    private FrozenDispatcher _frozen = null!;
    private MediatR.IMediator _mediatr = null!;
    private IServiceProvider _mediatrSp = null!;

    private static readonly Ping CustomPing = new("hello");
    private static readonly MediatRPing MediatRPingValue = new("hello");

    [GlobalSetup]
    public void Setup()
    {
        // Raw - direct handler reference, baseline.
        _rawHandler = new PingHandler();

        // Reflection-based dispatcher (the naive "build your own" most blogs ship).
        var reflectionSp = new ServiceCollection()
            .AddTransient<IRequestHandler<Ping, string>, PingHandler>()
            .BuildServiceProvider();
        _reflection = new ReflectionDispatcher(reflectionSp);

        // FrozenDictionary dispatcher (recommended).
        var frozenSp = new ServiceCollection()
            .AddTransient<IRequestHandler<Ping, string>, PingHandler>()
            .BuildServiceProvider();
        _frozen = new FrozenDispatcher(frozenSp);

        // MediatR 12.4.1 (last MIT version - the control we are replacing).
        var mediatrServices = new ServiceCollection();
        mediatrServices.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(MediatRPingHandler).Assembly));
        _mediatrSp = mediatrServices.BuildServiceProvider();
        _mediatr = _mediatrSp.GetRequiredService<MediatR.IMediator>();
    }

    [Benchmark(Baseline = true, Description = "Raw method call")]
    public ValueTask<string> Raw() => _rawHandler.Handle(CustomPing, default);

    [Benchmark(Description = "Reflection dispatcher")]
    public ValueTask<string> Reflection() => _reflection.Send<string>(CustomPing, default);

    [Benchmark(Description = "FrozenDictionary dispatcher")]
    public ValueTask<string> Frozen() => _frozen.Send<string>(CustomPing, default);

    [Benchmark(Description = "MediatR 12.4.1")]
    public Task<string> MediatR() => _mediatr.Send(MediatRPingValue);
}

public sealed class PingHandler : IRequestHandler<Ping, string>
{
    public ValueTask<string> Handle(Ping request, CancellationToken ct)
        => ValueTask.FromResult(request.Message);
}

public sealed record MediatRPing(string Message) : MediatR.IRequest<string>;

public sealed class MediatRPingHandler : MediatR.IRequestHandler<MediatRPing, string>
{
    public Task<string> Handle(MediatRPing request, CancellationToken cancellationToken)
        => Task.FromResult(request.Message);
}
