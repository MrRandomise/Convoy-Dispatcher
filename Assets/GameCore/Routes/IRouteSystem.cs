using System.Collections.Generic;

public interface IRouteSystem
{
    void RegisterRoute(Route route);
    Route GetRoute(string routeId);
    List<Route> GetAllRoutes();
    Route FindAlternativeRoute(string currentRouteId, string fromNodeId);
    bool IsSegmentAccessible(RouteSegment segment, float currentTime);
    void SetSegmentCondition(string startNodeId, string endNodeId, RoadCondition condition);
}