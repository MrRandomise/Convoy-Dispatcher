using System;
using System.Collections.Generic;
using UnityEngine;

public class SimulationSystem : ISimulationSystem
{
    public SimulationState State { get; private set; } = SimulationState.Idle;
    public float CurrentTime { get; private set; }
    public float TimeScale { get; set; } = 1f;

    public event Action<SimulationState> OnStateChanged;
    public event Action<float> OnTimeUpdated;
    public event Action<SimulationResult> OnSimulationEnded;

    private LevelData _levelData;
    private readonly Dictionary<string, ConvoySimulationState> _convoyStates = new();
    private readonly List<IConvoy> _activeConvoys = new();
    private SimulationResult _result;

    private IRouteSystem _routeSystem;
    private ITriggerSystem _triggerSystem;
    private IEventBus _eventBus;

    public void StartSimulation(LevelData levelData)
    {
        _levelData = levelData;
        _routeSystem = ServiceLocator.Get<IRouteSystem>();
        _triggerSystem = ServiceLocator.Get<ITriggerSystem>();
        _eventBus = ServiceLocator.Get<IEventBus>();

        CurrentTime = 0f;
        _result = new SimulationResult();
        _convoyStates.Clear();

        // Регистрируем маршруты
        foreach (var route in levelData.Routes)
        {
            _routeSystem.RegisterRoute(route);
        }

        // Инициализируем состояния конвоев
        foreach (var convoy in _activeConvoys)
        {
            InitializeConvoyState(convoy);
        }

        SubscribeToEvents();
        SetState(SimulationState.Running);
    }

    public void RegisterConvoy(IConvoy convoy)
    {
        _activeConvoys.Add(convoy);
    }

    public void ClearConvoys()
    {
        _activeConvoys.Clear();
        _convoyStates.Clear();
    }

    private void InitializeConvoyState(IConvoy convoy)
    {
        // Находим основной маршрут (не альтернативный)
        var mainRoute = _levelData.Routes.Find(r => !r.Id.Contains("alt"))
                        ?? _levelData.Routes[0];

        var state = new ConvoySimulationState
        {
            ConvoyId = convoy.Id,
            CurrentRouteId = mainRoute.Id,
            // Начинаем с индекса 1 - мы уже на базе (индекс 0)
            CurrentNodeIndex = 1,
            Position = _levelData.BaseSpawnPoint.Position,
            CurrentSpeed = GetSpeedFromMode(convoy.Rules.Speed, convoy.Stats.Speed),
            CurrentFuel = convoy.Stats.FuelCapacity,
            BehaviorState = ConvoyBehaviorState.Moving
        };
        _convoyStates[convoy.Id] = state;

        Debug.Log($"Convoy {convoy.Id} initialized on route {mainRoute.Id} with {mainRoute.Nodes.Count} nodes, speed: {state.CurrentSpeed}");
    }

    public void PauseSimulation()
    {
        if (State == SimulationState.Running)
        {
            SetState(SimulationState.Paused);
        }
    }

    public void ResumeSimulation()
    {
        if (State == SimulationState.Paused)
        {
            SetState(SimulationState.Running);
        }
    }

    public void StopSimulation()
    {
        UnsubscribeFromEvents();
        ClearConvoys();
        SetState(SimulationState.Idle);
    }

    public void Update(float deltaTime)
    {
        if (State != SimulationState.Running) return;

        var scaledDelta = deltaTime * TimeScale;
        CurrentTime += scaledDelta;
        OnTimeUpdated?.Invoke(CurrentTime);

        UpdateMapEvents();

        foreach (var convoy in _activeConvoys)
        {
            UpdateConvoy(convoy, scaledDelta);
        }

        CheckVictoryConditions();
        CheckFailureConditions();
    }

