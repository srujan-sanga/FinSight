# FinSight AI - Common Contracts

This package contains shared contracts and base classes used across all microservices.

## Base Classes

### MessageRequest

Base class for all incoming requests:

```csharp
public abstract class MessageRequest
{
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    public string RequestId { get; set; } = Guid.NewGuid().ToString();
}
```

### MessageResponse

Base class for all responses:

```csharp
public abstract class MessageResponse
{
    public string CorrelationId { get; set; }
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    public bool Success { get; set; }
    public string Message { get; set; }
}
```

### ErrorResponse

Standard error response:

```csharp
public class ErrorResponse : MessageResponse
{
    public Dictionary<string, string[]> Errors { get; set; }
    public string StackTrace { get; set; } // Dev only
}
```

## Common Exceptions

- `ValidationException` - Request validation failed
- `UnauthorizedException` - User is not authenticated
- `ForbiddenException` - User lacks required permissions
- `NotFoundException` - Resource not found
- `ConflictException` - Resource conflict (e.g., duplicate)
- `ServiceException` - Service-to-service communication error

## User Context

```csharp
public class UserContext
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public IEnumerable<string> Roles { get; set; }
    public Dictionary<string, object> Claims { get; set; }
    
    public bool HasRole(string role) => Roles?.Contains(role) ?? false;
    public bool HasPermission(string permission) => Claims?.ContainsKey(permission) ?? false;
}
```

## Pagination

```csharp
public class PagedRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; }
    public bool SortDescending { get; set; }
}

public class PagedResponse<T>
{
    public List<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
}
```

## Extension Methods

Helper extensions for common operations:

```csharp
public static class ContractExtensions
{
    public static UserContext GetUserContext(this ServerCallContext context)
    {
        // Extract claims from JWT token in metadata
        // Return UserContext
    }
    
    public static void SetCorrelationId(this ServerCallContext context, string correlationId)
    {
        // Set correlation ID in response metadata
    }
}
```

See `FinSight.Contracts` project for full implementation.
