using System;
using UnityEngine;

[Serializable]
public class DeliveryPoint
{
    public string Id;
    public string Name;
    public Vector3 Position;
    public float RequiredCargo;
    public float TimeBonus;        // Бонус за раннюю доставку
    public bool IsOptional;
}