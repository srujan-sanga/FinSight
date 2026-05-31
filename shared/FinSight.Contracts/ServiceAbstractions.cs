namespace FinSight.Contracts;

public interface IManager
{
}

public interface IExternalManager : IManager
{
}

public interface IEngine
{
}

public interface IDatabaseRA
{
}

public abstract class ManagerBase : IManager
{
    protected static TResponse Failure<TResponse>(MessageRequest request, string message)
        where TResponse : MessageResponse, new()
    {
        return new TResponse
        {
            Success = false,
            Message = message,
            CorrelationId = request.CorrelationId
        };
    }
}

public abstract class EngineBase : IEngine
{
}

public abstract class RaBase : IDatabaseRA
{
}
