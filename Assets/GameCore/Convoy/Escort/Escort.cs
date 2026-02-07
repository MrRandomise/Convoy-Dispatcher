using UnityEngine;

public class Escort : IEscort
{
    public string Id { get; }
    public EscortType Type { get; }
    public EscortStats Stats { get; }
    public float CurrentHealth { get; private set; }
    public Vector3 Position { get; set; }

    public Escort(string id, EscortType type, EscortStats stats)
    {
        Id = id;
        Type = type;
        Stats = stats;
        CurrentHealth = stats.MaxHealth;
    }

    public void TakeDamage(float amount)
    {
        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
    }

    public void Attack(Vector3 target)
    {
        // Логика атаки реализуется в системе боя
        ServiceLocator.Get<IEventBus>().Publish(new EscortAttackEvent
        {
            EscortId = Id,
            TargetPosition = target,
            Damage = Stats.AttackDamage
        });
    }
}