## Request Classifier

Municipal service request classification project.

## Application Flow

```mermaid
flowchart TD
    A[Citizen sends a service request] --> B[RequestClassifier.Api]
    B --> C[ServiceRequestController]
    C --> D[ServiceRequestService]
    D --> E[(SQL Server Database)]
    D --> F[RequestClassifier.ML]
    F --> G[Text is converted into features]
    G --> H[ML model predicts a category]
    H --> I{Prediction score >= 0.80?}
    I -- Yes --> J[Request is automatically assigned]
    I -- No --> K[Employee reviews the prediction]
    J --> L[Assigned Department and Category]
    K --> L
    L --> M[Request status is updated]
    M --> N[RequestStatusHistory is stored]

## Structure

flowchart TB
    API[RequestClassifier.Api<br/>Controllers, JWT, Swagger]

    APP[RequestClassifier.Application<br/>DTOs, Interfaces, Services]

    INF[RequestClassifier.Infrastructure<br/>EF Core, Identity, SQL Server]

    DOMAIN[RequestClassifier.Domain<br/>Entities, Enums]

    ML[RequestClassifier.ML<br/>Training, Prediction, Model]

    API --> APP
    API --> INF
    API --> ML

    APP --> DOMAIN
    INF --> APP
    INF --> DOMAIN
    ML --> DOMAIN
