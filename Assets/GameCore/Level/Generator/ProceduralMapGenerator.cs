using System;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralMapGenerator : MonoBehaviour
{
    [SerializeField] private MapGeneratorConfig _config;

    private System.Random _random;
    private Transform _mapRoot;
    private List<GameObject> _spawnedObjects = new();
    private List<Bounds> _occupiedAreas = new();

    public void Initialize()
    {
        if (_mapRoot == null)
        {
            _mapRoot = new GameObject("GeneratedMap").transform;
        }
    }

    public GeneratedMapData Generate(MapSize size, int seed = -1)
    {
        ClearMap();

        seed = seed == -1 ? Environment.TickCount : seed;
        _random = new System.Random(seed);

        var settings = _config.GetSettings(size);
        var mapData = new GeneratedMapData
        {
            Size = size,
            Seed = seed,
            Width = settings.Width,
            Length = settings.Length
        };

        // 1. Создаём землю
        CreateGround(settings);

        // 2. Генерируем основную дорогу (от начала к концу карты)
        mapData.MainRoadPath = GenerateMainRoad(settings);
        CreateRoadVisuals(mapData.MainRoadPath, false);

        // 3. Добавляем ответвления и альтернативные пути
        mapData.AlternativeRoads = GenerateAlternativeRoads(mapData.MainRoadPath, settings);
        foreach (var altRoad in mapData.AlternativeRoads)
        {
            CreateRoadVisuals(altRoad, true);
        }

        // 4. Размещаем здания вдоль дорог
        PlaceBuildings(mapData.MainRoadPath, settings);

        // 5. Добавляем декорации
        PlaceDecorations(settings);

        // 6. Освещение
        PlaceStreetLights(mapData.MainRoadPath, settings);

        // 7. Точки спавна и доставки
        mapData.SpawnPoint = mapData.MainRoadPath[0];
        mapData.DeliveryPoints = GenerateDeliveryPoints(mapData.MainRoadPath, settings);

        return mapData;
    }

    private void CreateGround(MapSizeSettings settings)
    {
        GameObject ground;

        if (_config.GroundPrefab != null)
        {
            ground = Instantiate(_config.GroundPrefab, _mapRoot);
        }
        else
        {
            ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.transform.parent = _mapRoot;

            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.25f, 0.2f, 0.15f); // Грязно-коричневый для пост-апокалипсиса
            ground.GetComponent<Renderer>().material = mat;
        }

        ground.name = "Ground";
        ground.transform.position = new Vector3(settings.Width / 2f, -0.1f, settings.Length / 2f);
        ground.transform.localScale = new Vector3(settings.Width / 10f + 5f, 1f, settings.Length / 10f + 5f);

        _spawnedObjects.Add(ground);
    }

    private List<Vector3> GenerateMainRoad(MapSizeSettings settings)
    {
        var path = new List<Vector3>();
        float segmentLength = settings.Length / settings.RoadSegments;
        float currentX = settings.Width / 2f;
        float roadWidth = 8f;

        // Начальная точка
        path.Add(new Vector3(currentX, 0f, 0f));

        for (int i = 1; i <= settings.RoadSegments; i++)
        {
            float z = i * segmentLength;

            // Добавляем небольшое отклонение для естественности
            float maxOffset = settings.Width * 0.3f;
            float xOffset = (float)(_random.NextDouble() * 2 - 1) * maxOffset * 0.3f;
            currentX = Mathf.Clamp(currentX + xOffset, roadWidth, settings.Width - roadWidth);

            // Иногда добавляем промежуточные точки для поворотов
            if (_random.NextDouble() < 0.4f && i < settings.RoadSegments)
            {
                float midZ = z - segmentLength * 0.5f;
                path.Add(new Vector3(currentX, 0f, midZ));
            }

            path.Add(new Vector3(currentX, 0f, z));
        }

        return path;
    }

    private List<List<Vector3>> GenerateAlternativeRoads(List<Vector3> mainRoad, MapSizeSettings settings)
    {
        var altRoads = new List<List<Vector3>>();
        int altCount = settings.RoadSegments / 2;

        for (int i = 0; i < altCount; i++)
        {
            int startIndex = _random.Next(0, mainRoad.Count - 2);
            int endIndex = _random.Next(startIndex + 2, mainRoad.Count);

            if (endIndex >= mainRoad.Count) endIndex = mainRoad.Count - 1;

            var altPath = new List<Vector3>();
            var startPoint = mainRoad[startIndex];
            var endPoint = mainRoad[endIndex];

            altPath.Add(startPoint);

            // Создаём обходной путь со смещением
            float offsetDirection = _random.NextDouble() > 0.5f ? 1f : -1f;
            float offset = 20f + (float)_random.NextDouble() * 30f;

            var midPoint = Vector3.Lerp(startPoint, endPoint, 0.5f);
            midPoint.x += offset * offsetDirection;
            midPoint.x = Mathf.Clamp(midPoint.x, 10f, settings.Width - 10f);

            altPath.Add(midPoint);
            altPath.Add(endPoint);

            altRoads.Add(altPath);
        }

        return altRoads;
    }

    private void CreateRoadVisuals(List<Vector3> path, bool isAlternative)
    {
        var roadParent = new GameObject(isAlternative ? "AlternativeRoad" : "MainRoad");
        roadParent.transform.parent = _mapRoot;
        _spawnedObjects.Add(roadParent);

        float roadWidth = isAlternative ? 5f : 8f;

        for (int i = 0; i < path.Count - 1; i++)
        {
            var start = path[i];
            var end = path[i + 1];

            CreateRoadSegment(start, end, roadWidth, roadParent.transform, isAlternative);
        }
    }

    private void CreateRoadSegment(Vector3 start, Vector3 end, float width, Transform parent, bool isAlternative)
    {
        GameObject segment;

        if (_config.StraightRoadPrefab != null && !isAlternative)
        {
            segment = Instantiate(_config.StraightRoadPrefab, parent);
        }
        else
        {
            segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
            segment.transform.parent = parent;

            var mat = isAlternative ? _config.DirtMaterial : _config.RoadMaterial;
            if (mat == null)
            {
                mat = new Material(Shader.Find("Standard"));
                mat.color = isAlternative ? new Color(0.4f, 0.35f, 0.25f) : new Color(0.2f, 0.2f, 0.2f);
            }
            segment.GetComponent<Renderer>().material = mat;
        }

        var direction = end - start;
        var length = direction.magnitude;
        var center = (start + end) / 2f;

        segment.transform.position = new Vector3(center.x, 0.01f, center.z);
        segment.transform.localScale = new Vector3(width, 0.1f, length + 0.5f);

        if (direction != Vector3.zero)
        {
            segment.transform.rotation = Quaternion.LookRotation(direction);
        }

        // Добавляем в занятые области
        var segmentBounds = new Bounds(center, new Vector3(width + 4f, 2f, length + 4f));
        _occupiedAreas.Add(segmentBounds);

        var collider = segment.GetComponent<Collider>();
        if (collider != null) Destroy(collider);
    }

    private void PlaceBuildings(List<Vector3> roadPath, MapSizeSettings settings)
    {
        int buildingCount = _random.Next(settings.MinBuildings, settings.MaxBuildings + 1);
        float buildingOffset = 15f;

        for (int i = 0; i < buildingCount; i++)
        {
            // Выбираем случайную точку вдоль дороги
            int segmentIndex = _random.Next(0, roadPath.Count - 1);
            float t = (float)_random.NextDouble();
            var roadPoint = Vector3.Lerp(roadPath[segmentIndex], roadPath[segmentIndex + 1], t);

            // Размещаем здание сбоку от дороги
            float side = _random.NextDouble() > 0.5f ? 1f : -1f;
            float distance = buildingOffset + (float)_random.NextDouble() * 20f;

            var direction = (roadPath[segmentIndex + 1] - roadPath[segmentIndex]).normalized;
            var perpendicular = new Vector3(-direction.z, 0, direction.x);

            var position = roadPoint + perpendicular * side * distance;
            position.x = Mathf.Clamp(position.x, 5f, settings.Width - 5f);
            position.z = Mathf.Clamp(position.z, 5f, settings.Length - 5f);

            // Проверяем пересечение с другими объектами
            var buildingBounds = new Bounds(position, new Vector3(12f, 20f, 12f));
            if (IsAreaOccupied(buildingBounds)) continue;

            CreateBuilding(position, _random.NextDouble() < 0.6f);
            _occupiedAreas.Add(buildingBounds);
        }
    }

    private void CreateBuilding(Vector3 position, bool isDestroyed)
    {
        GameObject building = null;
        GameObject[] prefabArray = isDestroyed ? _config.DestroyedBuildings : _config.IntactBuildings;

        if (prefabArray != null && prefabArray.Length > 0)
        {
            var prefab = prefabArray[_random.Next(prefabArray.Length)];
            if (prefab != null)
            {
                building = Instantiate(prefab, _mapRoot);
            }
        }

        if (building == null)
        {
            // Создаём процедурное здание
            building = CreateProceduralBuilding(isDestroyed);
        }

        building.transform.position = position;
        building.transform.rotation = Quaternion.Euler(0f, _random.Next(0, 4) * 90f, 0f);

        _spawnedObjects.Add(building);
    }

    private GameObject CreateProceduralBuilding(bool isDestroyed)
    {
        var building = new GameObject("Building");
        building.transform.parent = _mapRoot;

        float width = 6f + (float)_random.NextDouble() * 8f;
        float height = isDestroyed
            ? 5f + (float)_random.NextDouble() * 10f
            : 8f + (float)_random.NextDouble() * 15f;
        float depth = 6f + (float)_random.NextDouble() * 8f;

        var main = GameObject.CreatePrimitive(PrimitiveType.Cube);
        main.transform.parent = building.transform;
        main.transform.localPosition = new Vector3(0, height / 2f, 0);
        main.transform.localScale = new Vector3(width, height, depth);

        var mat = new Material(Shader.Find("Standard"));
        mat.color = isDestroyed
            ? new Color(0.3f + (float)_random.NextDouble() * 0.2f, 0.25f, 0.2f)
            : new Color(0.4f + (float)_random.NextDouble() * 0.2f, 0.4f, 0.35f);
        main.GetComponent<Renderer>().material = mat;

        Destroy(main.GetComponent<Collider>());

        // Для разрушенных зданий добавляем обломки
        if (isDestroyed && _random.NextDouble() < 0.5f)
        {
            var debris = GameObject.CreatePrimitive(PrimitiveType.Cube);
            debris.transform.parent = building.transform;
            debris.transform.localPosition = new Vector3(
                (float)_random.NextDouble() * 4f - 2f,
                1f,
                (float)_random.NextDouble() * 4f - 2f
            );
            debris.transform.localScale = new Vector3(
                (float)_random.NextDouble() * 3f + 1f,
                (float)_random.NextDouble() * 2f + 0.5f,
                (float)_random.NextDouble() * 3f + 1f
            );
            debris.transform.rotation = Quaternion.Euler(
                (float)_random.NextDouble() * 20f,
                (float)_random.NextDouble() * 360f,
                (float)_random.NextDouble() * 15f
            );
            debris.GetComponent<Renderer>().material = mat;
            Destroy(debris.GetComponent<Collider>());
        }

        return building;
    }

    private void PlaceDecorations(MapSizeSettings settings)
    {
        int decorCount = _random.Next(settings.MinDecorations, settings.MaxDecorations + 1);

        for (int i = 0; i < decorCount; i++)
        {
            var position = new Vector3(
                (float)_random.NextDouble() * settings.Width,
                0f,
                (float)_random.NextDouble() * settings.Length
            );

            var decorBounds = new Bounds(position, new Vector3(4f, 4f, 4f));
            if (IsAreaOccupied(decorBounds)) continue;

            CreateDecoration(position);
        }
    }

    private void CreateDecoration(Vector3 position)
    {
        // Выбираем тип декорации
        float roll = (float)_random.NextDouble();
        GameObject decoration = null;

        if (roll < 0.3f && _config.Containers != null && _config.Containers.Length > 0)
        {
            var prefab = _config.Containers[_random.Next(_config.Containers.Length)];
            if (prefab != null) decoration = Instantiate(prefab, _mapRoot);
        }
        else if (roll < 0.5f && _config.Debris != null && _config.Debris.Length > 0)
        {
            var prefab = _config.Debris[_random.Next(_config.Debris.Length)];
            if (prefab != null) decoration = Instantiate(prefab, _mapRoot);
        }
        else if (roll < 0.65f && _config.Vehicles != null && _config.Vehicles.Length > 0)
        {
            var prefab = _config.Vehicles[_random.Next(_config.Vehicles.Length)];
            if (prefab != null) decoration = Instantiate(prefab, _mapRoot);
        }

        if (decoration == null)
        {
            // Создаём простой объект обломков
            decoration = CreateProceduralDebris();
        }

        decoration.transform.position = position;
        decoration.transform.rotation = Quaternion.Euler(0f, (float)_random.NextDouble() * 360f, 0f);

        _spawnedObjects.Add(decoration);
    }

    private GameObject CreateProceduralDebris()
    {
        var debris = GameObject.CreatePrimitive(PrimitiveType.Cube);
        debris.transform.parent = _mapRoot;

        float size = 0.5f + (float)_random.NextDouble() * 2f;
        debris.transform.localScale = new Vector3(size, size * 0.5f, size);
        debris.transform.rotation = Quaternion.Euler(
            (float)_random.NextDouble() * 10f,
            (float)_random.NextDouble() * 360f,
            (float)_random.NextDouble() * 10f
        );

        var mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.35f, 0.3f, 0.25f);
        debris.GetComponent<Renderer>().material = mat;

        Destroy(debris.GetComponent<Collider>());
        return debris;
    }

    private void PlaceStreetLights(List<Vector3> roadPath, MapSizeSettings settings)
    {
        float lightSpacing = 20f;
        float lightOffset = 5f;

        for (int i = 0; i < roadPath.Count - 1; i++)
        {
            var start = roadPath[i];
            var end = roadPath[i + 1];
            var direction = (end - start).normalized;
            var perpendicular = new Vector3(-direction.z, 0, direction.x);
            var segmentLength = Vector3.Distance(start, end);

            int lightCount = Mathf.FloorToInt(segmentLength / lightSpacing);

            for (int j = 0; j < lightCount; j++)
            {
                if (_random.NextDouble() > _config.LightDensity) continue;

                float t = (j + 0.5f) / lightCount;
                var roadPoint = Vector3.Lerp(start, end, t);

                // Размещаем фонарь по сторонам дороги
                float side = j % 2 == 0 ? 1f : -1f;
                var lightPos = roadPoint + perpendicular * side * lightOffset;

                CreateStreetLight(lightPos, Quaternion.LookRotation(direction));
            }
        }
    }

    private void CreateStreetLight(Vector3 position, Quaternion rotation)
    {
        GameObject light = null;

        if (_config.StreetLights != null && _config.StreetLights.Length > 0)
        {
            var prefab = _config.StreetLights[_random.Next(_config.StreetLights.Length)];
            if (prefab != null) light = Instantiate(prefab, _mapRoot);
        }

        if (light == null)
        {
            // Создаём процедурный фонарь
            light = new GameObject("StreetLight");
            light.transform.parent = _mapRoot;

            var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.transform.parent = light.transform;
            pole.transform.localPosition = new Vector3(0, 3f, 0);
            pole.transform.localScale = new Vector3(0.2f, 3f, 0.2f);

            var poleMat = new Material(Shader.Find("Standard"));
            poleMat.color = new Color(0.3f, 0.3f, 0.3f);
            pole.GetComponent<Renderer>().material = poleMat;
            Destroy(pole.GetComponent<Collider>());

            var lamp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lamp.transform.parent = light.transform;
            lamp.transform.localPosition = new Vector3(0, 6.5f, 0);
            lamp.transform.localScale = new Vector3(0.6f, 0.4f, 0.6f);

            var lampMat = new Material(Shader.Find("Standard"));
            lampMat.color = new Color(1f, 0.9f, 0.7f);
            lampMat.EnableKeyword("_EMISSION");
            lampMat.SetColor("_EmissionColor", new Color(1f, 0.8f, 0.5f) * 2f);
            lamp.GetComponent<Renderer>().material = lampMat;
            Destroy(lamp.GetComponent<Collider>());

            // Добавляем свет (если много фонарей, можно отключить для производительности)
            var pointLight = new GameObject("Light").AddComponent<Light>();
            pointLight.transform.parent = light.transform;
            pointLight.transform.localPosition = new Vector3(0, 6f, 0);
            pointLight.type = LightType.Point;
            pointLight.color = new Color(1f, 0.85f, 0.6f);
            pointLight.intensity = 1.5f;
            pointLight.range = 15f;
        }

        light.transform.position = position;
        _spawnedObjects.Add(light);
    }

    private List<Vector3> GenerateDeliveryPoints(List<Vector3> mainRoad, MapSizeSettings settings)
    {
        var deliveryPoints = new List<Vector3>();
        int count = Mathf.Max(1, settings.RoadSegments / 2);

        for (int i = 0; i < count; i++)
        {
            int index = Mathf.Min(mainRoad.Count - 1, (i + 1) * mainRoad.Count / (count + 1));
            deliveryPoints.Add(mainRoad[index]);
        }

        // Последняя точка - конец маршрута
        if (deliveryPoints.Count == 0 || deliveryPoints[deliveryPoints.Count - 1] != mainRoad[mainRoad.Count - 1])
        {
            deliveryPoints.Add(mainRoad[mainRoad.Count - 1]);
        }

        return deliveryPoints;
    }

    private bool IsAreaOccupied(Bounds bounds)
    {
        foreach (var occupied in _occupiedAreas)
        {
            if (bounds.Intersects(occupied))
                return true;
        }
        return false;
    }

    public void ClearMap()
    {
        foreach (var obj in _spawnedObjects)
        {
            if (obj != null)
            {
                if (Application.isPlaying)
                    Destroy(obj);
                else
                    DestroyImmediate(obj);
            }
        }
        _spawnedObjects.Clear();
        _occupiedAreas.Clear();
    }
}
