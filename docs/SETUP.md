# FinSight AI - Development Setup Guide

## Prerequisites

### Required Software

| Tool | Version | Purpose |
|------|---------|---------|
| .NET SDK | 10.0+ | Backend services |
| Node.js | 20.0+ | Frontend & tooling |
| Docker | 27.0+ | Containerization |
| Docker Compose | 2.0+ | Local orchestration |
| PostgreSQL | 16+ | Databases |
| Keycloak | 24+ | Identity management |
| Git | 2.40+ | Version control |

### Optional Tools

- Visual Studio Code (recommended editor)
- Visual Studio 2025 (for Windows/.NET development)
- Azure Storage Emulator (for blob storage)
- Postman or Insomnia (API testing)

## Installation

### macOS

```bash
# Install Homebrew (if not installed)
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"

# Install core tools
brew install dotnet node docker postgresql keycloak git

# Install Docker Desktop
brew install --cask docker

# Start Docker daemon
open /Applications/Docker.app
```

### Windows

```powershell
# Using Chocolatey (recommended)
choco install dotnet-sdk nodejs docker-desktop postgresql keycloak git

# Or use Windows Package Manager
winget install Microsoft.DotNet.SDK.10 nodejs Docker.DockerDesktop PostgreSQL
```

### Linux (Ubuntu/Debian)

```bash
# Update packages
sudo apt update && sudo apt upgrade -y

# Install .NET SDK
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh

# Install Node.js
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
sudo apt install nodejs

# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Install PostgreSQL
sudo apt install postgresql postgresql-contrib

# Install Keycloak
sudo apt install openjdk-21-jdk
wget https://github.com/keycloak/keycloak/releases/download/24.0.0/keycloak-24.0.0.tar.gz
tar xzf keycloak-24.0.0.tar.gz
```

## Local Development Environment

### 1. Clone Repository

```bash
cd ~/Professional/Code
# Repository setup will be added after initial structure
```

### 2. Initialize Infrastructure with Docker Compose

```bash
cd FinSight
docker-compose up -d
```

This starts:
- **PostgreSQL**: 5432 (5 databases)
- **Keycloak**: 8080
- **Redis**: 6379 (Phase 2)
- **RabbitMQ**: 5672 (Phase 2)

### 3. Configure Keycloak

```bash
# Access Keycloak Admin Console
open http://localhost:8080

# Default credentials
Username: admin
Password: admin

# Create Realm: "finsight"
# Create Client: "finsight-ui"
# Configure OAuth2 settings
```

#### Keycloak Setup Script

See [keycloak-setup.sh](../docker/keycloak-setup.sh) for automated configuration.

```bash
cd docker
bash keycloak-setup.sh
```

### 4. Database Initialization

```bash
# Create databases
psql -h localhost -U postgres -c "CREATE DATABASE identitydb;"
psql -h localhost -U postgres -c "CREATE DATABASE transactiondb;"
psql -h localhost -U postgres -c "CREATE DATABASE portfoliodb;"
psql -h localhost -U postgres -c "CREATE DATABASE documentdb;"
psql -h localhost -U postgres -c "CREATE DATABASE aidb;"

# Run migrations
dotnet ef database update --project services/identity-service/IdentityService.Data
dotnet ef database update --project services/transaction-service/TransactionService.Data
# ... repeat for other services
```

### 5. Environment Configuration

Create `.env` files in each service:

**services/identity-service/.env**
```
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=https://localhost:5001;http://localhost:5001
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=identitydb;Username=postgres;Password=postgres
Keycloak__Authority=https://localhost:8080/realms/finsight
Keycloak__ClientId=finsight-api
Keycloak__ClientSecret=<your-secret>
```

**services/transaction-service/.env**
```
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://localhost:5002
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=transactiondb;Username=postgres;Password=postgres
Services__IdentityService=https://localhost:5001
```

Similar configuration for other services.

**frontend/.env**
```
VITE_API_URL=http://localhost:5000
VITE_IDENTITY_URL=http://localhost:8080
VITE_IDENTITY_REALM=finsight
VITE_IDENTITY_CLIENT_ID=finsight-ui
```

### 6. Build Solutions

```bash
# Identity Service
cd services/identity-service
dotnet build
dotnet run

# In another terminal - Transaction Service
cd services/transaction-service
dotnet build
dotnet run

# Similar for other services...
```

### 7. Run Frontend

```bash
cd frontend
npm install
npm start
```

Frontend will be available at: `http://localhost:4200`

## Development Workflow

### Creating a New Service

Use the service template:

