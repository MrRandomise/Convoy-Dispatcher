public class TriggerContext
{
    public IConvoy Convoy { get; set; }
    public float CurrentFuel { get; set; }
    public float MaxFuel { get; set; }
    public bool HasObstacleAhead { get; set; }
    public bool IsUnderAttack { get; set; }
    public bool IsConvoyLagging { get; set; }
    public float DistanceToNextPoint { get; set; }
    public RoadCondition RoadCondition { get; set; }
}