# SHIP Edge Server (SeS) - Data Extraction Component
🚀 **ship.ses.extractor**  

![Deploy Status](https://olasunkanmifadayomi@bitbucket.org/interswitch/ship-ses-extractor-service/actions/workflows/deployuat.yml/badge.svg)

## Overview
The **SHIP Edge Server (SeS) - Data Extraction Component** (**`ship.ses.extractor`**) is responsible for extracting healthcare data from **various EMR systems** and preparing it for transformation and synchronization with the **SHIP Core Platform**.

This service is part of the **SHIP Edge Server (SeS)** and is implemented using **.NET Core**, following **Domain-Driven Design (DDD)** principles for maintainability and scalability.

---

## Features
✅ **Multiple Data Extraction Methods**:
- **FHIR API** (For modern EMRs).
- **Custom API integration** (For non-FHIR EMRs).
- **Database extraction** (SQL and NoSQL support).
- **File-based ingestion** (XML/JSON formats).

✅ **Background worker service** for scheduled data pulls.  
✅ Implements **retry logic** for failed extractions.  
✅ Logs extraction events for **auditability**.  
✅ Secure **OAuth2 authentication** for API-based data extraction.  
✅ **Domain-Driven Design (DDD)** structured architecture.  

---

## Repository Structure (Domain-Driven Design)
```
ship.ses.extractor/
│── src/
│   ├── Ship.Ses.Extractor.Api/          # API layer (if applicable)
│   ├── Ship.Ses.Extractor.Application/  # Application Services (Use Cases, Command Handlers)
│   ├── Ship.Ses.Extractor.Domain/       # Domain Layer (Entities, Aggregates, Domain Services)
│   ├── Ship.Ses.Extractor.Infrastructure/ # Infrastructure Layer (Persistence, External Integrations)
│   ├── Ship.Ses.Extractor.Worker/       # Background worker service for scheduled extractions
│── tests/
│   ├── Ship.Ses.Extractor.UnitTests/    # Unit tests for domain & application logic
│   ├── Ship.Ses.Extractor.IntegrationTests/ # Integration tests for API & DB interactions
│── docker-compose.yml
│── README.md
│── .gitignore
│── LICENSE
│── Ship.Ses.Extractor.sln
```

---

## Installation
### **Prerequisites**
- **.NET 8.0+**
- **Docker** (for containerized deployments)
- **SQL Server / PostgreSQL** (for database extraction)
- **RabbitMQ / Kafka** (for event-driven extraction)

### **Clone the Repository**
```sh
ggit clone https://olasunkanmifadayomi@bitbucket.org/interswitch/ship-ses-extractor-service.git
cd ship-ses-extractor-service
```

### **Setup Configuration**
- Copy `.env.example` to `.env` and configure your environment variables:
  ```sh
  cp .env.example .env
  ```

- Edit `.env` with your preferred settings:
  ```ini
  DB_CONNECTION_STRING="Host=localhost;Database=ses_db;Username=ses_user;Password=your_password"
  FHIR_API_URL="https://fhir.emr.local"
  CUSTOM_API_URL="https://legacy-emr.local/api"
  SFTP_HOST="sftp.server.local"
  ```

---

## Running the Application
### **Run with Docker**
```sh
docker-compose up --build
```

### **Run Locally**
1. Restore dependencies:
   ```sh
   dotnet restore
   ```
2. Build the solution:
   ```sh
   dotnet build
   ```
3. Run the background worker:
   ```sh
   dotnet run --project src/Ship.Ses.Extractor.Worker
   ```

---

## Configuration
The application is configured using **`appsettings.json`** and supports **environment-based configurations**.

Example `appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "System": "Error"
    }
  },
  "Database": {
    
  },
  "ApiSettings": {
    "FhirBaseUrl": "https://fhir.emr.local",
    "CustomApiBaseUrl": "https://legacy-emr.local/api"
  }
}
```

---

## API Endpoints
| **Method** | **Endpoint** | **Description** |
|-----------|-------------|-----------------|
| `POST` | `/extract/fhir/patient` | Extract patient data from FHIR API |


---

## Authentication

### **Example Authorization Header**
```http

```

---

## Logging & Monitoring
SHIP Ses logs events using **Serilog**, and all logs are forwarded to **ELK Stack (Elasticsearch, Logstash, Kibana)**.

### **Log Example**
```json
{
  "timestamp": "2025-02-15T12:30:45Z",
  "level": "Information",
  "message": "Patient data extracted successfully",
  "context": {
    "patientId": "12345",
    "source": "FHIR API"
  }
}
```

---

## Testing
Run **unit tests**:
```sh
dotnet test
```
Run **integration tests**:
```sh
dotnet test tests/Ship.Ses.Extractor.IntegrationTests
```

---

## Deployment
**Kubernetes Helm Chart Deployment**
```sh
helm upgrade --install ses-extractor charts/ses-extractor
```

**Azure Deployment (Using ACR & AKS)**
```sh
az acr build --image ses-extractor:v1.0 --registry shipcontainerregistry .
az aks deploy --name ses-extractor --image shipcontainerregistry/ses-extractor:v1.0
```

---

## License
📜 *

---

## Contacts & Support
- 📧 **Support**: support@
- 🚀 **Contributors**: @ses  
- 📚 **Docs**: [Confluence Page](https://confluence.ses.io/docs)

---
