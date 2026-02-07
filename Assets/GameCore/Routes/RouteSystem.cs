using System.Collections.Generic;
using System.Linq;

public class RouteSystem : IRouteSystem
{
    private readonly Dictionary<string, Route> _routes = new();
    private readonly Dictionary<string, RouteSegment> _segments = new();

    public void RegisterRoute(Route route)
    {
        _routes[route.Id] = route;
        
        // Создаём сегменты для маршрута
        for (int i = 0; i < route.Nodes.Count - 1; i++)
        {
            var segmentKey = GetSegmentKey(route.Nodes[i].Id, route.Nodes[i + 1].Id);
            if (!_segments.ContainsKey(segmentKey))
            {
                _segments[segmentKey] = new RouteSegment
                {
                    Start = route.Nodes[i],
                    End = route.Nodes[i + 1]
                };
            }
        }
    }

    public Route GetRoute(string routeId)
    {
        return _routes.TryGetValue(routeId, out var route) ? route : null;
    }

    public List<Route> GetAllRoutes() => _routes.Values.ToList();

    public Route FindAlternativeRoute(string currentRouteId, string fromNodeId)
    {
        var currentRoute = GetRoute(currentRouteId);
        if (currentRoute == null) return null;

        foreach (var altId in currentRoute.AlternativeRouteIds)
        {
            var altRoute = GetRoute(altId);
            if (altRoute != null && altRoute.Nodes.Any(n => n.Id == fromNodeId))
            {
                return altRoute;
            }
        }
        return null;
    }

    public bool IsSegmentAccessible(RouteSegment segment, float currentTime)
    {
        if (segment.Condition == RoadCondition.Blocked)
            return false;

        if (segment.AccessWindow != null)
            return segment.AccessWindow.IsOpen(currentTime);

        return true;
    }

    public void SetSegmentCondition(string startNodeId, string endNodeId, RoadCondition condition)
    {
        var key = GetSegmentKey(startNodeId, endNodeId);
        if (_segments.TryGetValue(key, out var segment))
        {
            segment.Condition = condition;
            ServiceLocator.Get<IEventBus>().Publish(new RoadConditionChangedEvent
            {
                SegmentStartId = startNodeId,
                SegmentEndId = endNodeId,
                NewCondition = condition
            });
        }
    }

    private string GetSegmentKey(string startId, string endId) => $"{startId}_{endId}";
}