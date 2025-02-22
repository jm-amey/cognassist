using MediatR;

namespace Cognassist.Notification.Application;

public class BatchNotificationRequest : IRequest<int>
{
    public IEnumerable<Domain.Notification>? Notifications { get; set; }
}