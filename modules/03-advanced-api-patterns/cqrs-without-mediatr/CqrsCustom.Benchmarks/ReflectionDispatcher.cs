using System.Reflection;
using CqrsCustom.Benchmarks.Custom;
using Microsoft.Extensions.DependencyInjection;

namespace CqrsCustom.Benchmarks;

/// <summary>
/// The naive "build your own" dispatcher that every competitor on the SERP ships.
/// MakeGenericType + GetMethod + Invoke per call. This is the slowest possible variant
/// and the one being measured against.
/// </summary>
public sealed class ReflectionDispatcher(IServiceProvider provider)
{
    public async ValueTask<TResponse> Send<TResponse>(
        IRequest<TResponse> request, CancellationToken ct)
    {
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
        var handler = provider.GetRequiredService(handlerType);

        var handleMethod = handlerType.GetMethod("Handle", BindingFlags.Instance | BindingFlags.Public)!;

        var result = handleMethod.Invoke(handler, [request, ct]);
        return await (ValueTask<TResponse>)result!;
    }
}
