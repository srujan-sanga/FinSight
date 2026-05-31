# FinSight AI - Microservice Design Patterns

## Service Structure Template

Every microservice follows this layered architecture:

```
MyService.sln
├── MyService.External.Contracts/
│   ├── ServiceContracts/
│   │   └── IMyEntityManager.cs
│   └── MyService.External.Contracts.csproj
│
├── MyService.Internal.Contracts/
│   ├── ServiceContracts/
│   │   └── IMyEntityEngine.cs
│   ├── DataContracts/
│   │   ├── Req/
│   │   │   ├── CreateMyEntityRequest.cs
│   │   │   └── GetMyEntityRequest.cs
│   │   ├── Responses/
│   │   │   ├── CreateMyEntityResponse.cs
│   │   │   └── GetMyEntityResponse.cs
│   │   └── MyEntityDto.cs
│   └── MyService.Internal.Contracts.csproj
│
├── MyService.Business/
│   ├── Managers/
│   │   └── MyEntityManager.cs
│   ├── Engines/
│   │   └── MyEntityEngine.cs
│   ├── DatabaseRA/
│   │   └── MyEntityRa.cs
│   └── MyService.Business.csproj
│
├── MyService.Api/
│   ├── Program.cs
│   ├── Interceptors/
│   │   ├── ExceptionHandlingInterceptor.cs
│   │   ├── CorrelationIdInterceptor.cs
│   │   ├── AuthenticationInterceptor.cs
│   │   └── LoggingInterceptor.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── MyService.Api.csproj
│
└── MyService.Tests/
    ├── Unit/
    │   ├── Engines/
    │   │   └── CreateMyEntityEngineTests.cs
    │   ├── Managers/
    │   │   └── CreateMyEntityManagerTests.cs
    │   └── Validators/
    │       └── CreateMyEntityValidatorTests.cs
    ├── Integration/
    │   ├── DatabaseRA/
    │   │   └── MyEntityRaTests.cs
    │   └── Contracts/
    │       └── MyEntityManagerContractTests.cs
    ├── Fixtures/
    │   ├── DatabaseFixture.cs
    │   └── RaFixture.cs
    └── MyService.Tests.csproj
```

## Layer Responsibilities

### External Contracts Layer (`MyService.External.Contracts`)

**Purpose**: Define API-facing manager contracts. External applications reference this project, not Business.

```csharp
public interface ICreateTransactionManager : IExternalManager
{
    Task<CreateTransactionResponse> ExecuteAsync(
        CreateTransactionRequest request,
        CancellationToken cancellationToken = default);
}
```

### Internal Contracts Layer (`MyService.Internal.Contracts`)

**Purpose**: Define engine contracts and request/response data contracts.

```csharp
// Requests inherit from MessageRequest
public class CreateTransactionRequest : MessageRequest
{
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public string Category { get; set; }
    public DateTime TransactionDate { get; set; }
    
    // Validation
    public IEnumerable<string> Validate()
    {
        if (string.IsNullOrWhiteSpace(Description))
            yield return "Description is required";
        if (Amount <= 0)
            yield return "Amount must be greater than zero";
        if (TransactionDate > DateTime.UtcNow)
            yield return "Transaction date cannot be in the future";
    }
}

// Responses inherit from MessageResponse
public class CreateTransactionResponse : MessageResponse
{
    public string TransactionId { get; set; }
    public TransactionStatus Status { get; set; }
}
```

### Business Layer (`MyService.Business`)

Business references both External.Contracts and Internal.Contracts. API does not reference Business.

**Engines** - Pure business logic:

```csharp
public interface ICreateTransactionEngine : IEngine
{
    Task<CreateTransactionResponse> CreateAsync(
        CreateTransactionRequest request,
        CancellationToken cancellationToken = default);
}

public class CreateTransactionEngine : EngineBase, ICreateTransactionEngine
{
    private readonly ITransactionRa _ra;
    
    public async Task<CreateTransactionResponse> CreateAsync(
        CreateTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _ra.CreateAsync(request, cancellationToken);
    }
}
```

**Managers** - Orchestration & validation:

```csharp
public class CreateTransactionManager : ManagerBase, ICreateTransactionManager
{
    private readonly ICreateTransactionEngine _engine;
    
    public async Task<CreateTransactionResponse> ExecuteAsync(
        CreateTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Description))
            return Failure<CreateTransactionResponse>(request, "Description is required.");

        return await _engine.CreateAsync(request, cancellationToken);
    }
}
```

