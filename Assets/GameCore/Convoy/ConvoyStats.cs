using System;

[Serializable]
public class ConvoyStats
{
    public float Health = 100f;
    public float Speed = 10f;        // Единиц в секунду (синхронизировано с визуализацией)
    public float FuelCapacity = 100f;
    public float Maneuverability = 1f;
    public float Passability = 1f;
}