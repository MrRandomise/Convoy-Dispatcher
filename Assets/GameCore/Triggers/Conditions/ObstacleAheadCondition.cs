public class ObstacleAheadCondition : ITriggerCondition
{
    public string Id => "obstacle_ahead";

    public bool Evaluate(TriggerContext context)
    {
        return context.HasObstacleAhead;
    }
}