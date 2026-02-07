using System;
using UnityEngine;

[Serializable]
public class ConvoySimulationState
{
    public string ConvoyId;
    public string CurrentRouteId;
    public int CurrentNodeIndex;
    public Vector3 Position;
    public float CurrentSpeed;
    public float CurrentFuel;
    public float TotalDamage;
    public float CargoDelivered;
    public ConvoyBehaviorState BehaviorState;
    public float WaitTimer;
}