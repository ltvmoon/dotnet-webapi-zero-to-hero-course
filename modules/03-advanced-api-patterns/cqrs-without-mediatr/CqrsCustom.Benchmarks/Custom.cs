namespace CqrsCustom.Benchmarks.Custom;

public interface IRequest<out TResponse>;

public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    ValueTask<TResponse> Handle(TRequest request, CancellationToken ct);
}

public sealed record Ping(string Message) : IRequest<string>;