    private void UpdateConvoy(IConvoy convoy, float deltaTime)
    {
        if (!_convoyStates.TryGetValue(convoy.Id, out var state)) return;
        if (state.BehaviorState == ConvoyBehaviorState.Destroyed) return;
        if (state.BehaviorState == ConvoyBehaviorState.Stopped) return;

        // Обновляем триггеры
        var context = BuildTriggerContext(convoy, state);
        _triggerSystem.EvaluateTriggers(convoy.Id, context);

        switch (state.BehaviorState)
        {
            case ConvoyBehaviorState.Moving:
                UpdateMovement(convoy, state, deltaTime);
                break;
            case ConvoyBehaviorState.Waiting:
                UpdateWaiting(state, deltaTime);
                break;
            case ConvoyBehaviorState.Refueling:
                UpdateRefueling(convoy, state, deltaTime);
                break;
            case ConvoyBehaviorState.UnderAttack:
                UpdateCombat(convoy, state, deltaTime);
                break;
        }

        // Расход топлива
        if (state.BehaviorState == ConvoyBehaviorState.Moving)
        {
            state.CurrentFuel -= state.CurrentSpeed * 0.001f * deltaTime;
            _result.FuelUsed += state.CurrentSpeed * 0.001f * deltaTime;
        }
    }

    private void UpdateMovement(IConvoy convoy, ConvoySimulationState state, float deltaTime)
    {
        var route = _routeSystem.GetRoute(state.CurrentRouteId);
        if (route == null)
        {
            Debug.LogError($"Route {state.CurrentRouteId} not found!");
            return;
        }

        // Проверяем, не вышли ли за пределы маршрута
        if (state.CurrentNodeIndex >= route.Nodes.Count)
        {
            HandleRouteCompleted(convoy, state);
            return;
        }

        var targetNode = route.GetNode(state.CurrentNodeIndex);
        if (targetNode == null)
        {
            HandleRouteCompleted(convoy, state);
            return;
        }

        // Проверяем доступность сегмента
        if (state.CurrentNodeIndex > 0)
        {
            var prevNode = route.GetNode(state.CurrentNodeIndex - 1);
            var segment = new RouteSegment { Start = prevNode, End = targetNode };

            if (!_routeSystem.IsSegmentAccessible(segment, CurrentTime))
            {
                state.BehaviorState = ConvoyBehaviorState.Waiting;
                state.WaitTimer = 2f; // Ждём 2 секунды перед повторной проверкой
                return;
            }
        }

        // Движение к точке
        var direction = (targetNode.Position - state.Position).normalized;
        var moveDistance = state.CurrentSpeed * deltaTime;
        var remainingDistance = Vector3.Distance(state.Position, targetNode.Position);

        if (moveDistance >= remainingDistance)
        {
            state.Position = targetNode.Position;
            HandleNodeReached(convoy, state, targetNode);
        }
        else
        {
            state.Position += direction * moveDistance;
        }

        // Публикуем обновление позиции
        _eventBus.Publish(new ConvoyPositionUpdatedEvent
        {
            ConvoyId = convoy.Id,
            Position = state.Position
        });
    }

    private void HandleNodeReached(IConvoy convoy, ConvoySimulationState state, RouteNode node)
    {
        Debug.Log($"Convoy {convoy.Id} reached node {node.Id} (type: {node.Type}), index: {state.CurrentNodeIndex}");

        switch (node.Type)
        {
            case RouteNodeType.DeliveryPoint:
                HandleDelivery(convoy, state, node);
                break;
            case RouteNodeType.RefuelStation:
                state.BehaviorState = ConvoyBehaviorState.Refueling;
                break;
            case RouteNodeType.Gate:
                if (node.WaitTime > 0)
                {
                    state.BehaviorState = ConvoyBehaviorState.Waiting;
                    state.WaitTimer = node.WaitTime;
                }
                break;
            case RouteNodeType.HazardZone:
                CheckHazardZone(convoy, state);
                break;
        }

        state.CurrentNodeIndex++;

        _eventBus.Publish(new ConvoyNodeReachedEvent
        {
            ConvoyId = convoy.Id,
            NodeId = node.Id,
            NodeType = node.Type
        });
    }

