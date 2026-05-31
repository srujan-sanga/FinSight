using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace FinSight.Contracts
{
    /// <summary>
    /// Base class for all incoming requests.
    /// Includes correlation ID, timestamp, and request tracking.
    /// </summary>
    [ProtoContract]
    public abstract class MessageRequest
    {
        /// <summary>
        /// Unique identifier for correlating requests across services.
        /// </summary>
        [ProtoMember(1)]
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Unix timestamp in milliseconds when request was created.
        /// </summary>
        [ProtoMember(2)]
        public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        /// <summary>
        /// Unique request identifier for audit and logging.
        /// </summary>
        [ProtoMember(3)]
        public string RequestId { get; set; } = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Base class for all responses.
    /// Includes status, message, and correlation tracking.
    /// </summary>
    [ProtoContract]
    public abstract class MessageResponse
    {
        /// <summary>
        /// Correlation ID matching the request.
        /// </summary>
        [ProtoMember(1)]
        public string CorrelationId { get; set; } = string.Empty;

        /// <summary>
        /// Unix timestamp in milliseconds when response was created.
        /// </summary>
        [ProtoMember(2)]
        public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        /// <summary>
        /// Indicates whether the operation succeeded.
        /// </summary>
        [ProtoMember(3)]
        public bool Success { get; set; } = true;

        /// <summary>
        /// Human-readable message (especially for errors).
        /// </summary>
        [ProtoMember(4)]
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Standard error response structure.
    /// </summary>
    public class ErrorResponse : MessageResponse
    {
        /// <summary>
        /// Dictionary of validation errors per field.
        /// </summary>
        public Dictionary<string, string[]> Errors { get; set; } = new();

        /// <summary>
        /// Stack trace (only in development).
        /// </summary>
        public string StackTrace { get; set; } = string.Empty;
    }

    /// <summary>
    /// Generic paged response for list operations.
    /// </summary>
    public class PagedResponse<T> : MessageResponse
    {
        /// <summary>
        /// Items in the current page.
        /// </summary>
        public List<T> Items { get; set; } = new();

        /// <summary>
        /// Total number of items across all pages.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Current page number (1-based).
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Number of items per page.
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// Total number of pages.
        /// </summary>
        public int TotalPages => (TotalCount + PageSize - 1) / PageSize;

        /// <summary>
        /// Indicates if there is a next page.
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;

        /// <summary>
        /// Indicates if there is a previous page.
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;
    }

    /// <summary>
    /// Base class for paginated requests.
    /// </summary>
    public class PagedRequest : MessageRequest
    {
        /// <summary>
        /// Page number (1-based).
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Number of items per page.
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// Field to sort by.
        /// </summary>
        public string SortBy { get; set; } = string.Empty;

        /// <summary>
        /// Sort in descending order.
        /// </summary>
        public bool SortDescending { get; set; }

        /// <summary>
        /// Validates pagination parameters.
        /// </summary>
        public IEnumerable<string> Validate()
        {
            if (PageNumber < 1)
                yield return "PageNumber must be greater than 0";
            if (PageSize < 1 || PageSize > 100)
                yield return "PageSize must be between 1 and 100";
        }
    }

    /// <summary>
    /// User context extracted from JWT token.
    /// </summary>
    public class UserContext
    {
        /// <summary>
        /// Unique user identifier.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// User's email address.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User's assigned roles.
        /// </summary>
        public IEnumerable<string> Roles { get; set; } = new List<string>();

        /// <summary>
        /// Additional claims from the token.
        /// </summary>
        public Dictionary<string, object> Claims { get; set; } = new();

        /// <summary>
        /// Check if user has a specific role.
        /// </summary>
        public bool HasRole(string role)
        {
            return Roles?.Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase)) ?? false;
        }

        /// <summary>
        /// Check if user has a specific permission.
        /// </summary>
        public bool HasPermission(string permission)
        {
            return Claims?.ContainsKey(permission) ?? false;
        }

        /// <summary>
        /// Check if user is admin.
        /// </summary>
        public bool IsAdmin => HasRole("Admin");

        /// <summary>
        /// Check if user is advisor.
        /// </summary>
        public bool IsAdvisor => HasRole("Advisor");

        /// <summary>
        /// Check if user is customer.
        /// </summary>
        public bool IsCustomer => HasRole("Customer");
    }
}