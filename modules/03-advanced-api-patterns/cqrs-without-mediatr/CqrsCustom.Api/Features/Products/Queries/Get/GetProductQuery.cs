using CqrsCustom.Api.Behaviors;
using CqrsCustom.Api.Dispatcher;
using CqrsCustom.Api.Persistence;

namespace CqrsCustom.Api.Features.Products.Queries.Get;

public sealed record GetProductQuery(Guid Id) : IRequest<ProductDto?>, ICacheable
{
    public string CacheKey => $"product:{Id}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
}

public sealed record ProductDto(Guid Id, string Name, decimal Price, DateTime CreatedAt);

public sealed class GetProductQueryHandler(AppDbContext db)
    : IRequestHandler<GetProductQuery, ProductDto?>
{
    public async ValueTask<ProductDto?> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var product = await db.Products.FindAsync([request.Id], cancellationToken);
        return product is null
            ? null
            : new ProductDto(product.Id, product.Name, product.Price, product.CreatedAt);
    }
}