    private void HandleDelivery(IConvoy convoy, ConvoySimulationState state, RouteNode node)
    {
        var delivery = _levelData.DeliveryPoints.Find(d =>
            Vector3.Distance(d.Position, node.Position) < 10f);

        if (delivery != null && !_result.CompletedDeliveries.Contains(delivery.Id))
        {
            state.CargoDelivered += delivery.RequiredCargo;
            _result.CompletedDeliveries.Add(delivery.Id);

            Debug.Log($"Delivery completed: {delivery.Id}, total: {_result.CompletedDeliveries.Count}/{_levelData.DeliveryPoints.Count}");

            _eventBus.Publish(new ConvoyArrivedEvent
            {
                ConvoyId = convoy.Id,
                DeliveryPointId = delivery.Id
            });
        }
    }

    private void HandleRouteCompleted(IConvoy convoy, ConvoySimulationState state)
    {
        Debug.Log($"Convoy {convoy.Id} completed route {state.CurrentRouteId}");
        state.BehaviorState = ConvoyBehaviorState.Stopped;

        _eventBus.Publish(new ConvoyRouteCompletedEvent
        {
            ConvoyId = convoy.Id,
            RouteId = state.CurrentRouteId
        });
    }

    private void UpdateWaiting(ConvoySimulationState state, float deltaTime)
    {
        state.WaitTimer -= deltaTime;
        if (state.WaitTimer <= 0)
        {
            state.BehaviorState = ConvoyBehaviorState.Moving;
        }
    }

    private void UpdateRefueling(IConvoy convoy, ConvoySimulationState state, float deltaTime)
    {
        state.CurrentFuel += 20f * deltaTime;
        if (state.CurrentFuel >= convoy.Stats.FuelCapacity)
        {
            state.CurrentFuel = convoy.Stats.FuelCapacity;
            state.BehaviorState = ConvoyBehaviorState.Moving;
        }
    }

    private void UpdateCombat(IConvoy convoy, ConvoySimulationState state, float deltaTime)
    {
        float damagePerSecond = 5f;
        float defenseRating = convoy.Escorts.Count * 10f;
        float actualDamage = Mathf.Max(0, damagePerSecond - defenseRating) * deltaTime;

        state.TotalDamage += actualDamage;
        _result.TotalDamage += actualDamage;

        // Бой длится 5 секунд, потом конвой продолжает движение или уничтожается
        state.WaitTimer += deltaTime;
        if (state.WaitTimer >= 5f)
        {
            if (state.TotalDamage >= convoy.Stats.Health)
            {
                state.BehaviorState = ConvoyBehaviorState.Destroyed;
                _result.ConvoysLost++;
                _eventBus.Publish(new ConvoyDestroyedEvent { ConvoyId = convoy.Id });
            }
            else
            {
                state.BehaviorState = ConvoyBehaviorState.Moving;
                state.WaitTimer = 0f;
            }
        }
    }

    private void CheckHazardZone(IConvoy convoy, ConvoySimulationState state)
    {
        var hazardEvent = _levelData.Events.Find(e =>
            e.Type == MapEventType.AttackZone &&
            Vector3.Distance(e.Position, state.Position) < e.Radius);

        if (hazardEvent != null && hazardEvent.Severity >= EventSeverity.Medium)
        {
            state.BehaviorState = ConvoyBehaviorState.UnderAttack;
            state.WaitTimer = 0f;

            _eventBus.Publish(new ConvoyUnderAttackEvent
            {
                ConvoyId = convoy.Id,
                AttackSeverity = hazardEvent.Severity
            });
        }
    }

    private void UpdateMapEvents()
    {
        foreach (var mapEvent in _levelData.Events)
        {
            if (mapEvent.TriggerTime > 0 &&
                CurrentTime >= mapEvent.TriggerTime &&
                CurrentTime < mapEvent.TriggerTime + mapEvent.Duration)
            {
                ApplyMapEvent(mapEvent);
            }
        }
    }

