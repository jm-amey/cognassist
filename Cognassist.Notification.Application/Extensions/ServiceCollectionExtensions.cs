using Cognassist.Notification.Cache;
using Cognassist.Notification.ComsosDb;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Cognassist.Notification.Application;

public static class ServiceCollectionExtensions
{
    private static readonly string NotificationsContainer = "Notifications";

    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplicationInsightsTelemetry();

        services.Configure<CosmosDbOptions>(configuration.GetSection(CosmosDbOptions.ConfigPath));

        services.AddScoped<IRepository<Domain.Notification>, CosmosDbRepository<Domain.Notification>>(sp =>
        {
            var jsonSerialiserSettings = new JsonSerializerSettings().AddNotificationSerialisationSettings();
            var serialiser = new NewtonsoftCosmosSerialiser(jsonSerialiserSettings);
            var dbConfig = sp.GetRequiredService<CosmosDbOptions>();

            var cosmosClient = new CosmosClientBuilder(dbConfig.Account, dbConfig.Key).WithCustomSerializer(serialiser).Build();
            cosmosClient.InitialiseNotificationPersistence(dbConfig);

            var container = cosmosClient.GetContainer(dbConfig.DatabaseName, NotificationsContainer);

            return new CosmosDbRepository<Domain.Notification>(container);
        });

        services.AddScoped<ISortedSetRepository<Domain.Notification>, RedisSortedSetRepository<Domain.Notification>>(sp =>
        {
            return null;
        });

        return services;
    }

    public static void InitialiseNotificationPersistence(this CosmosClient cosmos, CosmosDbOptions options)
    {
        _ = cosmos.GetDatabase(options.DatabaseName)
                .DefineContainer(name: NotificationsContainer, "/requestId")
                .WithDefaultTimeToLive(604800)
                .CreateIfNotExistsAsync().GetAwaiter().GetResult();
    }
}
