namespace MediatR.Remote;

public class StrategyTypes
{
    public StrategyTypes(Type requestStrategyType)
    {
        RequestStrategyType = NotificationStrategyType = StreamStrategyType = requestStrategyType;
    }

    public StrategyTypes(Type requestStrategyType, Type notificationStrategyType)
    {
        RequestStrategyType = StreamStrategyType = requestStrategyType;
        NotificationStrategyType = notificationStrategyType;
    }

    public StrategyTypes(Type requestStrategyType, Type notificationStrategyType, Type streamStrategyType)
    {
        RequestStrategyType = requestStrategyType;
        NotificationStrategyType = notificationStrategyType;
        StreamStrategyType = streamStrategyType;
    }

    public Type RequestStrategyType { get; }
    public Type NotificationStrategyType { get; }
    public Type StreamStrategyType { get; }
}