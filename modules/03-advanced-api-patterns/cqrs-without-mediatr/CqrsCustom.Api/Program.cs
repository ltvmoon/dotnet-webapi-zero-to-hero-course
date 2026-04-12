using System.Reflection;
using CqrsCustom.Api.Behaviors;
using CqrsCustom.Api.Dispatcher;
using CqrsCustom.Api.Features.Products.Commands.Create;
using CqrsCustom.Api.Features.Products.Commands.Delete;
using CqrsCustom.Api.Features.Products.Commands.Update;
using CqrsCustom.Api.Features.Products.Notifications;
using CqrsCustom.Api.Features.Products.Queries.Get;
using CqrsCustom.Api.Features.Products.Queries.List;
using CqrsCustom.Api.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("CqrsCustomDb"));

#pragma warning disable EXTEXP0018 // HybridCache is in preview
builder.Services.AddHybridCache();
#pragma warning restore EXTEXP0018

// Custom dispatcher - replaces MediatR. One line.
builder.Services.AddDispatcher(Assembly.GetExecutingAssembly());

// Pipeline behaviors run in registration order: Logging wraps Validation wraps Caching wraps Transaction wraps the handler.
builder.Services.AddPipelineBehavior(typeof(LoggingBehavior<,>));
builder.Services.AddPipelineBehavior(typeof(ValidationBehavior<,>));
builder.Services.AddPipelineBehavior(typeof(CachingBehavior<,>));
builder.Services.AddPipelineBehavior(typeof(TransactionBehavior<,>));

builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Endpoints - note ISender, not IMediator. Migration from MediatR is a using-statement swap.
app.MapGet("/products/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
{
    var product = await sender.Send(new GetProductQuery(id), ct);
    return product is not null ? Results.Ok(product) : Results.NotFound();
});

app.MapGet("/products", async (ISender sender, CancellationToken ct) =>
{
    var products = await sender.Send(new ListProductsQuery(), ct);
    return Results.Ok(products);
});

app.MapPost("/products", async (
    CreateProductCommand command, ISender sender, IPublisher publisher, CancellationToken ct) =>
{
    var id = await sender.Send(command, ct);
    await publisher.Publish(new ProductCreatedNotification(id, command.Name), ct);
    return Results.Created($"/products/{id}", new { id });
});

app.MapPut("/products/{id:guid}", async (
    Guid id, UpdateProductCommand command, ISender sender, CancellationToken ct) =>
{
    if (id != command.Id) return Results.BadRequest();
    var ok = await sender.Send(command, ct);
    return ok ? Results.NoContent() : Results.NotFound();
});

app.MapDelete("/products/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
{
    var ok = await sender.Send(new DeleteProductCommand(id), ct);
    return ok ? Results.NoContent() : Results.NotFound();
});

app.Run();
