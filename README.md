# SHIP SeS Extractor Mapping API

This repository contains the **Mapping API and services** for the **Smart Health Information Platform (SHIP) SeS Extractor**, which enables flexible, facility-specific EMR-to-FHIR mapping for health data synchronization.

It is part of the broader SHIP Edge Server (SeS) ecosystem and is designed to support dynamic field transformation, hybrid mapping scenarios, and self-service onboarding of health facilities with minimal technical overhead.

---

##  Repository Structure

---

## Features
 **Dynamic EMR-to-FHIR mapping engine** for Patient and related resources
-  **Fallback logic** for prioritized fields (e.g., NIN → BVN → Passport)
-  **Extensible mapping templates** for name, address, contact, identifiers, telecom, etc.
-  **Built-in validation hooks** for data completeness and standardization
-  **API-first design** with OpenAPI/Swagger support
-  Docker-compose for easy local development and testing 

---

## Repository Structure (Domain-Driven Design)
```
ship.ses.extractor.mapping.api/
│── src/
│ ├── Ship.Ses.Extractor.Application/ # Application Services (Use Cases, Command Handlers)
│ ├── Ship.Ses.Extractor.Domain/ # Domain Layer (Entities, Aggregates, Domain Services)
│ ├── Ship.Ses.Extractor.Infrastructure/ # Infrastructure Layer (Persistence, External Integrations)
│ ├── Ship.Ses.Extractor.Presentation.Api/ # ASP.NET Web API project (Controllers, DI setup)
│
│── tests/
│ ├── Ship.Ses.Extractor.UnitTests/ # Unit tests for domain & application logic
│ ├── Ship.Ses.Extractor.IntegrationTests/ # Integration tests for API & database
│
│── docker-compose.yml # Dockerized setup for local development (DB, API)
│── Ship.Ses.Extractor.sln # Visual Studio Solution
│── .gitignore
│── LICENSE
│── README.md
```

---
## Tech Stack

- [.NET 8](https://dotnet.microsoft.com/)
- ASP.NET Core Web API
- Entity Framework Core (PostgreSQL or MySQL)
- MongoDB (for FHIR sync pool)
- xUnit & FluentAssertions (testing)
- Docker & Docker Compose

---

## Getting Started

### 1. Clone the Repository

```bash
git clone https://xxxxxx/ship.ses.extractor.mapping.api.git
cd ship.ses.extractor.mapping.api
**Run Locally via Docker**

docker-compose up --build
This spins up:

Mapping API (on http://localhost:5000)

**Access API Docs**
Once running, open:

http://localhost:5000/swagger

**Running Tests**

dotnet test Ship.Ses.Extractor.UnitTests
dotnet test Ship.Ses.Extractor.IntegrationTests

Packaging & Deployment
To build the API for production:


dotnet publish ./src/Ship.Ses.Extractor.Presentation.Api -c Release -o ./publish
Use Docker or your preferred CI/CD pipeline for deployment.

**Related Projects**
SHIP Edge Server Extractor
SHIP Developer Portal
FHIR Mapping UI (Blazor)
**Contributing**
We welcome contributions! Please open issues for feature requests or bug reports. Fork and submit pull requests via feature branches (e.g., feature/mapping-ui-enhancement).