    private void ApplyMapEvent(MapEvent mapEvent)
    {
        // Реализация событий карты
    }

    private TriggerContext BuildTriggerContext(IConvoy convoy, ConvoySimulationState state)
    {
        var route = _routeSystem.GetRoute(state.CurrentRouteId);
        var nextNode = route?.GetNode(state.CurrentNodeIndex);

        return new TriggerContext
        {
            Convoy = convoy,
            CurrentFuel = state.CurrentFuel,
            MaxFuel = convoy.Stats.FuelCapacity,
            HasObstacleAhead = CheckObstacleAhead(state),
            IsUnderAttack = state.BehaviorState == ConvoyBehaviorState.UnderAttack,
            IsConvoyLagging = CheckConvoyLagging(convoy.Id),
            DistanceToNextPoint = nextNode != null ? Vector3.Distance(state.Position, nextNode.Position) : 0,
            RoadCondition = GetCurrentRoadCondition(state)
        };
    }

    private bool CheckObstacleAhead(ConvoySimulationState state)
    {
        return _levelData.Events.Exists(e =>
            (e.Type == MapEventType.Roadblock || e.Type == MapEventType.Traffic) &&
            Vector3.Distance(e.Position, state.Position) < 50f);
    }

    private bool CheckConvoyLagging(string convoyId)
    {
        return false;
    }

    private RoadCondition GetCurrentRoadCondition(ConvoySimulationState state)
    {
        var nearbyEvent = _levelData.Events.Find(e =>
            Vector3.Distance(e.Position, state.Position) < e.Radius);

        if (nearbyEvent == null) return RoadCondition.Normal;

        return nearbyEvent.Type switch
        {
            MapEventType.Traffic => RoadCondition.Congested,
            MapEventType.Roadblock => RoadCondition.Blocked,
            MapEventType.WeatherHazard => RoadCondition.Slippery,
            _ => RoadCondition.Normal
        };
    }

    private void CheckVictoryConditions()
    {
        // Проверяем, все ли доставки выполнены
        bool allDelivered = _result.CompletedDeliveries.Count >= _levelData.DeliveryPoints.Count;

        // Проверяем, все ли конвои завершили маршрут
        bool allConvoysStopped = true;
        foreach (var state in _convoyStates.Values)
        {
            if (state.BehaviorState != ConvoyBehaviorState.Stopped &&
                state.BehaviorState != ConvoyBehaviorState.Destroyed)
            {
                allConvoysStopped = false;
                break;
            }
        }

        // Победа только когда ВСЕ доставки выполнены И ВСЕ конвои остановились
        if (allDelivered && allConvoysStopped)
        {
            Debug.Log($"Victory conditions met! Deliveries: {_result.CompletedDeliveries.Count}, All stopped: {allConvoysStopped}");
            ValidateVictoryConditions();
        }
    }

    private void ValidateVictoryConditions()
    {
        _result.IsSuccess = true;
        _result.TotalTime = CurrentTime;

        // Проверяем наличие Parameters и Conditions
        if (_levelData.Parameters?.Conditions != null)
        {
            foreach (var condition in _levelData.Parameters.Conditions)
            {
                bool met = condition switch
                {
                    VictoryCondition.DeliverAllCargo =>
                        _result.CompletedDeliveries.Count >= _levelData.DeliveryPoints.Count,
                    VictoryCondition.NoConvoyLoss => _result.ConvoysLost == 0,
                    VictoryCondition.NoTruckLoss => _result.TrucksLost == 0,
                    VictoryCondition.WithinTimeLimit => CurrentTime <= _levelData.Parameters.TimeLimit,
                    VictoryCondition.WithinFuelBudget => _result.FuelUsed <= _levelData.Parameters.FuelBudget,
                    VictoryCondition.NoDamage => _result.TotalDamage == 0,
                    _ => true
                };

                if (!met)
                {
                    _result.IsSuccess = false;
                    _result.FailedConditions.Add(condition.ToString());
                }
            }
        }

        if (_result.IsSuccess)
        {
            _result.CoinsEarned = CalculateReward();
        }

        SetState(_result.IsSuccess ? SimulationState.Completed : SimulationState.Failed);
        OnSimulationEnded?.Invoke(_result);
    }

