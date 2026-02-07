using System.Collections.Generic;
using UnityEngine;

public class GeneratedMapData
{
    public MapSize Size;
    public int Seed;
    public float Width;
    public float Length;
    public List<Vector3> MainRoadPath = new();
    public List<List<Vector3>> AlternativeRoads = new();
    public Vector3 SpawnPoint;
    public List<Vector3> DeliveryPoints = new();
}