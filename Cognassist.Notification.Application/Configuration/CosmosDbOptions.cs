namespace Cognassist.Notification.Application;

public sealed class CosmosDbOptions
{
    public const string ConfigPath = "CosmosDb";
    public required string Account { get; set; }
    public required string Key { get; set; }
    public required string DatabaseName { get; set; }
}