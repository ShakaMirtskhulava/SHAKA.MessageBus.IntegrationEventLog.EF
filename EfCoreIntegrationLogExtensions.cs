﻿using MessageBus.Abstractions;
using MessageBus.IntegrationEventLog.EF.Models;
using MessageBus.IntegrationEventLog.EF.Services;
using MessageBus.IntegrationEventLog.Publisher;
using MessageBus.IntegrationEventLog.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MessageBus.IntegrationEventLog.EF;

public static class EfCoreIntegrationLogExtensions
{
    const string INTEGRATION_EVNET_LOG_TABLE_NAME = "IntegrationEventLogs";
    const string FAILED_MESSAGE_CHAIN_TABLE_NAME = "FailedMessageChains";
    const string FAILED_MESSAGE_TABLE_NAME = "FailedMessages";

    public static void UseIntegrationEventLogs(this ModelBuilder builder)
    {
        builder.Entity<EFCoreIntegrationEventLog>(builder =>
        {
            builder.ToTable(INTEGRATION_EVNET_LOG_TABLE_NAME);

            builder.HasKey(e => e.EventId);
        });

        builder.Entity<FailedMessageChainEF>(builder =>
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

        builder.Entity<FailedMessageEF>(builder =>
        {
            builder.ToTable(FAILED_MESSAGE_TABLE_NAME);
            builder.HasKey(e => e.Id);
        });
    }

    public static void ConfigureEventLogServices<TContext>(this IServiceCollection services, string eventTyepsAssemblyName)
        where TContext : DbContext
    {
        services.AddScoped<IIntegrationEventLogService, EFIntegrationEventLogService<TContext>>();
        services.AddScoped<UnitOfWork<TContext>>();

        services.AddScoped<IIntegrationEventService, EFCoreIntegrationEventService<TContext>>(
            provider =>
            {
                var scope = provider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork<TContext>>();
                var integrationEventLogService = scope.ServiceProvider.GetRequiredService<IIntegrationEventLogService>();
                var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<EFCoreIntegrationEventService<TContext>>>();

                return new EFCoreIntegrationEventService<TContext>(dbContext, unitOfWork, integrationEventLogService, eventBus, eventTyepsAssemblyName, logger);
            }
        );
    }

    public static void ConfigureEFCoreEventLogServicesWithPublisher<TContext>(this IServiceCollection services, Action<PublisherOptions> optionsAction) 
        where TContext : DbContext
    {
        services.AddScoped<IIntegrationEventLogService, EFIntegrationEventLogService<TContext>>();
        services.AddScoped<UnitOfWork<TContext>>();
        var options = services.ConfigurePublisher(optionsAction);

        services.AddScoped<IIntegrationEventService, EFCoreIntegrationEventService<TContext>>(
            provider =>
            { 
                var scope = provider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork<TContext>>();
                var integrationEventLogService = scope.ServiceProvider.GetRequiredService<IIntegrationEventLogService>();
                var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<EFCoreIntegrationEventService<TContext>>>();

                return new EFCoreIntegrationEventService<TContext>(dbContext, unitOfWork, integrationEventLogService, eventBus, options.EventTyepsAssemblyName, logger);
            }
        );
    }

}
