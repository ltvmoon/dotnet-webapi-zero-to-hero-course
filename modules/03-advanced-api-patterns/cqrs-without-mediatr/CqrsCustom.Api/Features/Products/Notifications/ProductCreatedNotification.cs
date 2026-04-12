using CqrsCustom.Api.Dispatcher;

namespace CqrsCustom.Api.Features.Products.Notifications;

public sealed record ProductCreatedNotification(Guid ProductId, string Name) : INotification;

public sealed class LogProductCreatedHandler(ILogger<LogProductCreatedHandler> logger)
    : INotificationHandler<ProductCreatedNotification>
{
    public ValueTask Handle(ProductCreatedNotification notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Product created: {Id} - {Name}", notification.ProductId, notification.Name);
        return ValueTask.CompletedTask;
    }
}
