using UnityEngine;

public interface ITruck
{
    string Id { get; }
    TruckStats Stats { get; }
    float CurrentHealth { get; }
    float CurrentFuel { get; }
    float CargoCapacity { get; }
    float CurrentCargo { get; }
    Vector3 Position { get; }
    
    void TakeDamage(float amount);
    void Refuel(float amount);
    void LoadCargo(float amount);
    void UnloadCargo(float amount);
}