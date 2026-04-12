using CqrsCustom.Api.Behaviors;
using CqrsCustom.Api.Dispatcher;
using CqrsCustom.Api.Persistence;

namespace CqrsCustom.Api.Features.Products.Commands.Delete;

public sealed record DeleteProductCommand(Guid Id) : IRequest<bool>, ITransactional;

public sealed class DeleteProductCommandHandler(AppDbContext db)
    : IRequestHandler<DeleteProductCommand, bool>
{
    public async ValueTask<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await db.Products.FindAsync([request.Id], cancellationToken);
        if (product is null) return false;

        db.Products.Remove(product);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
