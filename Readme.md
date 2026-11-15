# AtlasPay.Finance

AtlasPay.Finance is a .NET 10 financial microservice responsible for account management, money transfers, deposits, withdrawals, and double-entry ledger accounting.

The service is built using Clean Architecture, CQRS principles, PostgreSQL, Entity Framework Core, and Apache Kafka.

## Core Functionality

### Account Management
- Manage financial accounts and their balances.
- Support account-based transaction history with pagination.
- Maintain account and ledger account relationships.

### Financial Operations
- **Transfers:** Move funds between accounts atomically.
- **Deposits:** Credit funds to an account.
- **Withdrawals:** Debit funds with balance validation.
- Support request identifiers for financial operation idempotency.
- Track financial operation statuses and timestamps.

### Double-Entry Ledger
- Record financial movements using balanced debit and credit entries.
- Maintain an auditable history of financial transactions.
- Separate ledger entries from account balance records.
- Execute related financial changes within PostgreSQL transactions.

## Transactional Outbox Pattern

- Persist integration events alongside financial operations.
- Store events in the `outbox_messages` table.
- Use a dedicated background Worker to publish pending events.
- Atomically claim messages using PostgreSQL `FOR UPDATE SKIP LOCKED`.
- Support worker ownership, temporary leases, and retry scheduling.
- Track successful publications and failed delivery attempts.

## Apache Kafka Integration

- Publish financial events to the `finance.events` topic.
- Use `Confluent.Kafka` with producer idempotence and `acks=all`.
- Include stable event identifiers and event-type headers.
- Mark outbox events as published after Kafka delivery confirmation.
- Support at-least-once event delivery through the Outbox Pattern.

## Architecture

- **Domain:** Financial entities, value objects, and business rules.
- **Application:** Use cases, CQRS handlers, and persistence abstractions.
- **Infrastructure:** EF Core, PostgreSQL repositories, and persistence configuration.
- **API:** HTTP endpoints for financial operations.
- **Worker:** Background outbox processing and Kafka publishing.
- **IntegrationTests:** Database-backed integration tests.

## Technology Stack

.NET 10 · ASP.NET Core · EF Core · PostgreSQL 17 · Apache Kafka · Confluent.Kafka · Podman · Clean Architecture · CQRS

## Current Status

Core financial operations and the Outbox-to-Kafka publishing workflow are implemented. Successful publication of three outbox events has been verified.

Remaining work includes comprehensive financial correctness tests, concurrency testing, failure recovery, retry improvements, security hardening, observability, and Kubernetes deployment.

## API Endpoints

### Accounts

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/accounts` | Create a financial account |
| `GET` | `/api/accounts/{accountId}` | Retrieve account details |
| `GET` | `/api/accounts/{accountId}/balance` | Retrieve the current account balance |
| `GET` | `/api/accounts/{accountId}/transactions` | Retrieve paginated account transfer history |

### Transfers

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/transfers` | Transfer funds between accounts |
| `GET` | `/api/transfers/{transferId}` | Retrieve transfer details |
| `GET` | `/api/transfers/by-request/{requestId}` | Retrieve a transfer by its request identifier |

### Deposits

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/deposits` | Deposit funds into an account |
| `GET` | `/api/deposits/{depositId}` | Retrieve deposit details |

### Withdrawals

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/withdrawals` | Withdraw funds from an account |
| `GET` | `/api/withdrawals/{withdrawalId}` | Retrieve withdrawal details |

### API Behavior

- Financial operations use request identifiers to support idempotency.
- Money movements and corresponding ledger entries are committed atomically.
- Account transaction history supports `page` and `pageSize` query parameters.
- Successfully committed financial operations generate persistent outbox events.
- Outbox events are published asynchronously to the Kafka `finance.events` topic.
- Kafka publishing is handled by `AtlasPay.Finance.Worker`, not the HTTP API.

dotnet ef migrations add AddWithdrawals --project AtlasPay.Finance.Infrastructure --startup-project AtlasPay.Finance --context FinanceDbContext --output-dir Persistence/Migrations

dotnet ef database update --project AtlasPay.Finance.Infrastructure --startup-project AtlasPay.Finance --context FinanceDbContext

dotnet run --project .\AtlasPay.Finance.Worker\AtlasPay.Finance.Worker.csproj