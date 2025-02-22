namespace Cognassist.Notification.Domain;

public class SmsNotification
    : Notification
{
    public required string Message { get; set; }
    public required string Target { get; set; }
}