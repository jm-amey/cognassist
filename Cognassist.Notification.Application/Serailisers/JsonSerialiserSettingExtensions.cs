using Cognassist.Notification.Domain;
using JsonSubTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Cognassist.Notification.Application;

public static class JsonSerialiserSettingExtensions
{

    /// <summary>
    /// Register polymorphic serialisers for Notifications.
    /// </summary>
    /// <param name="settings">The <see cref="JsonSerializerSettings" /> to apply the serialiser settings.</param>
    public static JsonSerializerSettings AddNotificationSerialisationSettings(this JsonSerializerSettings settings)
    {
        settings.ContractResolver = new CamelCasePropertyNamesContractResolver();

        settings.Converters.Add(JsonSubtypesConverterBuilder
            .Of(typeof(Domain.Notification), nameof(Domain.Notification.NotificationType))
            .RegisterSubtype(typeof(EmailNotification), NotificationType.SmsNotification)
            .RegisterSubtype(typeof(SmsNotification), NotificationType.EmailNotification)
            .RegisterSubtype(typeof(PushNotification), NotificationType.PushNotification)
            .Build());

        return settings;
    }
}