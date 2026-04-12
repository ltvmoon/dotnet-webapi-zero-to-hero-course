namespace CqrsCustom.Api.Dispatcher;

/// <summary>
/// Handles a request and returns a response. ValueTask is used over Task to avoid
/// allocations when the handler completes synchronously.
/// </summary>
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    ValueTask<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Handles a request that has no response payload.
/// </summary>
public interface IRequestHandler<in TRequest> : IRequestHandler<TRequest, Unit>
    where TRequest : IRequest<Unit>;
