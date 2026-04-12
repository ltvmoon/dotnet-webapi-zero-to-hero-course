using CqrsCustom.Api.Dispatcher;
using CqrsCustom.Api.Features.Products.Queries.Get;
using CqrsCustom.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CqrsCustom.Api.Features.Products.Queries.List;

public sealed record ListProductsQuery : IRequest<IReadOnlyList<ProductDto>>;

public sealed class ListProductsQueryHandler(AppDbContext db)
    : IRequestHandler<ListProductsQuery, IReadOnlyList<ProductDto>>
{
    public async ValueTask<IReadOnlyList<ProductDto>> Handle(
        ListProductsQuery request, CancellationToken cancellationToken)
    {
        return await db.Products
            .AsNoTracking()
            .Select(p => new ProductDto(p.Id, p.Name, p.Price, p.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
