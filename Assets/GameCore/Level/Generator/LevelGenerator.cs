using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : ILevelGenerator
{
    private readonly Dictionary<DifficultyType, IDifficultyStrategy> _strategies;
    private System.Random _random;

    public LevelGenerator()
    {
        _strategies = new Dictionary<DifficultyType, IDifficultyStrategy>
        {
            { DifficultyType.Training, new TrainingDifficulty() },
            { DifficultyType.Easy, new EasyDifficulty() },
            { DifficultyType.Normal, new NormalDifficulty() },
            { DifficultyType.Hard, new HardDifficulty() },
            { DifficultyType.Extreme, new ExtremeDifficulty() }
        };
    }

    public LevelData Generate(DifficultyType difficulty, int seed = -1)
    {
        seed = seed == -1 ? Environment.TickCount : seed;
        _random = new System.Random(seed);

        var strategy = _strategies[difficulty];
        var parameters = strategy.GenerateParameters();

        var levelData = new LevelData
        {
            Id = Guid.NewGuid().ToString(),
            Seed = seed,
            Difficulty = difficulty,
            Parameters = parameters,
            Bounds = GenerateBounds(difficulty)
        };

        levelData.BaseSpawnPoint = GenerateSpawnPoint(levelData.Bounds);
        levelData.DeliveryPoints = GenerateDeliveryPoints(parameters.DeliveryPoints, levelData.Bounds);
        levelData.Routes = GenerateRoutes(levelData.BaseSpawnPoint, levelData.DeliveryPoints, levelData.Bounds);
        levelData.Events = GenerateEvents(parameters.EventsCount, levelData.Routes, difficulty);

        return levelData;
    }

    private MapBounds GenerateBounds(DifficultyType difficulty)
    {
        float size = difficulty switch
        {
            DifficultyType.Training => 200f,
            DifficultyType.Easy => 300f,
            DifficultyType.Normal => 400f,
            DifficultyType.Hard => 500f,
            DifficultyType.Extreme => 600f,
            _ => 300f
        };

        return new MapBounds
        {
            MinX = -size / 2,
            MaxX = size / 2,
            MinZ = -size / 2,
            MaxZ = size / 2
        };
    }

    private SpawnPoint GenerateSpawnPoint(MapBounds bounds)
    {
        return new SpawnPoint
        {
            Position = new Vector3(bounds.MinX + 20f, 0, RandomRange(bounds.MinZ + 50f, bounds.MaxZ - 50f)),
            Rotation = 0f
        };
    }

    private List<DeliveryPoint> GenerateDeliveryPoints(int count, MapBounds bounds)
    {
        var points = new List<DeliveryPoint>();
        var usedPositions = new List<Vector3>();

        for (int i = 0; i < count; i++)
        {
            Vector3 position;
            int attempts = 0;
            do
            {
                position = new Vector3(
                    RandomRange(bounds.MinX + bounds.MaxX * 0.3f, bounds.MaxX - 20f),
                    0,
                    RandomRange(bounds.MinZ + 30f, bounds.MaxZ - 30f)
                );
                attempts++;
            } while (IsTooClose(position, usedPositions, 50f) && attempts < 100);

            usedPositions.Add(position);

            points.Add(new DeliveryPoint
            {
                Id = $"delivery_{i}",
                Name = $"Point {(char)('A' + i)}",
                Position = position,
                RequiredCargo = RandomRange(20f, 50f),
                TimeBonus = RandomRange(50f, 150f),
                IsOptional = i > 0 && _random.NextDouble() < 0.2f
            });
        }

        return points;
    }

    private List<Route> GenerateRoutes(SpawnPoint spawn, List<DeliveryPoint> deliveryPoints, MapBounds bounds)
    {
        var routes = new List<Route>();

        foreach (var delivery in deliveryPoints)
        {
            // Основной маршрут
            var mainRoute = GenerateRouteBetween(
                $"route_main_{delivery.Id}",
                spawn.Position,
                delivery.Position,
                bounds,
                false
            );
            routes.Add(mainRoute);

            // Альтернативный маршрут (более длинный, но безопаснее)
            var altRoute = GenerateRouteBetween(
                $"route_alt_{delivery.Id}",
                spawn.Position,
                delivery.Position,
                bounds,
                true
            );
            altRoute.RiskLevel = mainRoute.RiskLevel * 0.5f;
            routes.Add(altRoute);

            mainRoute.AlternativeRouteIds.Add(altRoute.Id);
            altRoute.AlternativeRouteIds.Add(mainRoute.Id);
        }

        return routes;
    }

    private Route GenerateRouteBetween(string id, Vector3 start, Vector3 end, MapBounds bounds, bool isAlternative)
    {
        var route = new Route
        {
            Id = id,
            Name = isAlternative ? "Объездной маршрут" : "Основной маршрут",
            RiskLevel = RandomRange(0.2f, 0.8f)
        };

        // Начальная точка
        route.Nodes.Add(new RouteNode($"{id}_start", start, RouteNodeType.Waypoint));

        // Генерируем промежуточные точки
        int waypointCount = isAlternative ? _random.Next(4, 7) : _random.Next(2, 4);
        var direction = (end - start).normalized;
        var perpendicular = new Vector3(-direction.z, 0, direction.x);
        var distance = Vector3.Distance(start, end);

        for (int i = 1; i <= waypointCount; i++)
        {
            float t = i / (float)(waypointCount + 1);
            var basePos = Vector3.Lerp(start, end, t);
            
            // Смещение от прямой линии
            float offset = isAlternative 
                ? RandomRange(30f, 60f) * (i % 2 == 0 ? 1 : -1)
                : RandomRange(-20f, 20f);
            
            var position = basePos + perpendicular * offset;
            position.x = Mathf.Clamp(position.x, bounds.MinX + 10f, bounds.MaxX - 10f);
            position.z = Mathf.Clamp(position.z, bounds.MinZ + 10f, bounds.MaxZ - 10f);

            var nodeType = DetermineNodeType(i, waypointCount);
            route.Nodes.Add(new RouteNode($"{id}_wp{i}", position, nodeType));
        }

        // Конечная точка
        route.Nodes.Add(new RouteNode($"{id}_end", end, RouteNodeType.DeliveryPoint));

        // Рассчитываем метрики
        route.EstimatedTime = route.GetTotalDistance() / 30f; // базовая скорость 30
        route.EstimatedFuel = route.GetTotalDistance() * 0.1f;

        return route;
    }

    private RouteNodeType DetermineNodeType(int index, int total)
    {
        var roll = _random.NextDouble();
        
        if (roll < 0.15f) return RouteNodeType.Gate;
        if (roll < 0.25f) return RouteNodeType.BridgeEntry;
        if (roll < 0.35f) return RouteNodeType.RefuelStation;
        if (roll < 0.45f) return RouteNodeType.HazardZone;
        
        return RouteNodeType.Waypoint;
    }

    private List<MapEvent> GenerateEvents(int count, List<Route> routes, DifficultyType difficulty)
    {
        var events = new List<MapEvent>();
        var eventTypes = GetEventTypesForDifficulty(difficulty);

        for (int i = 0; i < count; i++)
        {
            // Выбираем случайный маршрут и точку на нём
            var route = routes[_random.Next(routes.Count)];
            var nodeIndex = _random.Next(1, route.Nodes.Count - 1);
            var node = route.Nodes[nodeIndex];

            var eventType = eventTypes[_random.Next(eventTypes.Count)];
            
            events.Add(new MapEvent
            {
                Id = $"event_{i}",
                Type = eventType,
                Position = node.Position + new Vector3(RandomRange(-10f, 10f), 0, RandomRange(-10f, 10f)),
                Radius = RandomRange(15f, 40f),
                TriggerTime = eventType == MapEventType.BridgeSchedule ? 0 : RandomRange(10f, 120f),
                Duration = RandomRange(30f, 90f),
                Severity = (EventSeverity)_random.Next(0, (int)difficulty + 1)
            });
        }

        return events;
    }

    private List<MapEventType> GetEventTypesForDifficulty(DifficultyType difficulty)
    {
        var types = new List<MapEventType>
        {
            MapEventType.Roadblock,
            MapEventType.Traffic,
            MapEventType.FuelStation
        };

        if (difficulty >= DifficultyType.Easy)
        {
            types.Add(MapEventType.NarrowPassage);
            types.Add(MapEventType.BridgeSchedule);
        }

        if (difficulty >= DifficultyType.Normal)
        {
            types.Add(MapEventType.WeatherHazard);
            types.Add(MapEventType.RepairStation);
        }

        if (difficulty >= DifficultyType.Hard)
        {
            types.Add(MapEventType.AttackZone);
        }

        return types;
    }

    private bool IsTooClose(Vector3 position, List<Vector3> others, float minDistance)
    {
        foreach (var other in others)
        {
            if (Vector3.Distance(position, other) < minDistance)
                return true;
        }
        return false;
    }

    private float RandomRange(float min, float max)
    {
        return (float)(_random.NextDouble() * (max - min) + min);
    }
}