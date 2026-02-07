using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ProceduralMapGenerator _mapGenerator;
    [SerializeField] private MapCameraController _cameraController;
    [SerializeField] private ConvoyVisualizer _convoyVisualizerPrefab;

    [Header("Settings")]
    [SerializeField] private MapSize _defaultMapSize = MapSize.Medium;
    [SerializeField] private int _defaultTruckCount = 3;

    [Header("Debug")]
    [SerializeField] private bool _autoStartLevel = true;
    [SerializeField] private DifficultyType _testDifficulty = DifficultyType.Training;

    private ISimulationSystem _simulation;
    private IEventBus _eventBus;
    private GeneratedMapData _currentMapData;
    private List<ConvoyVisualizer> _convoyVisualizers = new();

    // Input Actions
    private InputAction _toggleSimulationAction;
    private InputAction _regenerateMapAction;

    private void Awake()
    {
        _toggleSimulationAction = new InputAction("ToggleSimulation", InputActionType.Button, "<Keyboard>/space");
        _regenerateMapAction = new InputAction("RegenerateMap", InputActionType.Button, "<Keyboard>/r");
    }

    private void OnEnable()
    {
        _toggleSimulationAction.Enable();
        _regenerateMapAction.Enable();
    }

    private void OnDisable()
    {
        _toggleSimulationAction.Disable();
        _regenerateMapAction.Disable();
    }

    private void Start()
    {
        _simulation = ServiceLocator.Get<ISimulationSystem>();
        _eventBus = ServiceLocator.Get<IEventBus>();

        InitializeComponents();

        if (_autoStartLevel)
        {
            StartNewLevel(_testDifficulty, _defaultMapSize);
        }
    }

    private void InitializeComponents()
    {
        if (_mapGenerator == null)
        {
            var generatorObj = new GameObject("ProceduralMapGenerator");
            _mapGenerator = generatorObj.AddComponent<ProceduralMapGenerator>();
        }
        _mapGenerator.Initialize();

        if (_cameraController == null)
        {
            _cameraController = FindObjectOfType<MapCameraController>();
            if (_cameraController == null)
            {
                var cameraObj = new GameObject("MapCameraController");
                _cameraController = cameraObj.AddComponent<MapCameraController>();
            }
        }
    }

    public void StartNewLevel(DifficultyType difficulty, MapSize mapSize)
    {
        ClearLevel();

        int seed = System.Environment.TickCount;

        _currentMapData = _mapGenerator.Generate(mapSize, seed);

        Debug.Log($"Map generated: Size={mapSize}, Seed={seed}");
        Debug.Log($"Spawn: {_currentMapData.SpawnPoint}");
        Debug.Log($"Deliveries: {_currentMapData.DeliveryPoints.Count}");

        _cameraController.SetupForMapSize(mapSize);
        _cameraController.SetPosition(_currentMapData.SpawnPoint);

        CreateConvoyVisualizer();

        Debug.Log("Level ready. Press SPACE to start simulation.");
    }

    private void CreateConvoyVisualizer()
    {
        if (_convoyVisualizerPrefab == null) return;

        var route = CreateRouteFromPath(_currentMapData.MainRoadPath);

        var visualizer = Instantiate(_convoyVisualizerPrefab);
        visualizer.Initialize(
            convoyId: "convoy_main",
            truckCount: _defaultTruckCount,
            spawnPosition: _currentMapData.SpawnPoint,
            route: route
        );
        _convoyVisualizers.Add(visualizer);

        _cameraController.SetFollowTarget(visualizer.transform);
    }

    private Route CreateRouteFromPath(List<Vector3> path)
    {
        var route = new Route
        {
            Id = "main_route",
            Name = "Основной маршрут"
        };

        for (int i = 0; i < path.Count; i++)
        {
            var nodeType = i == path.Count - 1 ? RouteNodeType.DeliveryPoint : RouteNodeType.Waypoint;
            route.Nodes.Add(new RouteNode($"node_{i}", path[i], nodeType));
        }

        return route;
    }

    private void Update()
    {
        if (_toggleSimulationAction.WasPressedThisFrame())
        {
            ToggleSimulation();
        }

        if (_regenerateMapAction.WasPressedThisFrame())
        {
            StartNewLevel(_testDifficulty, _defaultMapSize);
        }
    }

    private void ToggleSimulation()
    {
        if (_simulation == null) return;

        switch (_simulation.State)
        {
            case SimulationState.Paused:
                _simulation.ResumeSimulation();
                foreach (var vis in _convoyVisualizers) vis.StartMoving();
                break;
            case SimulationState.Running:
                _simulation.PauseSimulation();
                foreach (var vis in _convoyVisualizers) vis.StopMoving();
                break;
            default:
                var levelData = CreateLevelData();
                _simulation.StartSimulation(levelData);
                foreach (var vis in _convoyVisualizers) vis.StartMoving();
                break;
        }
    }

    private LevelData CreateLevelData()
    {
        var route = CreateRouteFromPath(_currentMapData.MainRoadPath);

        return new LevelData
        {
            Id = System.Guid.NewGuid().ToString(),
            Difficulty = _testDifficulty,
            Routes = new List<Route> { route },
            DeliveryPoints = CreateDeliveryPoints(),
            BaseSpawnPoint = new SpawnPoint { Position = _currentMapData.SpawnPoint },
            Parameters = new LevelParameters
            {
                TimeLimit = 300f,
                FuelBudget = 100f,
                Conditions = new VictoryCondition[] { VictoryCondition.DeliverAllCargo }
            },
            Events = new List<MapEvent>()
        };
    }

    private List<DeliveryPoint> CreateDeliveryPoints()
    {
        var points = new List<DeliveryPoint>();

        for (int i = 0; i < _currentMapData.DeliveryPoints.Count; i++)
        {
            points.Add(new DeliveryPoint
            {
                Id = $"delivery_{i}",
                Name = $"Point {(char)('A' + i)}",
                Position = _currentMapData.DeliveryPoints[i],
                RequiredCargo = 10f
            });
        }

        return points;
    }

    private void ClearLevel()
    {
        foreach (var visualizer in _convoyVisualizers)
        {
            if (visualizer != null)
            {
                visualizer.ClearTrucks();
                Destroy(visualizer.gameObject);
            }
        }
        _convoyVisualizers.Clear();

        _mapGenerator?.ClearMap();
    }

    private void OnDestroy()
    {
        _toggleSimulationAction?.Dispose();
        _regenerateMapAction?.Dispose();
        ClearLevel();
    }
}