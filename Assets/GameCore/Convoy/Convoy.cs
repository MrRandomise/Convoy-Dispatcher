using System.Collections.Generic;

public class Convoy : IConvoy
{
    public string Id { get; }
    public ConvoyStats Stats { get; }
    public List<ITruck> Trucks { get; } = new();
    public List<IEscort> Escorts { get; } = new();
    public ConvoyRules Rules { get; set; } = new();

    private readonly List<IUpgrade> _upgrades = new();
    private readonly int _maxUpgradeSlots;

    public Convoy(string id, ConvoyStats stats, int maxUpgradeSlots = 15)
    {
        Id = id;
        Stats = stats;
        _maxUpgradeSlots = maxUpgradeSlots;
    }

    public void AddUpgrade(IUpgrade upgrade)
    {
        if (!CanAddUpgrade()) return;

        upgrade.Apply(Stats);
        _upgrades.Add(upgrade);
    }

    public bool CanAddUpgrade() => _upgrades.Count < _maxUpgradeSlots;

    public void AddTruck(ITruck truck) => Trucks.Add(truck);

    public void AddEscort(IEscort escort) => Escorts.Add(escort);

    public void RemoveUpgrade(IUpgrade upgrade)
    {
        if (_upgrades.Remove(upgrade))
        {
            upgrade.Remove(Stats);
        }
    }
}