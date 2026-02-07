using UnityEngine;

public interface IEscort
{
    string Id { get; }
    EscortType Type { get; }
    EscortStats Stats { get; }
    float CurrentHealth { get; }
    Vector3 Position { get; }

    void TakeDamage(float amount);
    void Attack(Vector3 target);
}
