using UnityEngine;

public struct RouteNodeSelectedEvent : IGameEvent
{
    public Vector3 Position;
    public RouteNodeType NodeType;
}