    private void CheckFailureConditions()
    {
        // Все конвои уничтожены
        if (_convoyStates.Count > 0)
        {
            bool allDestroyed = true;
            foreach (var state in _convoyStates.Values)
            {
                if (state.BehaviorState != ConvoyBehaviorState.Destroyed)
                {
                    allDestroyed = false;
                    break;
                }
            }

            if (allDestroyed)
            {
                _result.IsSuccess = false;
                _result.TotalTime = CurrentTime;
                _result.FailedConditions.Add("AllConvoysDestroyed");
                SetState(SimulationState.Failed);
                OnSimulationEnded?.Invoke(_result);
                return;
            }
        }

        // Превышение времени (только если задан лимит)
        float timeLimit = _levelData.Parameters?.TimeLimit ?? 0f;
        if (timeLimit > 0 && CurrentTime > timeLimit * 2f)
        {
            _result.IsSuccess = false;
            _result.TotalTime = CurrentTime;
            _result.FailedConditions.Add("TimeExceeded");
            SetState(SimulationState.Failed);
            OnSimulationEnded?.Invoke(_result);
        }
    }

    private int CalculateReward()
    {
        int baseReward = 100;
        float timeBonus = Mathf.Max(0, _levelData.Parameters.TimeLimit - CurrentTime) * 0.5f;
        float fuelBonus = Mathf.Max(0, _levelData.Parameters.FuelBudget - _result.FuelUsed) * 0.3f;

        return Mathf.RoundToInt(baseReward + timeBonus + fuelBonus);
    }

    private float GetSpeedFromMode(SpeedMode mode, float baseSpeed)
    {
        return mode switch
        {
            SpeedMode.Slow => baseSpeed * 0.6f,
            SpeedMode.Normal => baseSpeed,
            SpeedMode.Fast => baseSpeed * 1.4f,
            _ => baseSpeed
        };
    }

    private void SetState(SimulationState newState)
    {
        State = newState;
        OnStateChanged?.Invoke(newState);
    }

    private void SubscribeToEvents()
    {
        _eventBus.Subscribe<RouteChangeRequestedEvent>(OnRouteChangeRequested);
        _eventBus.Subscribe<EmergencyStopEvent>(OnEmergencyStop);
    }

    private void UnsubscribeFromEvents()
    {
        _eventBus?.Unsubscribe<RouteChangeRequestedEvent>(OnRouteChangeRequested);
        _eventBus?.Unsubscribe<EmergencyStopEvent>(OnEmergencyStop);
    }

    private void OnRouteChangeRequested(RouteChangeRequestedEvent evt)
    {
        if (_convoyStates.TryGetValue(evt.ConvoyId, out var state))
        {
            var newRoute = _routeSystem.GetRoute(evt.NewRouteId);
            if (newRoute != null)
            {
                state.CurrentRouteId = evt.NewRouteId;
                state.CurrentNodeIndex = FindClosestNodeIndex(newRoute, state.Position);
            }
        }
    }

    private void OnEmergencyStop(EmergencyStopEvent evt)
    {
        if (evt.ConvoyId == "*")
        {
            foreach (var state in _convoyStates.Values)
            {
                state.BehaviorState = ConvoyBehaviorState.Stopped;
            }
        }
        else if (_convoyStates.TryGetValue(evt.ConvoyId, out var state))
        {
            state.BehaviorState = ConvoyBehaviorState.Stopped;
        }
    }

    private int FindClosestNodeIndex(Route route, Vector3 position)
    {
        int closest = 0;
        float minDist = float.MaxValue;

        for (int i = 0; i < route.Nodes.Count; i++)
        {
            float dist = Vector3.Distance(route.Nodes[i].Position, position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = i;
            }
        }
        return closest;
    }
}