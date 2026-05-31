# FinSight AI - Quick Start Guide

## 5-Minute Setup

### Prerequisites
- Docker & Docker Compose installed
- .NET 10 SDK
- Node.js 20+

### Start Services

```bash
# Navigate to project
cd ~/Professional/Code/FinSight

# Start infrastructure
docker-compose up -d

# Wait for services to be healthy (~30 seconds)
docker-compose ps

# Verify services running:
# ✓ postgres-identity
# ✓ postgres-transaction
# ✓ postgres-portfolio
# ✓ postgres-document
# ✓ postgres-ai
# ✓ keycloak
```

### Access Services

| Service | URL | Purpose |
|---------|-----|---------|
| Keycloak Admin | http://localhost:8080 | Identity Management |
| PostgreSQL | localhost:5432 | Databases (see docker-compose.yml for ports) |

### First Steps

1. **Access Keycloak**
   ```
   URL: http://localhost:8080/admin
   Username: admin
   Password: admin
   ```

2. **Create Realm**
   - Realm name: `finsight`
   - Enabled: Yes

3. **Create Client**
   - Client ID: `finsight-ui`
   - Client Protocol: `openid-connect`
   - Access Type: `public`
   - Valid Redirect URIs: `http://localhost:4200/*`

4. **Configure User**
   - Create user in Keycloak
   - Set password (temporary, requires change on first login)
   - Assign roles (Customer, Advisor, Admin)

## Project Structure Overview

```
FinSight/
├── docs/                  # Documentation
│   ├── ARCHITECTURE.md    # System design
│   ├── SERVICE_DESIGN.md  # Microservice patterns
│   ├── SETUP.md          # Installation guide
│   └── ...
├── services/             # Microservices (to be created)
│   ├── identity-service/
│   ├── transaction-service/
│   ├── portfolio-service/
│   ├── document-service/
│   └── ai-service/
├── gateway/              # API Gateway (to be created)
├── frontend/             # Angular UI (to be created)
├── shared/               # Shared frameworks
│   └── FinSight.Contracts/
├── docker/               # Docker configurations
└── .github/workflows/    # CI/CD pipelines
```

## Next Steps

### For Backend Developers

