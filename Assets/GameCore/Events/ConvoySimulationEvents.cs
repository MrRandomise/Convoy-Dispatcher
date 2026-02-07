public struct ConvoySimulationStartedEvent : IGameEvent
{
    public string ConvoyId;
}

public struct ConvoySimulationStoppedEvent : IGameEvent
{
    public string ConvoyId;
}