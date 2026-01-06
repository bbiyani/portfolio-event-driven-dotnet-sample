using System.Net.Http;
using Azure.Storage.Blobs;
using MassTransit;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pensions360.Application.Abstractions;
using Pensions360.Application.Pensions.Services;
using Pensions360.Infrastructure.Messaging;
using Pensions360.Infrastructure.Persistence;

namespace Pensions360.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        services.AddSingleton(sp =>
        {
            var conn = configuration["Storage:ConnectionString"]
                      ?? throw new InvalidOperationException("Storage connection string missing.");
            return new BlobServiceClient(conn);
        });

        services.AddSingleton(sp =>
        {
            var connStr = configuration["Cosmos:ConnectionString"]
                          ?? throw new InvalidOperationException("Cosmos connection string missing.");

            CosmosClientOptions? options = null;
            if (env.IsDevelopment())
            {
                options = new CosmosClientOptions
                {
                    ConnectionMode = ConnectionMode.Gateway,
                    HttpClientFactory = () =>
                    {
                        var handler = new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback =
                                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                        };
                        return new HttpClient(handler);
                    }
                };
            }

            return options is null ? new CosmosClient(connStr) : new CosmosClient(connStr, options);
        });

        services.AddSingleton(sp =>
        {
            var client = sp.GetRequiredService<CosmosClient>();
            var dbName = configuration["Cosmos:Database"] ?? "PensionsDb";
            var containerName = configuration["Cosmos:PensionPotsContainer"] ?? "PensionPots";
            return client.GetContainer(dbName, containerName);
        });

        services.AddScoped<IPensionIngestionService, PensionIngestionService>();
        services.AddScoped<IPensionPotStateStore, CosmosPensionPotStateStore>();
        services.AddScoped<IPensionPotReadRepository, CosmosPensionPotReadRepository>();
        services.AddScoped<IPensionDomainEventPublisher, ServiceBusDomainEventPublisher>();

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            x.UsingAzureServiceBus((context, cfg) =>
            {
                var sbConn = configuration["ServiceBus:ConnectionString"]
                             ?? throw new InvalidOperationException("ServiceBus connection string missing.");

                cfg.Host(sbConn);
            });
        });

        return services;
    }
}
