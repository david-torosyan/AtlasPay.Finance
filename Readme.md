# AtlasPay.Finance

AtlasPay.Finance is the AtlasPay money-management service. It provides customer accounts, transfers, withdrawals, internal deposits, double-entry accounting, and reliable finance events.

The service uses .NET 10, ASP.NET Core, Entity Framework Core, PostgreSQL, Keycloak JWT authentication, and Apache Kafka.

## Core functionality

- Create and manage customer accounts in AMD, USD, or EUR.
- Read account details, balances, and paginated transfer history.
- Transfer money between customers using the sender's account.
- Withdraw money with customer-funds and bank-cash checks.
- Credit accounts through the internal development deposit endpoint.
- Record every money movement as a balanced double-entry ledger transaction.
- Make financial requests idempotent through a unique `RequestId`.
- Save integration events through a transactional outbox and publish them to Kafka.

## Customer authorization

Customer endpoints require a valid JWT containing a `customer_id` claim.

- Customers can access and operate only on their own accounts.
- A transfer must start from an account owned by the authenticated customer.
- The destination account may belong to another customer.
- Transfer details are available to the owner of either participating account.
- Unknown or unauthorized resources are returned as `404 Not Found`.

## Customer API

| Method | Endpoint | Purpose |
|---|---|---|
| `POST` | `/api/accounts` | Create an account |
| `GET` | `/api/accounts/{accountId}` | Get account details |
| `GET` | `/api/accounts/{accountId}/balance` | Get the ledger balance |
| `GET` | `/api/accounts/{accountId}/transactions` | Get paginated transfer history |
| `POST` | `/api/accounts/{accountId}/activate` | Activate a pending account |
| `POST` | `/api/accounts/{accountId}/freeze` | Freeze an active account |
| `POST` | `/api/accounts/{accountId}/unfreeze` | Unfreeze an account |
| `POST` | `/api/transfers` | Transfer money |
| `GET` | `/api/transfers/{transferId}` | Get transfer details |
| `POST` | `/api/withdrawals` | Withdraw money |

Transaction history accepts `page` and `pageSize`. Defaults are page `1` and page size `20`; the maximum page size is `100`.

## Internal development endpoints

These endpoints return `404` outside the Development environment:

| Method | Endpoint | Purpose |
|---|---|---|
| `POST` | `/api/deposits` | Credit a customer account |
| `POST` | `/api/transfers/{transferId}/reverse` | Reverse a completed transfer |
| `GET` | `/api/ledger/reconciliation` | Compare stored and ledger-derived balances |
| `GET` | `/api/ledger/integrity` | Find unbalanced ledger transactions |

## Financial guarantees

- Transfers, deposits, withdrawals, ledger postings, balances, and outbox messages are committed in one PostgreSQL transaction.
- Account rows are locked during money movement to serialize concurrent balance changes.
- Accounts must be active and use the requested currency.
- Transfers and withdrawals cannot overdraw a customer balance.
- Every posted ledger transaction must have equal debits and credits per currency.
- Reusing a `RequestId` with the same request returns the original result; different details return `409 Conflict`.

## Finance events

Successful operations write `TransferCompletedV2`, `DepositCompletedV2`, `WithdrawalCompletedV2`, or `TransferReversedV2` to the outbox.

`AtlasPay.Finance.Worker` claims pending messages, publishes them to Kafka with idempotent producer settings, and records publication or retry state. Delivery is at least once, so consumers should deduplicate by event ID.

## Solution structure

- `AtlasPay.Finance.Domain` — entities, value objects, and ledger rules.
- `AtlasPay.Finance.Application` — commands, queries, handlers, and repository contracts.
- `AtlasPay.Finance.Infrastructure` — EF Core, PostgreSQL repositories, locking, and migrations.
- `AtlasPay.Finance` — authenticated HTTP API.
- `AtlasPay.Finance.Worker` — transactional-outbox Kafka publisher.
- `AtlasPay.Finance.IntegrationTests` — PostgreSQL-backed integration tests.

## Build and run

Configure `ConnectionStrings:FinanceDatabase`, JWT `Authentication:Authority` and `Authentication:Audience`, plus worker `Kafka:BootstrapServers` and `Kafka:Topic`.

```powershell
dotnet ef database update --project AtlasPay.Finance.Infrastructure --startup-project AtlasPay.Finance --context FinanceDbContext
dotnet build AtlasPay.Finance.slnx
dotnet run --project AtlasPay.Finance
dotnet run --project AtlasPay.Finance.Worker
```
