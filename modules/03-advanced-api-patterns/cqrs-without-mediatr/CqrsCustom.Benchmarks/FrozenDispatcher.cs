using System.Collections.Frozen;
using CqrsCustom.Benchmarks.Custom;
using Microsoft.Extensions.DependencyInjection;

namespace CqrsCustom.Benchmarks;

/// <summary>
/// The recommended dispatcher. Wrappers built once at construction, frozen, and looked up
/// in O(1). The Handle path is a single FrozenDictionary read plus a reference-type cast
/// plus a strongly-typed virtual call into the wrapper - no per-call reflection.
/// </summary>
public sealed class FrozenDispatcher
{
    private readonly IServiceProvider _provider;
    private readonly FrozenDictionary<Type, RequestHandlerBase> _wrappers;

    public FrozenDispatcher(IServiceProvider provider)
    {
        _provider = provider;
        var dict = new Dictionary<Type, RequestHandlerBase>();

        // For the benchmark we hand-register the single (Ping, string) pair.
        // In the API project this is auto-discovered by AddDispatcher() at startup.
        var pingWrapperType = typeof(RequestHandlerWrapper<,>)
            .MakeGenericType(typeof(Ping), typeof(string));
        dict[typeof(Ping)] = (RequestHandlerBase)Activator.CreateInstance(pingWrapperType)!;

        _wrappers = dict.ToFrozenDictionary();
    }

    public ValueTask<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct)
    {
        var wrapper = _wrappers[request.GetType()];
        return ((RequestHandlerBase<TResponse>)wrapper).Handle(request, _provider, ct);
    }

    internal abstract class RequestHandlerBase;

    internal abstract class RequestHandlerBase<TResponse> : RequestHandlerBase
    {
        public abstract ValueTask<TResponse> Handle(
            IRequest<TResponse> request, IServiceProvider sp, CancellationToken ct);
    }

    internal sealed class RequestHandlerWrapper<TRequest, TResponse> : RequestHandlerBase<TResponse>
        where TRequest : IRequest<TResponse>
    {
        public override ValueTask<TResponse> Handle(
            IRequest<TResponse> request, IServiceProvider sp, CancellationToken ct)
        {
            var handler = sp.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
            return handler.Handle((TRequest)request, ct);
        }
    }
}
