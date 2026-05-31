# FinSight AI - API Contracts & Data Models

## Request/Response Pattern

All external API requests and responses follow a consistent contract pattern:

### Request Pattern

```csharp
public class GetTransactionRequest : MessageRequest
{
    public string TransactionId { get; set; }
}
```

### Response Pattern

```csharp
public class GetTransactionResponse : MessageResponse
{
    public TransactionDto Transaction { get; set; }
}
```

## Core Domain Models

### Transaction Model

```csharp
public class TransactionDto
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public string Category { get; set; }
    public DateTime TransactionDate { get; set; }
    public TransactionType Type { get; set; }  // Expense, Income, Transfer
    public TransactionStatus Status { get; set; }  // Pending, Completed, Failed
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum TransactionType
{
    Expense = 1,
    Income = 2,
    Transfer = 3
}

public enum TransactionStatus
{
    Pending = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4
}
```

### Portfolio Model

```csharp
public class PortfolioDto
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public decimal TotalValue { get; set; }
    public decimal TotalGainLoss { get; set; }
    public double GainLossPercentage { get; set; }
    public List<HoldingDto> Holdings { get; set; }
    public PortfolioPerformanceDto Performance { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class HoldingDto
{
    public string Id { get; set; }
    public string Symbol { get; set; }
    public string Name { get; set; }
    public decimal Shares { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal GainLoss { get; set; }
    public double GainLossPercentage { get; set; }
    public HoldingType Type { get; set; }  // Stock, MutualFund, ETF, Bond
}

public enum HoldingType
{
    Stock = 1,
    MutualFund = 2,
    ETF = 3,
    Bond = 4,
    Crypto = 5
}
```

### Document Model

```csharp
public class DocumentDto
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string FileName { get; set; }
    public long FileSizeBytes { get; set; }
    public string ContentType { get; set; }
    public DocumentType Type { get; set; }  // Statement, Report, Receipt
    public DateTime UploadDate { get; set; }
    public string StoragePath { get; set; }
    public DocumentStatus Status { get; set; }  // Uploaded, Processing, Indexed, Failed
    public Dictionary<string, string> Metadata { get; set; }
}

public enum DocumentType
{
    BankStatement = 1,
    InvestmentStatement = 2,
    TaxReport = 3,
    Receipt = 4,
    Invoice = 5,
    Other = 6
}

public enum DocumentStatus
{
    Uploaded = 1,
    Processing = 2,
    Indexed = 3,
    Failed = 4,
    Deleted = 5
}
```

### User Model

```csharp
public class UserDto
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public List<string> Roles { get; set; }
    public UserStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastLoginAt { get; set; }
}

public enum UserStatus
{
    Active = 1,
    Inactive = 2,
    Suspended = 3,
    Deleted = 4
}
```

## Service Contracts

### Transaction Service Contracts

```csharp
// Create Transaction
public class CreateTransactionRequest : MessageRequest
{
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public string Category { get; set; }
    public TransactionType Type { get; set; }
    public DateTime TransactionDate { get; set; }
}

public class CreateTransactionResponse : MessageResponse
{
    public string TransactionId { get; set; }
    public TransactionStatus Status { get; set; }
}

// Get Transaction
public class GetTransactionRequest : MessageRequest
{
    public string TransactionId { get; set; }
}

public class GetTransactionResponse : MessageResponse
{
    public TransactionDto Transaction { get; set; }
}

// List Transactions
public class ListTransactionsRequest : PagedRequest
{
    public string UserId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Category { get; set; }
    public TransactionType? Type { get; set; }
}

public class ListTransactionsResponse : MessageResponse
{
    public List<TransactionDto> Transactions { get; set; }
    public int TotalCount { get; set; }
}
```

### Portfolio Service Contracts