```bash
cd services
mkdir my-new-service
cd my-new-service

# Create projects
dotnet new globaljson --sdk-version 10.0.0
dotnet new sln -n MyNewService

# Create class libraries
dotnet new classlib -n MyNewService.External.Contracts
dotnet new classlib -n MyNewService.Internal.Contracts
dotnet new classlib -n MyNewService.Business
dotnet new web -n MyNewService.Api
dotnet new xunit -n MyNewService.Tests

# Add to solution
dotnet sln add MyNewService.External.Contracts/MyNewService.External.Contracts.csproj
dotnet sln add MyNewService.Internal.Contracts/MyNewService.Internal.Contracts.csproj
dotnet sln add MyNewService.Business/MyNewService.Business.csproj
dotnet sln add MyNewService.Api/MyNewService.Api.csproj
dotnet sln add MyNewService.Tests/MyNewService.Tests.csproj

# Add references
dotnet add MyNewService.Internal.Contracts reference ../shared/FinSight.Contracts
dotnet add MyNewService.External.Contracts reference MyNewService.Internal.Contracts ../shared/FinSight.Contracts
dotnet add MyNewService.Business reference MyNewService.External.Contracts MyNewService.Internal.Contracts
dotnet add MyNewService.Api reference MyNewService.External.Contracts
dotnet add MyNewService.Tests reference MyNewService.Business MyNewService.Api

# Add gRPC/code-first contract packages
dotnet add ../shared/FinSight.Contracts package protobuf-net
dotnet add MyNewService.Internal.Contracts package protobuf-net
dotnet add MyNewService.External.Contracts package protobuf-net.Grpc
dotnet add MyNewService.External.Contracts package System.ServiceModel.Primitives --version 4.5.3
dotnet add MyNewService.Api package protobuf-net.Grpc.AspNetCore
```

### Testing

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverageFormat=cobertura

# Run specific test
dotnet test --filter "TestClass"
```

### Code Quality Checks

```bash
# Format code
dotnet format

# Analyze code
dotnet analyzers run

# Lint (Angular)
cd frontend && npm run lint
```

### Git Workflow

```bash
# Create feature branch
git checkout -b feature/add-transaction-api

# Make changes
git add .
git commit -m "feat: add transaction creation endpoint"

# Push to remote
git push origin feature/add-transaction-api

# Create Pull Request
# GitHub Actions will run:
# - Build verification
# - Unit tests
# - Integration tests
# - Code coverage
```

## Debugging

### Debugging .NET Services

**VS Code Debug Configuration** (`.vscode/launch.json`):
```json
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": "IdentityService",
            "type": "coreclr",
            "request": "launch",
            "program": "${workspaceFolder}/services/identity-service/IdentityService.Api/bin/Debug/net10.0/IdentityService.Api.dll",
            "args": [],
            "cwd": "${workspaceFolder}/services/identity-service/IdentityService.Api",
            "preLaunchTask": "build",
            "env": {
                "ASPNETCORE_ENVIRONMENT": "Development"
            }
        }
    ]
}
```

### Debugging Frontend

```bash
# Angular debug mode
ng serve --poll 2000 --host 0.0.0.0

# Debug in browser DevTools (F12)
```

### Debugging gRPC

```bash
# Using gRPCUI for inspection
grpcui -plaintext localhost:5002

# Enable gRPC logging
export GRPC_VERBOSITY=DEBUG
export GRPC_TRACE=all
```

## Troubleshooting

### Port Already in Use

```bash
# Find process using port
lsof -i :5001

# Kill process
kill -9 <PID>
```

### Database Connection Issues

```bash
# Test PostgreSQL connection
psql -h localhost -U postgres -d postgres -c "SELECT version();"

# Check if databases exist
psql -h localhost -U postgres -c "\l"
```

### Keycloak Not Starting

```bash
# Check Keycloak logs
docker logs keycloak

# Restart Keycloak
docker restart keycloak

# Verify Keycloak is accessible
curl -s http://localhost:8080/health | jq .
```

### Docker Issues

```bash
# Check Docker status
docker ps

# View Docker logs
docker logs <container-id>

# Rebuild containers
docker-compose build --no-cache

# Reset Docker (WARNING: clears data)
docker system prune -a
```

## Performance Tips

1. **Enable Hot Reload**: 
   ```bash
   dotnet watch run
   ```

2. **Database Indexing**: Create indexes on frequently queried fields

3. **gRPC Connection Pooling**: Reuse gRPC channels

4. **Angular OnPush Detection**: Use OnPush change detection strategy

## Next Steps

1. Review [ARCHITECTURE.md](./ARCHITECTURE.md) for system design
2. Check [SERVICE_DESIGN.md](./SERVICE_DESIGN.md) for microservice patterns
3. Study [API_CONTRACTS.md](./API_CONTRACTS.md) for contract definitions
4. Follow code samples in `/docs/examples/`

---

**Status**: Setup guide for Phase 1
**Last Updated**: May 2026
