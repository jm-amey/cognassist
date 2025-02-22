using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cognassist.Notification.Domain;

public abstract class Notification : IEntity
{
    [JsonConverter(typeof(StringEnumConverter))]
    public virtual NotificationType NotificationType { get; }
    public DateTime ScheduleDate { get; set; }
    public string? Importance { get; set; }
    public string Id { get; set; }
}
