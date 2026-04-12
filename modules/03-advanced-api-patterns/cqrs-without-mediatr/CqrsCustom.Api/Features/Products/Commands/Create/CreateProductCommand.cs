using CqrsCustom.Api.Behaviors;
using CqrsCustom.Api.Dispatcher;
using CqrsCustom.Api.Persistence;
using FluentValidation;

namespace CqrsCustom.Api.Features.Products.Commands.Create;

public sealed record CreateProductCommand(string Name, decimal Price)
    : IRequest<Guid>, ITransactional;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}

public sealed class CreateProductCommandHandler(AppDbContext db)
    : IRequestHandler<CreateProductCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Price = request.Price,
            CreatedAt = DateTime.UtcNow
        };

        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);
        return product.Id;
    }
}
