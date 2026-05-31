# FinSight AI - Enterprise Financial Intelligence Platform

![Architecture](./docs/architecture.md)

## Overview

**FinSight AI** is a production-grade enterprise financial intelligence platform built to demonstrate modern software architecture, distributed systems, AI integration, and cloud-native development practices.

This portfolio project showcases skills expected from senior backend engineers, solution architects, and AI platform developers.

## Business Capabilities

Users can:
- Track personal and business finances
- Manage transactions with categorization
- Manage investments and portfolios
- Upload financial documents (PDFs, statements, reports)
- Ask natural language questions about financial data
- Receive AI-generated insights and recommendations
- Search across financial documents
- Generate summaries and financial analysis

### AI-Powered Features

- "Why did my expenses increase this month?"
- "What subscriptions am I paying for?"
- "Summarize my investment performance."
- "What was my highest spending category last quarter?"

## Technology Stack

### Backend
- **Framework**: ASP.NET Core 10+
- **Architecture**: Microservices with gRPC
- **Database**: PostgreSQL (per-service)
- **Communication**: gRPC (service-to-service), REST (external)
- **Authentication**: OIDC/OAuth2 with Keycloak
- **Messaging**: RabbitMQ (Phase 2)
- **Caching**: Redis (Phase 2)
- **AI/ML**: LLM integration with RAG

### Frontend
- **Framework**: Angular 18+
- **Styling**: TailwindCSS
- **State Management**: NgRx (optional)
- **Authentication**: OAuth2 Integration

### Infrastructure
- **Containerization**: Docker
- **Orchestration**: Docker Compose (dev), Kubernetes (prod-ready)
- **CI/CD**: GitHub Actions
- **Observability**: OpenTelemetry

## Project Structure

```
FinSight/
├── services/                    # Microservices
│   ├── identity-service/        # Authentication & Authorization
│   ├── transaction-service/     # Financial Transactions
│   ├── portfolio-service/       # Investment Management
│   ├── document-service/        # Document Management & Storage
│   └── ai-service/              # AI/RAG & Intelligence
├── gateway/                     # API Gateway
├── frontend/                    # Angular Application
├── shared/                      # Common packages & contracts
│   ├── FinSight.Contracts/      # Base contracts
│   ├── FinSight.Common/         # Utilities
│   └── FinSight.Testing/        # Testing utilities
├── docker/                      # Docker configurations
├── .github/workflows/           # CI/CD pipelines
└── docs/                        # Documentation
```

## Microservices Architecture

### 1. Identity Service
- **Port**: 5001
- **DB**: identitydb
- **Responsibilities**: Authentication, Authorization, User Management, Roles, Permissions
- **Tech**: Keycloak Provider, OIDC, OAuth2

### 2. Transaction Service
- **Port**: 5002
- **DB**: transactiondb
- **Responsibilities**: Expenses, Income, Transfers, Categories, Recurring Transactions

### 3. Portfolio Service
- **Port**: 5003
- **DB**: portfoliodb
- **Responsibilities**: Investments, Stocks, Mutual Funds, Holdings, Portfolio Performance

### 4. Document Service
- **Port**: 5004
- **DB**: documentdb
- **Responsibilities**: Document Upload, Storage, Metadata, Search

### 5. AI Service
- **Port**: 5005
- **DB**: aidb
- **Responsibilities**: Chat, Financial Analysis, RAG, Recommendations, Summaries

### API Gateway
- **Port**: 5000
- **Responsibilities**: Request routing, Authentication enforcement, Rate limiting, API aggregation

## Service Design Pattern

Each microservice follows the **Manager/Engine Pattern**:

```
Controller/gRPC Handler
    ↓
Manager (Validation, Security, Orchestration)
    ↓
Engine (Business Rules, Domain Logic)
    ↓
Repository (Data Access)
```

Every microservice includes:
- **[Service].Contracts** - External DTOs (inherit from MessageRequest/MessageResponse)
- **[Service].Business** - Business logic engines
- **[Service].Data** - Data access and repositories
- **[Service].Api** - gRPC handlers and managers
- **[Service].Tests** - Unit and integration tests

## Communication Patterns

### External → Internal
```
Angular Frontend
    ↓ (REST)
API Gateway
    ↓ (gRPC)
Microservices
```

### Service-to-Service
- **Only gRPC** - No REST between services
- Services communicate exclusively through gRPC
- Each service owns its data

