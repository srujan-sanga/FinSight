using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace FinSight.GrpcInterceptors;

/// <summary>
/// Ensures CorrelationId flows through gRPC calls.
/// Extracts from request metadata or generates a new one.
/// </summary>
public sealed class CorrelationIdInterceptor : Interceptor
{
    private readonly ILogger<CorrelationIdInterceptor> _logger;

    public CorrelationIdInterceptor(ILogger<CorrelationIdInterceptor> logger)
    {
        _logger = logger;
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var correlationId = Guid.NewGuid().ToString();
        var headers = new Metadata
        {
            { "x-correlation-id", correlationId }
        };

        var options = context.Options.WithHeaders(headers);
        var newContext = new ClientInterceptorContext<TRequest, TResponse>(
            context.Method,
            context.Host,
            options);

        return continuation(request, newContext);
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var correlationId = context.RequestHeaders?
            .FirstOrDefault(h => h.Key == "x-correlation-id")?.Value
            ?? Guid.NewGuid().ToString();

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["Method"] = context.Method
        }))
        {
            _logger.LogInformation("Processing gRPC call: {Method}", context.Method);
            try
            {
                return await continuation(request, context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing gRPC call: {Method}", context.Method);
                throw;
            }
        }
    }
}