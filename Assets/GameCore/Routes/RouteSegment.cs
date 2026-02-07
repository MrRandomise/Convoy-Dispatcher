using System;
using UnityEngine;

[Serializable]
public class RouteSegment
{
    public RouteNode Start;
    public RouteNode End;
    public RoadCondition Condition = RoadCondition.Normal;
    public float Width = 2f;           // Ширина дороги
    public bool IsBridge;
    public bool IsHazardous;           // Опасная зона
    public TimeWindow AccessWindow;    // Временное окно доступа
    
    public float Length => Vector3.Distance(Start.Position, End.Position);
}

[Serializable]
public class TimeWindow
{
    public float OpenTime;      // Секунда симуляции когда открывается
    public float CloseTime;     // Секунда симуляции когда закрывается
    public float CycleDuration; // Период цикла (0 = без цикла)

    public bool IsOpen(float currentTime)
    {
        if (CycleDuration <= 0)
        {
            return currentTime >= OpenTime && currentTime <= CloseTime;
        }
        
        var cycleTime = currentTime % CycleDuration;
        return cycleTime >= OpenTime && cycleTime <= CloseTime;
    }
}