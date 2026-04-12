namespace CqrsCustom.Api.Dispatcher;

/// <summary>
/// Marker for a request that returns a response. MediatR-12 compatible shape so existing
/// handler code can move with a single using-statement swap.
/// </summary>
public interface IRequest<out TResponse>;

/// <summary>
/// Marker for a request that returns no payload. Returns <see cref="Unit"/>.
/// </summary>
public interface IRequest : IRequest<Unit>;

/// <summary>
/// Void return type for commands that have no response payload.
/// </summary>
public readonly struct Unit
{
    public static readonly Unit Value = default;
}
