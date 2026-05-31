# FinSight AI - Microservice Design Patterns

## Service Structure Template

Every microservice follows this layered architecture:

```
MyService.sln
├── MyService.Contracts/
│   ├── Requests/
│   │   ├── CreateMyEntityRequest.cs
│   │   ├── UpdateMyEntityRequest.cs
│   │   └── GetMyEntityRequest.cs
│   ├── Responses/
│   │   ├── CreateMyEntityResponse.cs
│   │   ├── GetMyEntityResponse.cs
│   │   └── ListMyEntityResponse.cs
│   └── MyService.Contracts.csproj
│
├── MyService.Business/
│   ├── Engines/
│   │   ├── ICreateMyEntityEngine.cs
│   │   ├── CreateMyEntityEngine.cs
│   │   └── ...
│   ├── Managers/
│   │   ├── ICreateMyEntityManager.cs
│   │   ├── CreateMyEntityManager.cs
│   │   └── ...
│   ├── Validators/
│   │   ├── ICreateMyEntityValidator.cs
│   │   └── CreateMyEntityValidator.cs
│   ├── Domain/
│   │   ├── MyEntity.cs
│   │   └── ValueObjects/
│   └── MyService.Business.csproj
│
├── MyService.Data/
│   ├── Migrations/
│   │   ├── 001_InitialCreate.cs
│   │   └── 002_AddIndexes.cs
│   ├── Context/
│   │   └── MyServiceDbContext.cs
│   ├── Repositories/
│   │   ├── IMyEntityRepository.cs
│   │   └── MyEntityRepository.cs
│   ├── Configurations/
│   │   └── MyEntityConfiguration.cs
│   └── MyService.Data.csproj
│
├── MyService.Api/
│   ├── grpc/
│   │   ├── Protos/
│   │   │   └── myservice.proto
│   │   └── Services/
│   │       └── MyServiceImpl.cs
│   ├── Controllers/
│   │   └── HealthController.cs
│   ├── Interceptors/
│   │   ├── ExceptionHandlingInterceptor.cs
│   │   ├── CorrelationIdInterceptor.cs
│   │   ├── AuthenticationInterceptor.cs
│   │   └── LoggingInterceptor.cs
│   ├── Middleware/
│   │   └── ErrorHandlingMiddleware.cs
│   ├── Startup/
│   │   ├── Program.cs
│   │   └── StartupConfiguration.cs
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
    │   ├── Repositories/
    │   │   └── MyEntityRepositoryTests.cs
    │   └── Services/
    │       └── MyServiceImplTests.cs
    ├── Fixtures/
    │   ├── DatabaseFixture.cs
    │   └── RepositoryFixture.cs
    └── MyService.Tests.csproj
```

## Layer Responsibilities

### Contracts Layer (`MyService.Contracts`)

**Purpose**: Define external API contracts

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

**Engines** - Pure business logic:

```csharp
public interface ICreateTransactionEngine
{
    Task<Transaction> CreateAsync(
        CreateTransactionRequest request,
        UserContext userContext);
}

public class CreateTransactionEngine : ICreateTransactionEngine
{
    private readonly ITransactionRepository _repository;
    private readonly ILogger<CreateTransactionEngine> _logger;
    
    public async Task<Transaction> CreateAsync(
        CreateTransactionRequest request,
        UserContext userContext)
    {
        // Business logic only - no validation, auth, or orchestration
        var transaction = new Transaction
        {
            UserId = userContext.UserId,
            Description = request.Description,
            Amount = request.Amount,
            Category = request.Category,
            TransactionDate = request.TransactionDate,
            CreatedAt = DateTime.UtcNow
        };
        
        await _repository.AddAsync(transaction);
        _logger.LogInformation(
            "Transaction created: {TransactionId} for user {UserId}",
            transaction.Id, userContext.UserId);
            
        return transaction;
    }
}
```

**Managers** - Orchestration & validation:

