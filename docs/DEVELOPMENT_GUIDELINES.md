# Development Guidelines & Best Practices

## Code Style

### C# Conventions

Follow Microsoft C# Coding Conventions with these additional guidelines:

1. **Naming**
   - Use PascalCase for class names: `TransactionService`
   - Use camelCase for local variables: `userContext`
   - Use UPPERCASE for constants: `MAX_TRANSACTION_AMOUNT`
   - Use I prefix for interfaces: `ITransactionRepository`

2. **Async/Await**
   - Always use `async`/`await` for I/O operations
   - Methods that are `async` should end with `Async`: `GetTransactionAsync()`
   - Avoid `.Result` and `.Wait()` - can cause deadlocks

3. **LINQ**
   - Use method syntax over query syntax
   - Defer database queries: `.AsNoTracking()` for read-only

4. **Error Handling**
   - Use specific exceptions, not base `Exception`
   - Log before throwing
   - Use correlation IDs for tracing

### TypeScript/Angular Conventions

1. **File Organization**
   - One component per file
   - Group related components in folders
   - Use `*.component.ts`, `*.service.ts`, `*.model.ts` naming

2. **Component Structure**
   ```typescript
   @Component({
     selector: 'app-transaction-list',
     templateUrl: './transaction-list.component.html',
     styleUrls: ['./transaction-list.component.css']
   })
   export class TransactionListComponent implements OnInit, OnDestroy {
     // 1. @Input/@Output properties
     // 2. Public properties
     // 3. Private properties
     // 4. Constructor
     // 5. Lifecycle hooks
     // 6. Public methods
     // 7. Private methods
   }
   ```

3. **Change Detection**
   - Use `OnPush` strategy for performance
   - Manually trigger detection only when needed

## Git Workflow

### Branch Naming

```
feature/short-description      # New features
bugfix/issue-description        # Bug fixes
docs/what-you-documented        # Documentation
refactor/what-changed           # Code improvements
test/what-you-tested            # Test additions
```

### Commit Messages

Follow Conventional Commits:

```
feat: add transaction filtering by category
fix: correct portfolio calculation logic
docs: update API documentation
test: add integration tests for transaction service
refactor: extract common validation logic
chore: update dependencies
```

### Pull Request Process

1. Create feature branch from `develop`
2. Make focused, atomic commits
3. Push to remote
4. Create PR with clear description
5. Ensure CI/CD passes
6. Request code review (2+ reviewers)
7. Squash and merge

## Testing Strategy

### Test Pyramid

```
      /\          End-to-End Tests (10%)
     /  \
    /    \       Integration Tests (30%)
   /      \
  /        \     Unit Tests (60%)
 /          \
```

### Unit Tests

- Test business logic in isolation
- Mock external dependencies
- Aim for >80% coverage

```csharp
[Fact]
public async Task CreateTransaction_WithValidRequest_ReturnsSuccess()
{
    // Arrange
    var request = new CreateTransactionRequest { /* ... */ };
    var engine = new CreateTransactionEngine(mockRepository);
    
    // Act
    var result = await engine.CreateAsync(request, userContext);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal("desc", result.Description);
    mockRepository.Verify(r => r.AddAsync(It.IsAny<Transaction>()), Times.Once);
}
```

### Integration Tests

- Test repository layer with real database
- Use TestContainers for PostgreSQL
- Test cross-service communication

### E2E Tests

- Test complete user workflows
- Use Angular testing utilities
- Test API Gateway routing

## Logging

### Structured Logging with Serilog

```csharp
_logger.LogInformation(
    "Transaction created: {TransactionId} for user {UserId} in category {Category}",
    transactionId, userId, category);

_logger.LogWarning(
    "Duplicate transaction detected for user {UserId}: {Description}",
    userId, description);

_logger.LogError(ex,
    "Failed to process transaction for user {UserId}",
    userId);
```

### Log Levels

- **Debug**: Detailed diagnostic information (dev only)
- **Information**: General informational messages
- **Warning**: Warning conditions
- **Error**: Error events
- **Fatal**: Critical errors

## Security Considerations

### Input Validation

1. Validate at API boundary (Manager)
2. Validate data types and ranges
3. Prevent SQL injection with parameterized queries
4. Sanitize user input

### Authentication & Authorization

1. Always use HTTPS in production
2. Validate JWT tokens
3. Check authorization before operations
4. Use role-based access control

### Secrets Management

```csharp
// Use configuration, not hardcoded strings
var apiKey = configuration["Services:ExternalApi:Key"];

// Use environment variables in Docker
ENV Services__ExternalApi__Key=actual-key-here
```

### API Security

- Rate limiting on gateway
- CORS properly configured
- Request size limits
- No sensitive data in logs

## Performance Optimization

### Database

1. **Indexing**: Create indexes on foreign keys and frequently queried fields
2. **Queries**: Use `.AsNoTracking()` for read-only queries
3. **Pagination**: Always limit result sets
4. **Connection Pooling**: Reuse connections

### gRPC

1. **Connection Reuse**: Share channels across requests
2. **Message Size**: Keep protobuf messages small
3. **Streaming**: Use server/client streaming for large datasets

### Frontend

1. **OnPush Detection**: Use change detection strategy
2. **Lazy Loading**: Lazy load feature modules
3. **Bundling**: Analyze and optimize bundles
4. **Caching**: Cache HTTP responses appropriately

## Documentation

### Code Documentation

Use XML documentation for public APIs:

```csharp
/// <summary>
/// Creates a new transaction for the user.
/// </summary>
/// <param name="request">Transaction creation details</param>
/// <param name="userContext">Current user context</param>
/// <returns>Created transaction response</returns>
/// <exception cref="ValidationException">If request is invalid</exception>
/// <exception cref="UnauthorizedException">If user lacks permissions</exception>
public async Task<CreateTransactionResponse> ExecuteAsync(
    CreateTransactionRequest request,
    UserContext userContext)
{
    // ...
}
```

### API Documentation

Use Swagger/OpenAPI for REST endpoints:

```csharp
/// <summary>
/// Create a new transaction
/// </summary>
/// <response code="200">Transaction created successfully</response>
/// <response code="400">Invalid request</response>
/// <response code="401">Unauthorized</response>
[HttpPost]
[Authorize(Roles = "Customer")]
public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
{
    // ...
}
```

## Code Review Checklist

- [ ] Code follows naming conventions
- [ ] No hardcoded values (use configuration)
- [ ] Proper error handling
- [ ] No N+1 queries
- [ ] Tests added/updated
- [ ] Documentation updated
- [ ] No security vulnerabilities
- [ ] Performance acceptable
- [ ] Follows SOLID principles

## Dependency Updates

```bash
# Check for outdated packages
dotnet outdated

# Update packages
dotnet add package SomePackage

# Update NuGet packages
dotnet nuget update source

# Angular dependencies
npm outdated
npm update
```

## Common Pitfalls to Avoid

1. **Blocking Calls**: Don't use `.Result` or `.Wait()`
2. **N+1 Queries**: Use `.Include()` in Entity Framework
3. **Hardcoded Values**: Use configuration
4. **Bare Exceptions**: Catch specific exceptions
5. **Shared Mutable State**: Use immutable patterns
6. **Missing Null Checks**: Use nullable reference types
7. **Inadequate Testing**: Aim for >80% coverage
8. **Poor Error Messages**: Make errors actionable
9. **Ignoring Warnings**: Treat warnings as errors
10. **Skipping Documentation**: Document as you code

---

**Last Updated**: May 2026
