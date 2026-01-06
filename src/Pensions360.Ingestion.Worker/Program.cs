using Azure.Identity;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Pensions360.Application.Abstractions;
using Pensions360.Application.Pensions.Services;
using Pensions360.Ingestion.Worker.Consumers;
using Pensions360.Infrastructure;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        if (!context.HostingEnvironment.IsDevelopment())
        {
            var builtConfig = config.Build();
            var keyVaultUri = builtConfig["KeyVault:Uri"];

            if (string.IsNullOrWhiteSpace(keyVaultUri))
            {
                throw new InvalidOperationException("KeyVault:Uri must be configured for staging/production environments.");
            }

            config.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());
        }
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;
        var env = context.HostingEnvironment;

        services.AddInfrastructure(configuration, env);
        services.AddScoped<IPensionIngestionService, PensionIngestionService>();

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.AddConsumer<ProcessProviderFileConsumer>();

            x.UsingAzureServiceBus((ctx, cfg) =>
            {
                var sbConn = configuration["ServiceBus:ConnectionString"]
                             ?? throw new InvalidOperationException("ServiceBus connection string missing.");

                cfg.Host(sbConn);

                cfg.ReceiveEndpoint("provider-ingestion-commands", e =>
                {
                    e.ConfigureConsumer<ProcessProviderFileConsumer>(ctx);
                });
            });
        });
    })
    .Build();

await host.RunAsync();
