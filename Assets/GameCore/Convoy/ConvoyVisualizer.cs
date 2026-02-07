using System.Collections.Generic;
using UnityEngine;

public class ConvoyVisualizer : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject _truckPrefab;

    [Header("Convoy Settings")]
    [SerializeField] private float _truckSpacing = 8f;
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private float _rotationSpeed = 5f;

    private List<TruckController> _trucks = new();
    private IEventBus _eventBus;
    private IRouteSystem _routeSystem;
    private string _convoyId;
    private Route _currentRoute;
    private bool _isMoving;

    public void Initialize(string convoyId, int truckCount, Vector3 spawnPosition, Route route)
    {
        _convoyId = convoyId;
        _currentRoute = route;
        _eventBus = ServiceLocator.Get<IEventBus>();
        _routeSystem = ServiceLocator.Get<IRouteSystem>();

        _eventBus.Subscribe<ConvoySimulationStartedEvent>(OnSimulationStarted);
        _eventBus.Subscribe<ConvoySimulationStoppedEvent>(OnSimulationStopped);

        // Рассчитываем начальное направление от базы к первой точке
        Vector3 initialDirection = Vector3.forward;
        if (route != null && route.Nodes.Count > 1)
        {
            initialDirection = (route.Nodes[1].Position - route.Nodes[0].Position).normalized;
        }

        // Создаём грузовики, выстроенные вдоль начального направления
        for (int i = 0; i < truckCount; i++)
        {
            // Каждый следующий грузовик позади предыдущего
            Vector3 truckPosition = spawnPosition - initialDirection * (_truckSpacing * i);
            CreateTruck(i, truckPosition, initialDirection, route);
        }
    }

    private void CreateTruck(int index, Vector3 position, Vector3 direction, Route route)
    {
        GameObject truckObj;

        if (_truckPrefab != null)
        {
            truckObj = Instantiate(_truckPrefab, transform);
        }
        else
        {
            truckObj = CreateDefaultTruck(index);
        }

        truckObj.name = $"Truck_{index}";
        truckObj.transform.position = position + Vector3.up * 0.5f;

        // Поворачиваем в направлении движения
        if (direction.sqrMagnitude > 0.01f)
        {
            truckObj.transform.rotation = Quaternion.LookRotation(direction);
        }

        var controller = new TruckController
        {
            GameObject = truckObj,
            Index = index,
            CurrentPosition = position,
            MoveSpeed = _moveSpeed,
            RotationSpeed = _rotationSpeed
        };

        // Инициализируем путь для каждого грузовика
        if (route != null)
        {
            controller.InitializePath(route, _truckSpacing * index);
        }

        _trucks.Add(controller);
    }

    private GameObject CreateDefaultTruck(int index)
    {
        var truckObj = new GameObject($"Truck_{index}");
        truckObj.transform.parent = transform;

        // Кабина
        var cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cabin.transform.parent = truckObj.transform;
        cabin.transform.localPosition = new Vector3(0, 0.5f, 1.2f);
        cabin.transform.localScale = new Vector3(1.8f, 1.2f, 1.2f);
        var cabinMat = new Material(Shader.Find("Standard"));
        cabinMat.color = GetTruckColor(index);
        cabin.GetComponent<Renderer>().material = cabinMat;
        Destroy(cabin.GetComponent<Collider>());

        // Кузов
        var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.transform.parent = truckObj.transform;
        body.transform.localPosition = new Vector3(0, 0.6f, -0.8f);
        body.transform.localScale = new Vector3(2f, 1.5f, 3f);
        var bodyMat = new Material(Shader.Find("Standard"));
        bodyMat.color = new Color(0.3f, 0.3f, 0.35f);
        body.GetComponent<Renderer>().material = bodyMat;
        Destroy(body.GetComponent<Collider>());

        return truckObj;
    }

    private Color GetTruckColor(int index)
    {
        return index switch
        {
            0 => new Color(0.9f, 0.2f, 0.2f),  // Красный - лидер
            1 => new Color(0.2f, 0.6f, 0.9f),  // Синий
            2 => new Color(0.2f, 0.8f, 0.3f),  // Зелёный
            _ => new Color(0.7f, 0.5f, 0.2f)   // Оранжевый
        };
    }

    private void OnSimulationStarted(ConvoySimulationStartedEvent evt)
    {
        if (evt.ConvoyId == _convoyId)
        {
            _isMoving = true;
        }
    }

    private void OnSimulationStopped(ConvoySimulationStoppedEvent evt)
    {
        if (evt.ConvoyId == _convoyId)
        {
            _isMoving = false;
        }
    }

    private void Update()
    {
        if (!_isMoving) return;

        // Обновляем каждый грузовик индивидуально
        foreach (var truck in _trucks)
        {
            truck.UpdateMovement(Time.deltaTime);
        }
    }

    public void StartMoving()
    {
        _isMoving = true;
    }

    public void StopMoving()
    {
        _isMoving = false;
    }

    public void ClearTrucks()
    {
        foreach (var truck in _trucks)
        {
            if (truck.GameObject != null)
            {
                Destroy(truck.GameObject);
            }
        }
        _trucks.Clear();
    }

    private void OnDestroy()
    {
        _eventBus?.Unsubscribe<ConvoySimulationStartedEvent>(OnSimulationStarted);
        _eventBus?.Unsubscribe<ConvoySimulationStoppedEvent>(OnSimulationStopped);
    }

    /// <summary>
    /// Контроллер отдельного грузовика - управляет движением по маршруту
    /// </summary>
    private class TruckController
    {
        public GameObject GameObject;
        public int Index;
        public Vector3 CurrentPosition;
        public float MoveSpeed;
        public float RotationSpeed;

        private List<Vector3> _pathPoints = new();
        private int _currentPointIndex;
        private float _distanceTraveled;
        private float _startOffset;
        private bool _hasReachedEnd;

        public void InitializePath(Route route, float startOffset)
        {
            _startOffset = startOffset;
            _pathPoints.Clear();
            _currentPointIndex = 0;
            _distanceTraveled = 0f;
            _hasReachedEnd = false;

            // Копируем все точки маршрута
            foreach (var node in route.Nodes)
            {
                _pathPoints.Add(node.Position);
            }

            // Если есть начальное смещение, "отматываем" грузовик назад по маршруту
            if (_startOffset > 0 && _pathPoints.Count > 1)
            {
                _distanceTraveled = -_startOffset;
            }
        }

        public void UpdateMovement(float deltaTime)
        {
            if (_hasReachedEnd || _pathPoints.Count < 2) return;

            // Двигаемся вперёд
            _distanceTraveled += MoveSpeed * deltaTime;

            // Находим текущую позицию на пути
            float accumulatedDistance = 0f;
            Vector3 newPosition = _pathPoints[0];
            Vector3 moveDirection = Vector3.forward;

            for (int i = 0; i < _pathPoints.Count - 1; i++)
            {
                Vector3 start = _pathPoints[i];
                Vector3 end = _pathPoints[i + 1];
                float segmentLength = Vector3.Distance(start, end);

                if (_distanceTraveled <= accumulatedDistance + segmentLength)
                {
                    // Мы на этом сегменте
                    float t = (_distanceTraveled - accumulatedDistance) / segmentLength;
                    t = Mathf.Clamp01(t);

                    newPosition = Vector3.Lerp(start, end, t);
                    moveDirection = (end - start).normalized;
                    _currentPointIndex = i;
                    break;
                }

                accumulatedDistance += segmentLength;

                // Если прошли весь путь
                if (i == _pathPoints.Count - 2)
                {
                    newPosition = end;
                    moveDirection = (end - start).normalized;
                    _hasReachedEnd = true;
                }
            }

            // Если ещё не начали двигаться (отрицательная дистанция из-за смещения)
            if (_distanceTraveled < 0)
            {
                // Остаёмся на стартовой позиции, но смотрим в направлении движения
                moveDirection = (_pathPoints[1] - _pathPoints[0]).normalized;
                newPosition = _pathPoints[0] - moveDirection * (-_distanceTraveled);
            }

            CurrentPosition = newPosition;

            // Обновляем позицию GameObject
            GameObject.transform.position = CurrentPosition + Vector3.up * 0.5f;

            // Плавный поворот в направлении движения
            if (moveDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                GameObject.transform.rotation = Quaternion.Slerp(
                    GameObject.transform.rotation,
                    targetRotation,
                    deltaTime * RotationSpeed
                );
            }
        }

        public bool HasReachedEnd => _hasReachedEnd;
    }
}