using System;
using UnityEngine;

[Serializable]
public class MapEvent
{
    public string Id;
    public MapEventType Type;
    public Vector3 Position;
    public float Radius;
    public float TriggerTime;      // -1 = всегда активно
    public float Duration;
    public EventSeverity Severity;
}


