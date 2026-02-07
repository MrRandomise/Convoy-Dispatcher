using System;
using System.Collections.Generic;

[Serializable]
public class LevelData
{
    public string Id;
    public int Seed;
    public DifficultyType Difficulty;
    public LevelParameters Parameters;
    public List<Route> Routes = new();
    public List<MapEvent> Events = new();
    public List<DeliveryPoint> DeliveryPoints = new();
    public SpawnPoint BaseSpawnPoint;
    public MapBounds Bounds;
}

[Serializable]
public class SpawnPoint
{
    public UnityEngine.Vector3 Position;
    public float Rotation;
}

[Serializable]
public class MapBounds
{
    public float MinX, MaxX;
    public float MinZ, MaxZ;
}