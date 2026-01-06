## Overview

This repository is a **portfolio showcase project** that demonstrates how to design and implement a **production-style, event-driven .NET system on Azure**, using modern architectural patterns and cloud-native services.

The project intentionally models a **simplified domain** (pension data ingestion and querying) to focus on **architecture, patterns, and operational concerns**, rather than full domain completeness.

It is designed to give reviewers a clear view of how I approach:
- Clean Architecture / Hexagonal Architecture
- CQRS with MediatR
- Event-driven systems
- Azure messaging and data services
- Containerized development and deployment
- Environment-safe configuration and secrets management

---

## What the System Does

At a high level, the system supports two core capabilities:

1. **Asynchronous ingestion of pension data**
   - External providers are simulated via CSV files uploaded to Blob Storage.
   - A background worker processes these files asynchronously.
   - Pension data is stored in Cosmos DB.
   - Domain and integration events are published as part of ingestion.

2. **Read-only API for querying pension data**
   - A Minimal API exposes a `Find Pensions` endpoint.
   - Consumers can query pension pots by National Insurance Number (NINO).
   - The API uses CQRS and reads from Cosmos DB.
   - No business logic exists in the API layer.

The emphasis is on **data flow, separation of concerns, and scalability**, not on implementing a complete pensions platform.

---

## Current Functionality

### Pension Data Ingestion (Write Side)

- Pension providers are simulated using CSV files.
- A message-driven ingestion worker:
  - Downloads provider files from Blob Storage
  - Parses pension records
  - Creates or updates citizens and pension pots in Cosmos DB
  - Publishes domain/integration events (e.g. pension pot created or updated)

This flow demonstrates:
- Event-driven processing
- Asynchronous background workers
- Message-based orchestration
- Idempotent data handling

---

### Pension Query API (Read Side)

- A Minimal API exposes:


- The API:
- Uses MediatR to dispatch a CQRS query
- Reads pension data from Cosmos DB
- Returns a normalized, read-only response

This flow demonstrates:
- CQRS separation
- Thin API layer
- Clean dependency boundaries
- Container-ready API design

---

## Architecture Highlights

- **Domain Layer**
- Pure domain model (entities, value objects, domain events)
- No dependencies on frameworks or infrastructure

- **Application Layer**
- Use cases, CQRS handlers, and domain orchestration
- Defines interfaces (ports) for infrastructure concerns

- **Infrastructure Layer**
- Azure Cosmos DB, Blob Storage, Service Bus integrations
- MassTransit for messaging
- Implements application interfaces

- **API & Worker**
- API handles HTTP requests only
- Worker handles asynchronous ingestion
- Both depend on the Application layer, not on each other

---

## What This Project Is (and Is Not)

**This project is:**
- A reference implementation
- A showcase of architectural patterns
- Production-oriented in structure and practices

**This project is not:**
- A full pensions domain implementation
- Integrated with real pension providers
- A complete end-user product

This scope is intentional to keep the focus on **design quality and engineering practices**.

---

## Technology Stack (Current)

- .NET 8
- Minimal APIs
- MediatR (CQRS)
- Azure Cosmos DB (SQL API)
- Azure Service Bus
- Azure Blob Storage
- MassTransit
- Docker & Docker Compose
- Azure Key Vault (staging/production configuration)

---

## Configuration & Secrets

### Local Development (Environment Variables / User Secrets)

Local development keeps secrets out of source control by using **environment variables** or **user secrets**. The application expects the following configuration keys (these are used in `src/Pensions360.Infrastructure/DependencyInjection.cs`):

- `Storage:ConnectionString`
- `Cosmos:ConnectionString`
- `ServiceBus:ConnectionString`

Example (environment variables):

```
Storage__ConnectionString=UseDevelopmentStorage=true
Cosmos__ConnectionString=AccountEndpoint=...
ServiceBus__ConnectionString=Endpoint=sb://...
```

### Staging/Production (Azure Key Vault)

In non-development environments, both the API and worker load secrets from **Azure Key Vault** using `DefaultAzureCredential`. Provide the Key Vault URI via:

```
KeyVault__Uri=https://<your-key-vault-name>.vault.azure.net/
```

#### Key Vault Secret Naming Conventions

The Key Vault configuration provider maps `--` to `:` for hierarchical keys. Use the following naming scheme to map to the expected configuration keys:

- `Storage--ConnectionString` → `Storage:ConnectionString`
- `Cosmos--ConnectionString` → `Cosmos:ConnectionString`
- `ServiceBus--ConnectionString` → `ServiceBus:ConnectionString`

With this in place, the API and worker will read the same configuration keys in all environments while keeping secrets in Key Vault for staging/production.

---

## Future Enhancements (Planned)

- API Management (gateway, versioning)
- React front-end
- CI/CD pipelines
- Additional event consumers (notifications, ops, analytics)

---
## Architecture & Flow Diagram
                        ┌──────────────────────┐
                        │   External Provider  │
                        │   (CSV Files)        │
                        └───────────┬──────────┘
                                    │
                                    │ Upload CSV
                                    ▼
                        ┌──────────────────────┐
                        │   Azure Blob Storage │
                        │   provider-ingestion │
                        └───────────┬──────────┘
                                    │
                                    │ ProcessProviderFileCommand
                                    ▼
                        ┌──────────────────────┐
                        │  Azure Service Bus   │
                        │  Queue               │
                        │  provider-ingestion  │
                        └───────────┬──────────┘
                                    │
                                    │ Consumed by
                                    ▼
                  ┌──────────────────────────────────┐
                  │  Ingestion Worker (.NET)         │
                  │                                  │
                  │  - Parse CSV                     │
                  │  - Upsert Citizens               │
                  │  - Upsert Pension Pots           │
                  │  - Publish Domain Events         │
                  └───────────┬───────────┬─────────┘
                              │           │
                              │           │ Publish events
                              │           ▼
                              │   ┌──────────────────────┐
                              │   │ Azure Service Bus     │
                              │   │ Domain Events         │
                              │   └──────────────────────┘
                              │
                              │ Write / Update
                              ▼
                    ┌──────────────────────────┐
                    │ Azure Cosmos DB           │
                    │ - Citizens                │
                    │ - PensionPots             │
                    └───────────┬──────────────┘
                                │
                                │ Read
                                ▼
                 ┌──────────────────────────────────┐
                 │  Pension Query API (.NET)        │
                 │  Minimal API + MediatR           │
                 │                                  │
                 │  GET /api/pensions/find          │
                 └───────────┬─────────────────────┘
                             │
                             │ JSON Response
                             ▼
                    ┌──────────────────────────┐
                    │ API Consumers             │
                    │ (Postman / UI / Client)   │
                    └──────────────────────────┘
