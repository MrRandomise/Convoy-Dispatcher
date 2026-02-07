public class RoadCongestedCondition : ITriggerCondition
{
    public string Id => "road_congested";

    public bool Evaluate(TriggerContext context)
    {
        return context.RoadCondition == RoadCondition.Congested;
    }
}