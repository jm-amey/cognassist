namespace Cognassist.Notification.Domain;

public class PushNotification
    : Notification
{
    public required string Message { get; set; }
    public required string Target { get; set; }
}