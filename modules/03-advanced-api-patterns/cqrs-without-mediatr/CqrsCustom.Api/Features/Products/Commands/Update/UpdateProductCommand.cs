using CqrsCustom.Api.Behaviors;
using CqrsCustom.Api.Dispatcher;
using CqrsCustom.Api.Persistence;

namespace CqrsCustom.Api.Features.Products.Commands.Update;

public sealed record UpdateProductCommand(Guid Id, string Name, decimal Price)
    : IRequest<bool>, ITransactional;

public sealed class UpdateProductCommandHandler(AppDbContext db)
    : IRequestHandler<UpdateProductCommand, bool>
{
    public async ValueTask<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await db.Products.FindAsync([request.Id], cancellationToken);
        if (product is null) return false;

        product.Name = request.Name;
        product.Price = request.Price;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
