namespace CqrsCustom.Api.Behaviors;

/// <summary>
/// Opt-in marker. Queries that implement this get a HybridCache lookup wrapped around them.
/// </summary>
public interface ICacheable
{
    string CacheKey { get; }
    TimeSpan? Expiration => null;
}