## Authentication & Authorization

**OIDC/OAuth2** implementation:
- Access Tokens
- Refresh Tokens
- Role-Based Authorization (RBAC)
- Claims-Based Authorization (CBAC)

**Roles**:
- Admin
- Advisor
- Customer

## AI/RAG Pipeline

**Document Processing**:
```
Upload (PDF, Statement)
    ↓
Chunk & Tokenize
    ↓
Embedding Generation
    ↓
Vector Storage
    ↓
Retrieval on Query
    ↓
LLM Processing
```

**MCP Integration**: Expose tools for AI agents:
- GetTransactions
- GetPortfolio
- GetUserProfile
- SearchDocuments
- GetInvestmentPerformance

## Development Setup

### Prerequisites
- .NET 10+ SDK
- Node.js 20+
- Docker & Docker Compose
- PostgreSQL 16+
- Keycloak

### Quick Start

```bash
# Clone repository
git clone https://github.com/yourusername/finsight-ai.git
cd finsight-ai

# Start infrastructure
docker-compose up -d

# Run services
dotnet run --project services/identity-service/IdentityService.Api

# Run frontend
cd frontend && npm install && npm start
```

See [SETUP.md](./docs/SETUP.md) for detailed instructions.

## Design Principles

### Clean Architecture
- Clear separation of concerns
- Independent layers
- Testable components
- Domain-driven design

### SOLID Principles
- Single Responsibility
- Open/Closed
- Liskov Substitution
- Interface Segregation
- Dependency Inversion

### Enterprise Quality
- Structured logging
- OpenTelemetry instrumentation
- Comprehensive testing (Unit, Integration, E2E)
- Security-first design
- Strongly typed DTOs

## Implementation Phases

### Phase 1: Core Platform (Foundation)
- [x] Project structure
- [ ] Common contracts framework
- [ ] Identity Service (with Keycloak)
- [ ] Transaction Service
- [ ] Portfolio Service
- [ ] Document Service
- [ ] AI Service (basic RAG)
- [ ] API Gateway
- [ ] Angular Frontend
- [ ] Docker setup
- [ ] CI/CD pipeline

### Phase 2: Enhanced Features
- [ ] Redis caching
- [ ] RabbitMQ event-driven architecture
- [ ] Advanced AI features
- [ ] Analytics dashboard
- [ ] Performance optimization

### Phase 3: Production Ready
- [ ] Kubernetes manifests
- [ ] Advanced security (mTLS, service mesh)
- [ ] Distributed tracing
- [ ] Load testing & optimization
- [ ] Production monitoring

## Key Enterprise Patterns

### 1. gRPC Interceptors
- Correlation ID tracking
- Request/Response logging
- Exception handling
- User context resolution
- Tenant resolution (future)

### 2. Manager/Engine Pattern
- Manager: Validates, secures, orchestrates
- Engine: Implements business logic
- Clean separation of concerns

### 3. Service Contracts
- All DTOs inherit from base types
- Strong typing throughout
- Version compatibility

### 4. Database-per-Service
- Each service owns its database
- Cross-service queries via gRPC
- No shared databases

## Deployment

### Docker
```bash
# Build all services
docker-compose build

# Start entire stack
docker-compose up
```

### Kubernetes (Phase 3)
```bash
kubectl apply -f k8s/
```

## CI/CD Pipeline

**GitHub Actions**:
- Build & Test on PR
- Docker image build & push
- Integration testing
- Security scanning

See [.github/workflows](./.github/workflows) for pipeline definitions.

## Contributing

This is a personal portfolio project. Please see [ARCHITECTURE.md](./docs/ARCHITECTURE.md) for detailed design decisions.

## Documentation

- [ARCHITECTURE.md](./docs/ARCHITECTURE.md) - System design & patterns
- [SETUP.md](./docs/SETUP.md) - Development environment setup
- [SERVICE_DESIGN.md](./docs/SERVICE_DESIGN.md) - Microservice patterns
- [API_CONTRACTS.md](./docs/API_CONTRACTS.md) - Contract definitions
- [DEPLOYMENT.md](./docs/DEPLOYMENT.md) - Deployment guide

## License

This project is provided as-is for portfolio purposes.

---

**Status**: 🚀 In Development (Phase 1)

**Last Updated**: May 2026
