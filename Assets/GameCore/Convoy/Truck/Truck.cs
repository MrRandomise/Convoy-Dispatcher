using UnityEngine;

public class Truck : ITruck
{
    public string Id { get; }
    public TruckStats Stats { get; }
    public float CurrentHealth { get; private set; }
    public float CurrentFuel { get; private set; }
    public float CargoCapacity => Stats.CargoCapacity;
    public float CurrentCargo { get; private set; }
    public Vector3 Position { get; set; }

    public Truck(string id, TruckStats stats)
    {
        Id = id;
        Stats = stats;
        CurrentHealth = stats.MaxHealth;
        CurrentFuel = stats.MaxFuel;
    }

    public void TakeDamage(float amount)
    {
        CurrentHealth = Mathf.Max(0, CurrentHealth - Mathf.Max(0, amount - Stats.Armor));
    }

    public void Refuel(float amount)
    {
        CurrentFuel = Mathf.Min(Stats.MaxFuel, CurrentFuel + amount);
    }

    public void LoadCargo(float amount)
    {
        CurrentCargo = Mathf.Min(CargoCapacity, CurrentCargo + amount);
    }

    public void UnloadCargo(float amount)
    {
        CurrentCargo = Mathf.Max(0, CurrentCargo - amount);
    }
}