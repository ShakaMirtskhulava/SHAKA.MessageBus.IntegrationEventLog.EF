# SHAKA.MessageBus.IntegrationEventLog.EF

This project provides an **EF Core-based implementation** of `SHAKA.MessageBus.IntegrationEventLog`, designed for seamless database interactions in an Event-Driven System.

## Overview

### EF Core Model Implementations
The package includes EF Core implementations for the following `SHAKA.MessageBus.IntegrationEventLog` models:
- **`IIntegrationEventLog`**
- **`IFailedMessageChain`**
- **`IFailedMessage`**

Each model is configured using EF Core data annotations to ensure proper schema mapping.

## Service Implementations

### IntegrationEventLogService
This service is responsible for managing `IntegrationEventLog` operations, including:
- **`SaveEvent()`** – Converts an `IntegrationEvent` into an `IIntegrationEventLog` and persists it in the database.
- **`AddInFailedMessageChain()`** – Creates a chain for failed messages belonging to the same entity and appends new failures if the chain already exists. The method extracts exception details to generate `FailedMessage` entries.
- **State Management Methods:**
  - `MarkEventAsPublished()`
  - `MarkEventAsInProgress()`
  - `MarkEventAsFailed()`

### IntegrationEventService
This service handles `IntegrationEvent` operations and interacts with `IIntegrationEventLogService` to manage event persistence. It provides:
- **`GetPendingEvents(batchSize)`** – Loads pending `IntegrationEvents` for publishing in batches.
- **`RetrieveFailedEventsToRepublish(chainBatchSize)`** – Identifies failed message chains marked with `ShouldRepublish`, loads their failed messages, converts them back into events for republishing, and removes the processed chains and messages.
- **Add, Update, and Remove methods** – These ensure atomic database writes using the **Unit of Work** pattern with a default resilience strategy.

## EF Core Configuration & Dependency Injection
To facilitate EF Core entity relations and service registration, this package provides the `EfCoreIntegrationLogExtensions` class, which includes three key extension methods:

### `UseIntegrationEventLogs()`
Configures EF Core entity relations using the **Fluent API**.

### `ConfigureEventLogServices()`
Registers essential services in the **DI Container**, including:
- `IIntegrationEventLogService`
- `IntegrationEventService`
- Logging services
- A generic EF Core **database context**, allowing consumers to inject their own `DbContext` implementation.

### `ConfigureEFCoreEventLogServicesWithPublisher()`
In addition to configuring services, this method also registers the **Background Service publisher**.

## Usage Scenarios
This package supports **two approaches** for integrating event publishing and handling logic:
1. **Single .NET Application Approach**: The same application handles both publishing and processing of events. Use `ConfigureEventLogServicesWithPublisher()` to set up the publisher and handlers within the same project.
2. **Distributed Approach**: The publisher and event handlers exist in separate applications. Use `ConfigureEventLogServices()` in the publisher app to log events, while another service handles publishing and processing.

---
This implementation enhances **event consistency, failure recovery, and transaction reliability** in an event-driven microservices architecture.
