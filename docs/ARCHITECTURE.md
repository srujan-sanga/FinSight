# FinSight AI - Architecture Design

## System Architecture Overview

```
┌─────────────────┐
│  Angular UI     │
│  (Frontend)     │
└────────┬────────┘
         │ REST API
         │ OAuth2 Token
         │
    ┌────▼────────────────┐
    │   API Gateway       │
    │ ● Route requests    │
    │ ● Auth enforcement  │
    │ ● Rate limiting     │
    └────┬────────────────┘
         │ gRPC
    ┌────┴──────────────────────────────────┐
    │                                        │
┌───▼────────────┐  ┌──────────────────┐   │
│ Identity       │  │ Transaction      │   │
│ Service        │  │ Service          │   │
│                │  │                  │   │
│ ● Auth         │  │ ● Transactions   │   │
│ ● Users        │  │ ● Categories     │   │
│ ● Roles        │  │ ● Recurring      │   │
│ ● Permissions  │  │                  │   │
│                │  │                  │   │
│ Port: 5001     │  │ Port: 5002       │   │
│ identitydb     │  │ transactiondb    │   │
└────────────────┘  └──────────────────┘   │
    │                    │                  │
┌───▼────────────┐  ┌────▼──────────────┐  │
│ Portfolio      │  │ Document          │  │
│ Service        │  │ Service           │  │
│                │  │                   │  │
│ ● Investments  │  │ ● Upload Docs     │  │
│ ● Stocks       │  │ ● Metadata        │  │
│ ● Holdings     │  │ ● Search          │  │
│ ● Performance  │  │ ● RAG Pipeline    │  │
│                │  │                   │  │
│ Port: 5003     │  │ Port: 5004        │  │
│ portfoliodb    │  │ documentdb        │  │
└────────────────┘  └───────────────────┘  │
    │                     │                 │
    │  ┌──────────────────▼──────────────┐ │
    │  │      AI Service                  │ │
    │  │                                  │ │
    │  │ ● Financial Analysis             │ │
    │  │ ● RAG (Retrieval-Augmented Gen)  │ │
    │  │ ● Chat Interface                 │ │
    │  │ ● Recommendations                │ │
    │  │ ● Summaries & Insights           │ │
    │  │ ● MCP Tool Integration           │ │
    │  │                                  │ │
    │  │ Port: 5005                       │ │
    │  │ aidb                             │ │
    │  └──────────────────────────────────┘ │
    │                                        │
    └────────────────────────────────────────┘
                     │ gRPC
        ┌────────────┴────────────┐
        │                         │
    ┌───▼─────────────┐   ┌──────▼──────────┐
    │  PostgreSQL     │   │  Keycloak       │
    │  (5 instances)  │   │  (Auth Server)  │
    │                 │   │                 │
    │ ● identitydb    │   │ OIDC/OAuth2     │
    │ ● transactiondb │   │ Provider        │
    │ ● portfoliodb   │   │                 │
    │ ● documentdb    │   │ Port: 8080      │
    │ ● aidb          │   └─────────────────┘
    │                 │
    │ Port: 5432      │
    └─────────────────┘

Phase 2 (Optional):
    ┌──────────────────┐  ┌──────────────────┐
    │    Redis         │  │    RabbitMQ      │
    │   Caching        │  │  Event Bus       │
    │ Port: 6379       │  │ Port: 5672       │
    └──────────────────┘  └──────────────────┘
```

## Design Patterns

### 1. Manager/Engine Pattern

**Purpose**: Clean separation between contract validation/orchestration and business logic.

```
HTTP/gRPC Request
    ↓
┌─────────────────────────────────────────┐
│  gRPC Contract Host                     │
│  - Expose external manager contracts    │
│  - No Business project reference        │
└────────────────┬────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────┐
│  Manager (Validation & Orchestration)   │
│  - Validate input contracts             │
│  - Check authorization                  │
│  - Orchestrate multiple engines         │
│  - Transform response contracts         │
│  - Correlation ID handling              │
└────────────────┬────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────┐
│  Engine (Business Logic)                │
│  - Domain logic                         │
│  - Business rules                       │
│  - State changes                        │
│  - Complex calculations                 │
└────────────────┬────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────┐
│  DatabaseRA (Data Access)               │
│  - Database queries                     │
│  - Entity persistence                   │
│  - Data aggregation                     │
└─────────────────────────────────────────┘
```

