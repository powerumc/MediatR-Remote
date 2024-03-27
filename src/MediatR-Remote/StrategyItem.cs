namespace MediatR.Remote;

public class StrategyItem
{
    public StrategyItem(Type requestStrategyType)
    {
        RequestStrategyType = NotificationStrategyType = StreamStrategyType = requestStrategyType;
    }

    public StrategyItem(Type requestStrategyType, Type notificationStrategyType)
    {
        RequestStrategyType = StreamStrategyType = requestStrategyType;
        NotificationStrategyType = notificationStrategyType;
    }

    public StrategyItem(Type requestStrategyType, Type notificationStrategyType, Type streamStrategyType)
    {
        RequestStrategyType = requestStrategyType;
        NotificationStrategyType = notificationStrategyType;
        StreamStrategyType = streamStrategyType;
    }

    public Type RequestStrategyType { get; }
    public Type NotificationStrategyType { get; }
    public Type StreamStrategyType { get; }
}