### DatabaseRA Layer (`MyService.Business/DatabaseRA`)

**Purpose**: Keep data access behind the engine inside Business. API never references DatabaseRA.

```csharp
public interface ITransactionRa : IDatabaseRA
{
    Task<Transaction> GetByIdAsync(string id);
    Task AddAsync(Transaction entity);
}

public class TransactionRa : RaBase, ITransactionRa
{
    private readonly MyServiceDbContext _context;
    
    public async Task<Transaction> GetByIdAsync(string id)
    {
        return await _context.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);
    }
    
    public async Task AddAsync(Transaction entity)
    {
        await _context.Transactions.AddAsync(entity);
        await _context.SaveChangesAsync();
    }
}
```

### API Layer (`MyService.Api`)

The API project references `MyService.External.Contracts` only. gRPC hosting is generated from external manager contracts once, so developers add methods to `IAbcManager` and implement them in `AbcManager`.

**Program.cs Configuration**:

```csharp
using MyService.Api.Grpc;
using MyService.External.Contracts.ServiceContracts;
using ProtoBuf.Grpc.Server;

var builder = WebApplication.CreateBuilder(args);
var businessAssembly = BusinessAssemblyLoader.Load("MyService.Business");
var grpcManagerServices = builder.Services.AddExternalManagerGrpcServices(
    typeof(ICreateTransactionManager).Assembly,
    businessAssembly);

builder.Services.AddCodeFirstGrpc();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapExternalManagerGrpcServices(grpcManagerServices);
app.MapHealthChecks("/health");

app.Run();
```

External manager contracts are code-first gRPC contracts:

```csharp
[ServiceContract(Name = "TransactionManager")]
public interface ICreateTransactionManager : IExternalManager
{
    [OperationContract]
    Task<CreateTransactionResponse> ExecuteAsync(
        CreateTransactionRequest request,
        CallContext context = default);
}
```

## Testing Strategy

### Unit Tests (Business Logic)

```csharp
public class CreateTransactionEngineTests
{
    private readonly Mock<ITransactionRa> _raMock;
    private readonly CreateTransactionEngine _engine;
    
    public CreateTransactionEngineTests()
    {
        _raMock = new Mock<ITransactionRa>();
        _engine = new CreateTransactionEngine(_raMock.Object);
    }
    
    [Fact]
    public async Task CreateAsync_WithValidRequest_ShouldCreateTransaction()
    {
        // Arrange
        var request = new CreateTransactionRequest
        {
            Description = "Grocery",
            Amount = 50.00m,
            Category = "Food",
            TransactionDate = DateTime.UtcNow
        };
        
        // Act
        var result = await _engine.CreateAsync(request);
        
        // Assert
        Assert.NotNull(result);
        _raMock.Verify(r => r.CreateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

### Integration Tests (DatabaseRA)

```csharp
public class TransactionRaTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;
    private MyServiceDbContext _context;
    
    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder().Build();
        await _container.StartAsync();
        
        var options = new DbContextOptionsBuilder<MyServiceDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;
            
        _context = new MyServiceDbContext(options);
        await _context.Database.MigrateAsync();
    }
    
    [Fact]
    public async Task AddAsync_WithValidTransaction_ShouldPersist()
    {
        // Arrange
        var ra = new TransactionRa(_context);
        var transaction = new Transaction
        {
            UserId = "user123",
            Description = "Test",
            Amount = 100m
        };
        
        // Act
        await ra.AddAsync(transaction);
        var result = await ra.GetByIdAsync(transaction.Id);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.Description);
    }
}
```

## Dependency Injection

Register Business services in the Business composition module, not directly in API:

```csharp
builder.Services
    .AddScoped<ITransactionRa, TransactionRa>()
    .AddScoped<ICreateTransactionEngine, CreateTransactionEngine>()
    .AddScoped<ICreateTransactionManager, CreateTransactionManager>();
```

## Error Handling

Consistent exception handling:

```csharp
public class ValidationException : Exception
{
    public IEnumerable<string> Errors { get; }
    
    public ValidationException(IEnumerable<string> errors)
        : base("Validation failed")
    {
        Errors = errors;
    }
}

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message)
        : base(message)
    {
    }
}

public class NotFoundException : Exception
{
    public NotFoundException(string resource, string id)
        : base($"{resource} with id {id} not found")
    {
    }
}
```

---

Follow this pattern for consistent, maintainable microservices.

See [ARCHITECTURE.md](./ARCHITECTURE.md) for system-wide patterns.
