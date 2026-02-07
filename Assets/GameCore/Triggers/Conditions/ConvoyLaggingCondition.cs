public class ConvoyLaggingCondition : ITriggerCondition
{
    public string Id => "convoy_lagging";

    public bool Evaluate(TriggerContext context)
    {
        return context.IsConvoyLagging;
    }
}