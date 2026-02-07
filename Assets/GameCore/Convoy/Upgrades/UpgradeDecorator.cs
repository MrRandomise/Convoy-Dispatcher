public abstract class UpgradeDecorator : IUpgrade
{
    public abstract string Id { get; }
    public abstract void Apply(ConvoyStats stats);
    public abstract void Remove(ConvoyStats stats);
}

public class ArmorUpgrade : UpgradeDecorator
{
    public override string Id => "armor_plate";
    public override void Apply(ConvoyStats stats) => stats.Health += 25;
    public override void Remove(ConvoyStats stats) => stats.Health -= 25;
}

public class TurboEngineUpgrade : UpgradeDecorator
{
    public override string Id => "turbo_engine";
    public override void Apply(ConvoyStats stats) => stats.Speed += 15;
    public override void Remove(ConvoyStats stats) => stats.Speed -= 15;
}

public class ReinforcedTiresUpgrade : UpgradeDecorator
{
    public override string Id => "reinforced_tires";
    public override void Apply(ConvoyStats stats) => stats.Passability += 0.3f;
    public override void Remove(ConvoyStats stats) => stats.Passability -= 0.3f;
}