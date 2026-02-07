using UnityEngine;

public struct ConvoyPositionUpdatedEvent : IGameEvent
{
    public string ConvoyId;
    public Vector3 Position;
}

public struct ConvoyNodeReachedEvent : IGameEvent
{
    public string ConvoyId;
    public string NodeId;
    public RouteNodeType NodeType;
}

public struct ConvoyRouteCompletedEvent : IGameEvent
{
    public string ConvoyId;
    public string RouteId;
}

public struct ConvoyDestroyedEvent : IGameEvent
{
    public string ConvoyId;
}

public struct ConvoyUnderAttackEvent : IGameEvent
{
    public string ConvoyId;
    public EventSeverity AttackSeverity;
}

public struct ConvoyArrivedEvent : IGameEvent
{
    public string ConvoyId;
       public string DeliveryPointId;
}

public struct ConvoyDamagedEvent : IGameEvent
{
    public string ConvoyId;
    public float Damage;
}

public struct RouteChangeRequestedEvent : IGameEvent
{
    public string ConvoyId;
    public string NewRouteId;
}

public struct EmergencyStopEvent : IGameEvent
{
    public string ConvoyId;
}

public struct RefuelRequestedEvent : IGameEvent
{
    public string ConvoyId;
}

public struct ConvoySpeedChangedEvent : IGameEvent
{
    public string ConvoyId;
    public SpeedMode NewSpeed;
}

public struct TriggerActivatedEvent : IGameEvent
{
    public string TriggerId;
    public string ConvoyId;
}

public struct RoadConditionChangedEvent : IGameEvent
{
    public string SegmentStartId;
    public string SegmentEndId;
    public RoadCondition NewCondition;
}