```csharp
// Get Portfolio
public class GetPortfolioRequest : MessageRequest
{
    public string UserId { get; set; }
}

public class GetPortfolioResponse : MessageResponse
{
    public PortfolioDto Portfolio { get; set; }
}

// Create Holding
public class CreateHoldingRequest : MessageRequest
{
    public string UserId { get; set; }
    public string Symbol { get; set; }
    public string Name { get; set; }
    public decimal Shares { get; set; }
    public decimal PurchasePrice { get; set; }
    public HoldingType Type { get; set; }
}

public class CreateHoldingResponse : MessageResponse
{
    public string HoldingId { get; set; }
}
```

### Document Service Contracts

```csharp
// Upload Document
public class UploadDocumentRequest : MessageRequest
{
    public string UserId { get; set; }
    public string FileName { get; set; }
    public byte[] FileContent { get; set; }
    public DocumentType DocumentType { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
}

public class UploadDocumentResponse : MessageResponse
{
    public string DocumentId { get; set; }
    public DocumentStatus Status { get; set; }
}

// Search Documents
public class SearchDocumentsRequest : PagedRequest
{
    public string UserId { get; set; }
    public string SearchQuery { get; set; }
    public DocumentType? DocumentType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class SearchDocumentsResponse : MessageResponse
{
    public List<DocumentDto> Documents { get; set; }
    public int TotalCount { get; set; }
}
```

### AI Service Contracts

```csharp
// Chat Message
public class ChatRequest : MessageRequest
{
    public string UserId { get; set; }
    public string ConversationId { get; set; }
    public string Message { get; set; }
    public List<string> DocumentIds { get; set; }  // Optional: specific docs to use
}

public class ChatResponse : MessageResponse
{
    public string ConversationId { get; set; }
    public string Reply { get; set; }
    public List<string> SourceDocuments { get; set; }
    public double ConfidenceScore { get; set; }
}

// Get Recommendations
public class GetRecommendationsRequest : MessageRequest
{
    public string UserId { get; set; }
    public string RecommendationType { get; set; }  // Spending, Investment, Savings
}

public class GetRecommendationsResponse : MessageResponse
{
    public List<RecommendationDto> Recommendations { get; set; }
}

public class RecommendationDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public double Confidence { get; set; }
    public string ActionUrl { get; set; }
}
```

## Error Responses

### Validation Error

```json
{
  "success": false,
  "message": "Validation failed",
  "correlationId": "550e8400-e29b-41d4-a716-446655440000",
  "timestamp": 1716019200000,
  "errors": {
    "Amount": ["Amount must be greater than zero"],
    "Category": ["Category is required"]
  }
}
```

### Authentication Error

```json
{
  "success": false,
  "message": "Invalid or expired token",
  "correlationId": "550e8400-e29b-41d4-a716-446655440000",
  "timestamp": 1716019200000
}
```

### Authorization Error

```json
{
  "success": false,
  "message": "Insufficient permissions to perform this action",
  "correlationId": "550e8400-e29b-41d4-a716-446655440000",
  "timestamp": 1716019200000
}
```

### Not Found Error

```json
{
  "success": false,
  "message": "Transaction with id 'abc123' not found",
  "correlationId": "550e8400-e29b-41d4-a716-446655440000",
  "timestamp": 1716019200000
}
```

### Server Error

```json
{
  "success": false,
  "message": "An internal server error occurred",
  "correlationId": "550e8400-e29b-41d4-a716-446655440000",
  "timestamp": 1716019200000,
  "stackTrace": "[development only]"
}
```

## API Versioning

Services will support API versioning via URL path or header:

### URL Path
```
GET /api/v1/transactions
GET /api/v2/transactions  (future)
```

### Header
```
Accept: application/vnd.finsight.v1+json
Accept: application/vnd.finsight.v2+json
```

## Rate Limiting

Rate limits enforced at API Gateway:

```
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 999
X-RateLimit-Reset: 1234567890
```

---

See service-specific documentation for detailed endpoint definitions.