**Example: CreateTransactionManager**
```csharp
public class CreateTransactionManager : ICreateTransactionManager
{
    private readonly ICreateTransactionEngine _engine;
    private readonly ITransactionValidator _validator;
    
    public async Task<CreateTransactionResponse> ExecuteAsync(
        CreateTransactionRequest request, 
        UserContext context)
    {
        // Validation
        _validator.ValidateRequest(request);
        
        // Authorization
        if (!context.HasPermission("transaction:create"))
            throw new UnauthorizedException();
        
        // Business Logic (delegated to Engine)
        var transaction = await _engine.CreateAsync(request, context);
        
        // Response transformation
        return new CreateTransactionResponse
        {
            Id = transaction.Id,
            Status = transaction.Status
        };
    }
}
```

### 2. Contract-First Design

All DTOs inherit from base types for consistency:

```csharp
// Base request
public abstract class MessageRequest
{
    public string CorrelationId { get; set; }
    public long Timestamp { get; set; }
    public string RequestId { get; set; }
}

// Base response
public abstract class MessageResponse
{
    public string CorrelationId { get; set; }
    public long Timestamp { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; }
}

// Specific request
public class CreateTransactionRequest : MessageRequest
{
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public string Category { get; set; }
    public DateTime TransactionDate { get; set; }
}

// Specific response
public class CreateTransactionResponse : MessageResponse
{
    public string TransactionId { get; set; }
    public TransactionStatus Status { get; set; }
}
```

### 3. gRPC Service Definition

```protobuf
syntax = "proto3";

service TransactionService {
    rpc CreateTransaction(CreateTransactionRequest) returns (CreateTransactionResponse);
    rpc GetTransaction(GetTransactionRequest) returns (GetTransactionResponse);
    rpc ListTransactions(ListTransactionsRequest) returns (ListTransactionsResponse);
    rpc UpdateTransaction(UpdateTransactionRequest) returns (UpdateTransactionResponse);
    rpc DeleteTransaction(DeleteTransactionRequest) returns (DeleteTransactionResponse);
}

message CreateTransactionRequest {
    string description = 1;
    string amount = 2;
    string category = 3;
    string transaction_date = 4;
}

message CreateTransactionResponse {
    bool success = 1;
    string transaction_id = 2;
    string status = 3;
}
```

### 4. gRPC Interceptors

Interceptors execute **before** any Manager invocation:

```csharp
public class CorrelationIdInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var correlationId = context.RequestHeaders
            .FirstOrDefault(h => h.Key == "correlation-id")?.Value 
            ?? Guid.NewGuid().ToString();
            
        using var scope = new CorrelationIdScope(correlationId);
        return await continuation(request, context);
    }
}

public class ExceptionHandlingInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (UnauthorizedException ex)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, ex.Message));
        }
        catch (ValidationException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            throw new RpcException(new Status(StatusCode.Internal, "Internal error"));
        }
    }
}
```

### 5. Service-to-Service Communication (gRPC Only)

**Example: Transaction Service calling Portfolio Service**

```csharp
public class TransactionEngine : ITransactionEngine
{
    private readonly PortfolioService.PortfolioServiceClient _portfolioClient;
    private readonly ITransactionRa _ra;
    
    public async Task<Transaction> CreateAsync(
        CreateTransactionRequest request, 
        UserContext context)
    {
        // Verify user exists in Portfolio Service
        var portfolioRequest = new GetUserPortfolioRequest { UserId = context.UserId };
        var portfolioResponse = await _portfolioClient.GetUserPortfolioAsync(portfolioRequest);
        
        if (!portfolioResponse.Success)
            throw new InvalidOperationException("User portfolio not found");
        
        // Create transaction
        var transaction = new Transaction
        {
            UserId = context.UserId,
            Description = request.Description,
            Amount = request.Amount,
            Category = request.Category,
            TransactionDate = request.TransactionDate
        };
        
        await _ra.AddAsync(transaction);
        return transaction;
    }
}
```

### 6. Database-per-Service Pattern

