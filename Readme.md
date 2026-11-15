flowchart TB
    API["Finance.Api"]
    APP["Finance.Application"]
    INFRA["Finance.Infrastructure"]
    DOMAIN["Finance.Domain"]

    API --> APP
    API --> INFRA
    APP --> DOMAIN
    INFRA --> APP
    INFRA --> DOMAIN

    style DOMAIN fill:#dcfce7,color:#166534,stroke:#16a34a


    Endpoint	Purpose
POST /api/accounts	Create account
GET /api/accounts/{id}	Get account
POST /api/accounts/{id}/activate	Activate account
POST /api/accounts/{id}/freeze	Freeze account
POST /api/accounts/{id}/unfreeze	Unfreeze account
GET /api/accounts/{id}/balance	Get balance
POST /api/transfers	Transfer money
GET /api/transfers/{id}	Get transfer details
GET /api/accounts/{id}/transactions	Paginated transfer history

dotnet ef migrations add AddWithdrawals --project AtlasPay.Finance.Infrastructure --startup-project AtlasPay.Finance --context FinanceDbContext --output-dir Persistence/Migrations

dotnet ef database update --project AtlasPay.Finance.Infrastructure --startup-project AtlasPay.Finance --context FinanceDbContext