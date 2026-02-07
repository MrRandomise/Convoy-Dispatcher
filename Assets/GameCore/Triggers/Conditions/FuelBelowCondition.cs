public class FuelBelowCondition : ITriggerCondition
{
    public string Id => "fuel_below";
    private readonly float _threshold;

    public FuelBelowCondition(float thresholdPercent)
    {
        _threshold = thresholdPercent;
    }

    public bool Evaluate(TriggerContext context)
    {
        var fuelPercent = (context.CurrentFuel / context.MaxFuel) * 100f;
        return fuelPercent < _threshold;
    }
}