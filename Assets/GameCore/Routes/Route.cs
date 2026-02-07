using System;
using System.Collections.Generic;

[Serializable]
public class Route
{
    public string Id;
    public string Name;
    public List<RouteNode> Nodes = new();
    public List<string> AlternativeRouteIds = new(); // Альтернативные маршруты
    public float EstimatedTime;
    public float EstimatedFuel;
    public float RiskLevel; // 0-1

    public RouteNode GetNode(int index)
    {
        return index >= 0 && index < Nodes.Count ? Nodes[index] : null;
    }

    public RouteNode GetNextNode(string currentNodeId)
    {
        var index = Nodes.FindIndex(n => n.Id == currentNodeId);
        return GetNode(index + 1);
    }

    public float GetTotalDistance()
    {
        float distance = 0;
        for (int i = 0; i < Nodes.Count - 1; i++)
        {
            distance += UnityEngine.Vector3.Distance(Nodes[i].Position, Nodes[i + 1].Position);
        }
        return distance;
    }
}