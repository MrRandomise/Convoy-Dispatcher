using System;
using System.Collections.Generic;

[Serializable]
public class SimulationResult
{
    public bool IsSuccess;
    public float TotalTime;
    public float FuelUsed;
    public int TrucksLost;
    public int ConvoysLost;
    public float TotalDamage;
    public int CoinsEarned;
    public List<string> CompletedDeliveries = new();
    public List<string> FailedConditions = new();
}