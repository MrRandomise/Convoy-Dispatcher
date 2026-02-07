using System;
using UnityEngine;

[Serializable]
public class RouteNode
{
    public string Id;
    public Vector3 Position;
    public RouteNodeType Type;
    public float SpeedLimit = -1f; // -1 = без ограничения
    public float WaitTime = 0f;    // Время ожидания (для шлагбаумов)
    
    public RouteNode(string id, Vector3 position, RouteNodeType type = RouteNodeType.Waypoint)
    {
        Id = id;
        Position = position;
        Type = type;
    }
}
