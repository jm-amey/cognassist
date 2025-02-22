using MediatR;
using Cognassist.Notification.Domain;
using Cognassist.Notification.Cache;

namespace Cognassist.Notification.Application;

public class BatchNotificationRequestHandler(ISortedSetRepository<Domain.Notification> sortedSetRepo) : IRequestHandler<BatchNotificationRequest, int>
{
    public async Task<int> Handle(BatchNotificationRequest request, CancellationToken cancellationToken)
    {
        if (request.Notifications is null || !request.Notifications.Any())
        {
            return 200;
        }

        foreach (var notification in request.Notifications)
        {
            sortedSetRepo.Add(Guid.NewGuid().ToString(), notification, notification.ScheduleDate.Ticks);
        }

        return 200;
    }
}