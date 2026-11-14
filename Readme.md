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