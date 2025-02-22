namespace Cognassist.Notification.Domain;

public class EmailNotification
    : Notification
{
    public required string To { get; set; }
    public string? Message { get; set; }
    public string? Cc { get; set; }
    public string? Bcc { get; set; }
    public string? Subject { get; set; }
}