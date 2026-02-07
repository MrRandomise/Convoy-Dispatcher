using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MapGeneratorConfig", menuName = "Game/Map Generator Config")]
public class MapGeneratorConfig : ScriptableObject
{
    [Header("Размеры карт")]
    public MapSizeSettings SmallMap = new() { Width = 100f, Length = 150f, RoadSegments = 3 };
    public MapSizeSettings MediumMap = new() { Width = 150f, Length = 250f, RoadSegments = 5 };
    public MapSizeSettings LargeMap = new() { Width = 200f, Length = 400f, RoadSegments = 8 };

    [Header("Префабы зданий")]
    public GameObject[] DestroyedBuildings;
    public GameObject[] IntactBuildings;
    public GameObject[] Ruins;

    [Header("Префабы дорог")]
    public GameObject StraightRoadPrefab;
    public GameObject CrossroadPrefab;
    public GameObject TurnRoadPrefab;
    public GameObject BridgePrefab;

    [Header("Декорации")]
    public GameObject[] StreetLights;
    public GameObject[] Containers;
    public GameObject[] Debris;
    public GameObject[] Billboards;
    public GameObject[] Vehicles;

    [Header("Окружение")]
    public GameObject GroundPrefab;
    public Material RoadMaterial;
    public Material DirtMaterial;

    [Header("Настройки генерации")]
    [Range(0f, 1f)] public float BuildingDensity = 0.7f;
    [Range(0f, 1f)] public float DebrisDensity = 0.5f;
    [Range(0f, 1f)] public float LightDensity = 0.3f;

    public MapSizeSettings GetSettings(MapSize size)
    {
        return size switch
        {
            MapSize.Small => SmallMap,
            MapSize.Medium => MediumMap,
            MapSize.Large => LargeMap,
            _ => MediumMap
        };
    }
}

[Serializable]
public class MapSizeSettings
{
    public float Width = 150f;
    public float Length = 250f;
    public int RoadSegments = 5;
    public int MinBuildings = 10;
    public int MaxBuildings = 30;
    public int MinDecorations = 20;
    public int MaxDecorations = 50;
}