1. **Review Architecture**: Read [ARCHITECTURE.md](./docs/ARCHITECTURE.md)
2. **Study Service Pattern**: Read [SERVICE_DESIGN.md](./docs/SERVICE_DESIGN.md)
3. **Set Up First Service**: Follow [Create Identity Service](#create-identity-service) below

### For Frontend Developers

1. **Review Architecture**: Read [ARCHITECTURE.md](./docs/ARCHITECTURE.md)
2. **Set Up Angular**: See Angular setup section below
3. **Study Contracts**: Read [API_CONTRACTS.md](./docs/API_CONTRACTS.md)

### For DevOps/Infrastructure

1. **Review Docker Setup**: Check [docker-compose.yml](./docker-compose.yml)
2. **Review Deployment**: Read [DEPLOYMENT.md](./docs/DEPLOYMENT.md)
3. **Study CI/CD**: Review [.github/workflows](./.github/workflows/)

## Create Identity Service

```bash
cd services
mkdir identity-service
cd identity-service

# Initialize .NET solution
dotnet new globaljson --sdk-version 10.0.0
dotnet new sln -n IdentityService

# Create projects
dotnet new classlib -n IdentityService.Contracts -f net10.0
dotnet new classlib -n IdentityService.Business -f net10.0
dotnet new classlib -n IdentityService.Data -f net10.0
dotnet new web -n IdentityService.Api -f net10.0
dotnet new xunit -n IdentityService.Tests -f net10.0

# Add to solution
dotnet sln add IdentityService.Contracts/IdentityService.Contracts.csproj
dotnet sln add IdentityService.Business/IdentityService.Business.csproj
dotnet sln add IdentityService.Data/IdentityService.Data.csproj
dotnet sln add IdentityService.Api/IdentityService.Api.csproj
dotnet sln add IdentityService.Tests/IdentityService.Tests.csproj

# Add shared reference
dotnet add IdentityService.Contracts reference ../../shared/FinSight.Contracts
dotnet add IdentityService.Business reference IdentityService.Contracts ../../shared/FinSight.Contracts
dotnet add IdentityService.Data reference IdentityService.Business ../../shared/FinSight.Contracts
dotnet add IdentityService.Api reference IdentityService.Business IdentityService.Data ../../shared/FinSight.Contracts
dotnet add IdentityService.Tests reference IdentityService.Api ../../shared/FinSight.Contracts

# Add NuGet packages
dotnet add IdentityService.Data package Microsoft.EntityFrameworkCore
dotnet add IdentityService.Data package Microsoft.EntityFrameworkCore.PostgreSQL
dotnet add IdentityService.Api package Grpc.AspNetCore
dotnet add IdentityService.Api package Microsoft.AspNetCore.Authentication.JwtBearer
```

## Create Angular Frontend

```bash
cd frontend

# Install Angular CLI if needed
npm install -g @angular/cli@18

# Create new Angular app
ng new finsight-ui --package-manager=npm --routing --skip-git

cd finsight-ui

# Install dependencies
npm install

# Start dev server
ng serve --open
```

**Angular app runs at**: http://localhost:4200

## Essential Commands

### Docker
```bash
# View running containers
docker-compose ps

# View logs from service
docker-compose logs -f identity-service

# Stop all services
docker-compose down

# Stop and remove volumes (WARNING: deletes data)
docker-compose down -v

# Rebuild containers
docker-compose build --no-cache
```

### .NET
```bash
# Build solution
dotnet build

# Run tests
dotnet test

# Run specific service
dotnet run --project services/identity-service/IdentityService.Api

# Watch mode (auto-rebuild on changes)
dotnet watch --project services/identity-service/IdentityService.Api run
```

### Database
```bash
# Connect to PostgreSQL
psql -h localhost -U postgres -d identitydb

# Create database
psql -h localhost -U postgres -c "CREATE DATABASE newdb;"

# Run migrations
dotnet ef database update --project services/identity-service/IdentityService.Data
```

### Angular
```bash
# Generate component
ng generate component features/transactions/transaction-list

# Generate service
ng generate service services/transaction

# Build for production
ng build --prod

# Run unit tests
ng test
```

## Troubleshooting

### Ports Already in Use

```bash
# Find process using port
lsof -i :5001

# Kill process
kill -9 <PID>
```

### Database Connection Issues

```bash
# Test connection
psql -h localhost -U postgres -d postgres -c "SELECT 1"

# Check if databases exist
psql -h localhost -U postgres -c "\l"
```

### Keycloak Issues

```bash
# Check Keycloak status
curl -s http://localhost:8080/health | jq .

# View Keycloak logs
docker logs keycloak
```

## Resources

- **Documentation**: `/docs` folder
- **Architecture**: [ARCHITECTURE.md](./docs/ARCHITECTURE.md)
- **Setup Guide**: [SETUP.md](./docs/SETUP.md)
- **API Contracts**: [API_CONTRACTS.md](./docs/API_CONTRACTS.md)
- **Development Guidelines**: [DEVELOPMENT_GUIDELINES.md](./docs/DEVELOPMENT_GUIDELINES.md)
- **Roadmap**: [ROADMAP.md](./ROADMAP.md)

## Getting Help

1. Check [SETUP.md](./docs/SETUP.md) for detailed installation
2. Review [DEVELOPMENT_GUIDELINES.md](./docs/DEVELOPMENT_GUIDELINES.md) for best practices
3. See [ARCHITECTURE.md](./docs/ARCHITECTURE.md) for design patterns
4. Check GitHub Issues for common problems

---

**Status**: Phase 1 Foundation  
**Last Updated**: May 2026

💡 **Tip**: Keep this guide open while working on the project!