```
TransactionService owns transactiondb
- transactions table
- categories table
- recurring_transactions table
- transaction_items table

PortfolioService owns portfoliodb
- holdings table
- investments table
- performance_metrics table

No cross-database queries.
All cross-service queries via gRPC.
```

### 7. Authentication Flow (OIDC/OAuth2)

```
┌──────────────┐
│ User Browser │
└────────┬─────┘
         │ 1. Login request
         ▼
    ┌─────────────────┐
    │  API Gateway    │
    │  - Redirect to  │
    │    Keycloak     │
    └────────┬────────┘
             │
             ▼
    ┌─────────────────┐
    │  Keycloak       │
    │  - Authenticate │
    │  - Issue tokens │
    └────────┬────────┘
             │ Returns access token
             │ + refresh token
             ▼
    ┌─────────────────┐
    │  API Gateway    │
    │  - Validates    │
    │    JWT token    │
    │  - Extracts     │
    │    claims       │
    │  - Sets context │
    └────────┬────────┘
             │
             ▼
    ┌─────────────────┐
    │  Service        │
    │  - Receives     │
    │    user context │
    │  - Enforces     │
    │    RBAC/CBAC    │
    └─────────────────┘
```

## Data Storage Strategy

### PostgreSQL (Per-Service)

**IdentityDB**
```sql
users (id, email, name, created_at, updated_at)
roles (id, name, description)
user_roles (user_id, role_id)
permissions (id, name, resource, action)
role_permissions (role_id, permission_id)
```

**TransactionDB**
```sql
transactions (id, user_id, amount, description, category, date)
transaction_items (id, transaction_id, item_name, amount)
categories (id, user_id, name, type)
recurring_transactions (id, user_id, amount, frequency, category)
```

**PortfolioDB**
```sql
holdings (id, user_id, symbol, shares, purchase_price, purchase_date)
investments (id, user_id, name, type, value, allocation_percentage)
performance_metrics (id, holdings_id, date, value, gain_loss)
```

**DocumentDB**
```sql
documents (id, user_id, file_name, file_size, upload_date, document_type)
document_metadata (id, document_id, key, value)
chunks (id, document_id, chunk_index, content, embedding_vector)
```

**AIDB**
```sql
conversations (id, user_id, created_at, updated_at)
messages (id, conversation_id, role, content, timestamp)
rag_index (id, source_type, source_id, embedding_vector, metadata)
recommendations (id, user_id, recommendation_type, content, created_at)
```

## API Gateway Pattern

**Routes** (REST → gRPC):
```
POST /api/auth/login → IdentityService.Login
POST /api/transactions → TransactionService.CreateTransaction
GET /api/transactions/:id → TransactionService.GetTransaction
GET /api/portfolio → PortfolioService.GetPortfolio
POST /api/documents/upload → DocumentService.UploadDocument
POST /api/ai/chat → AIService.Chat
```

**Responsibilities**:
- Request routing to services
- Authentication enforcement
- Token validation
- Rate limiting
- Request/response logging
- Error handling
- CORS

## Security Layers

1. **Transport Security**: gRPC over TLS
2. **Authentication**: OIDC/OAuth2 with Keycloak
3. **Authorization**: RBAC (Role-Based Access Control)
4. **Claims-Based Authorization**: Fine-grained permissions
5. **Input Validation**: In Manager layer
6. **SQL Injection Prevention**: Parameterized queries
7. **Correlation ID Tracking**: For audit trails

## Deployment Strategy

### Development (Docker Compose)
```yaml
services:
  postgres:
    image: postgres:16
    
  keycloak:
    image: quay.io/keycloak/keycloak:latest
    
  gateway:
    build: ./gateway
    
  identity-service:
    build: ./services/identity-service
    
  # ... other services
```

### Production (Kubernetes Ready)
- Each service in separate pod
- Persistent volumes for databases
- ConfigMaps for configuration
- Secrets for credentials
- Ingress for API Gateway
- HPA for scaling

---

See related documentation:
- [SERVICE_DESIGN.md](./SERVICE_DESIGN.md) - Individual service architecture
- [API_CONTRACTS.md](./API_CONTRACTS.md) - Contract specifications
- [DEPLOYMENT.md](./DEPLOYMENT.md) - Deployment procedures