```csharp
public interface ICreateTransactionManager
{
    Task<CreateTransactionResponse> ExecuteAsync(
        CreateTransactionRequest request,
        UserContext userContext);
}

public class CreateTransactionManager : ICreateTransactionManager
{
    private readonly ICreateTransactionEngine _engine;
    private readonly ICreateTransactionValidator _validator;
    private readonly ILogger<CreateTransactionManager> _logger;
    
    public async Task<CreateTransactionResponse> ExecuteAsync(
        CreateTransactionRequest request,
        UserContext userContext)
    {
        try
        {
            // 1. Validate
            var validationErrors = _validator.Validate(request);
            if (validationErrors.Any())
                throw new ValidationException(validationErrors);
            
            // 2. Authorize
            if (!userContext.HasPermission("transaction:create"))
                throw new UnauthorizedException("Insufficient permissions");
            
            // 3. Execute business logic
            var transaction = await _engine.CreateAsync(request, userContext);
            
            // 4. Return response
            return new CreateTransactionResponse
            {
                Success = true,
                TransactionId = transaction.Id,
                Status = TransactionStatus.Pending,
                CorrelationId = request.CorrelationId
            };
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed: {Errors}", ex.Errors);
            return new CreateTransactionResponse
            {
                Success = false,
                Message = "Validation failed",
                CorrelationId = request.CorrelationId
            };
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning("Authorization failed: {Message}", ex.Message);
            throw;
        }
    }
}
```

### Data Layer (`MyService.Data`)

**Entity Configuration**:

```csharp
public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(t => t.Amount)
            .HasPrecision(18, 2);
        
        builder.HasIndex(t => new { t.UserId, t.TransactionDate });
        builder.HasIndex(t => t.Category);
    }
}
```

**Repository Pattern**:

```csharp
public interface ITransactionRepository
{
    Task<Transaction> GetByIdAsync(string id);
    Task<IEnumerable<Transaction>> GetByUserIdAsync(string userId);
    Task AddAsync(Transaction entity);
    Task UpdateAsync(Transaction entity);
    Task DeleteAsync(string id);
}

public class TransactionRepository : ITransactionRepository
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

**gRPC Service Implementation**:

```csharp
public class TransactionServiceImpl : TransactionService.TransactionServiceBase
{
    private readonly ICreateTransactionManager _createManager;
    private readonly ILogger<TransactionServiceImpl> _logger;
    
    public override async Task<CreateTransactionResponse> CreateTransaction(
        CreateTransactionRequest request,
        ServerCallContext context)
    {
        var userContext = context.GetUserContext();
        var response = await _createManager.ExecuteAsync(request, userContext);
        
        return response;
    }
}
```

**Program.cs Configuration**:

```csharp
public static class StartupConfiguration
{
    public static WebApplicationBuilder ConfigureServices(
        this WebApplicationBuilder builder)
    {
        // Database
        builder.Services.AddDbContext<MyServiceDbContext>(options =>
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("DefaultConnection")));
        
        // Repositories
        builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
        
        // Business Logic
        builder.Services.AddScoped<ICreateTransactionEngine, CreateTransactionEngine>();
        builder.Services.AddScoped<ICreateTransactionManager, CreateTransactionManager>();
        
        // gRPC
        builder.Services.AddGrpc(options =>
        {
            options.Interceptors.Add<CorrelationIdInterceptor>();
            options.Interceptors.Add<ExceptionHandlingInterceptor>();
            options.Interceptors.Add<AuthenticationInterceptor>();
        });
        
        // Authentication
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = builder.Configuration["Keycloak:Authority"];
                options.Audience = builder.Configuration["Keycloak:Audience"];
            });
        
        return builder;
    }
    
    public static WebApplication ConfigurePipeline(
        this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapGrpcService<TransactionServiceImpl>();
        
        return app;
    }
}
```

## Testing Strategy

### Unit Tests (Business Logic)

```csharp
public class CreateTransactionEngineTests
{
    private readonly Mock<ITransactionRepository> _repositoryMock;
    private readonly CreateTransactionEngine _engine;
    
    public CreateTransactionEngineTests()
    {
        _repositoryMock = new Mock<ITransactionRepository>();
        _engine = new CreateTransactionEngine(_repositoryMock.Object, LoggerFactory.CreateLogger());
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
        
        var userContext = new UserContext { UserId = "user123" };
        
        // Act
        var result = await _engine.CreateAsync(request, userContext);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Grocery", result.Description);
        Assert.Equal(50.00m, result.Amount);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Transaction>()), Times.Once);
    }
}
```

### Integration Tests (Repositories)

```csharp
public class TransactionRepositoryTests : IAsyncLifetime
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
        var repository = new TransactionRepository(_context);
        var transaction = new Transaction
        {
            UserId = "user123",
            Description = "Test",
            Amount = 100m
        };
        
        // Act
        await repository.AddAsync(transaction);
        var result = await repository.GetByIdAsync(transaction.Id);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.Description);
    }
}
```

## Dependency Injection

Register services in `Program.cs`:

```csharp
builder.Services
    .AddScoped<ITransactionRepository, TransactionRepository>()
    .AddScoped<ITransactionValidator, TransactionValidator>()
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
