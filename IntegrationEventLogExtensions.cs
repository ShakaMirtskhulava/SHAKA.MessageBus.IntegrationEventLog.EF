using MessageBus.Abstractions;
using MessageBus.IntegrationEventLog.EF.Models;
using MessageBus.IntegrationEventLog.EF.Services;
using MessageBus.IntegrationEventLog.Publisher;
using MessageBus.IntegrationEventLog.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace MessageBus.IntegrationEventLog.EF;

public static class IntegrationEventLogExtensions
{
    const string INTEGRATION_EVNET_LOG_TABLE_NAME = "IntegrationEventLogs";
    const string FAILED_MESSAGE_CHAIN_TABLE_NAME = "FailedMessageChains";
    const string FAILED_MESSAGE_TABLE_NAME = "FailedMessages";

    public static void UseIntegrationEventLogs(this ModelBuilder builder)
    {
        builder.Entity<Models.IntegrationEventLog>(builder =>
        {
            builder.ToTable(INTEGRATION_EVNET_LOG_TABLE_NAME);

            builder.HasKey(e => e.EventId);
        });

        builder.Entity<FailedMessageChain>(builder =>
        {
            builder.ToTable(FAILED_MESSAGE_CHAIN_TABLE_NAME);
            builder.HasKey(e => e.Id);
            builder.HasIndex(e => e.EntityId)
                   .IsUnique();
            builder.HasMany(e => e.FailedMessages)
                   .WithOne(fm => fm.FailedMessageChain)
                   .HasForeignKey(e => e.FailedMessageChainId)
                   .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<FailedMessage>(builder =>
        {
            builder.ToTable(FAILED_MESSAGE_TABLE_NAME);
            builder.HasKey(e => e.Id);
        });
    }

    public static void ConfigureEventLogServices<TContext>(this IServiceCollection services, string eventTyepsAssemblyName)
        where TContext : DbContext
    {
        services.AddScoped<IIntegrationEventLogService, IntegrationEventLogService<TContext>>();
        services.AddScoped<UnitOfWork<TContext>>();

        services.AddScoped<IIntegrationEventService, IntegrationEventService<TContext>>(
            provider =>
            {
                var scope = provider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork<TContext>>();
                var integrationEventLogService = scope.ServiceProvider.GetRequiredService<IIntegrationEventLogService>();
                var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<IntegrationEventService<TContext>>>();

                return new IntegrationEventService<TContext>(dbContext, unitOfWork, integrationEventLogService, eventBus, eventTyepsAssemblyName, logger);
            }
        );
    }

    public static void ConfigureEventLogServicesWithPublisher<TContext>(this IServiceCollection services, Action<PublisherOptions> optionsAction) 
        where TContext : DbContext
    {
        services.AddScoped<IIntegrationEventLogService, IntegrationEventLogService<TContext>>();
        services.AddScoped<UnitOfWork<TContext>>();
        var options = services.ConfigurePublisher(optionsAction);

        services.AddScoped<IIntegrationEventService, IntegrationEventService<TContext>>(
            provider =>
            { 
                var scope = provider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork<TContext>>();
                var integrationEventLogService = scope.ServiceProvider.GetRequiredService<IIntegrationEventLogService>();
                var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<IntegrationEventService<TContext>>>();

                return new IntegrationEventService<TContext>(dbContext, unitOfWork, integrationEventLogService, eventBus, options.EventTyepsAssemblyName, logger);
            }
        );
    }

}
