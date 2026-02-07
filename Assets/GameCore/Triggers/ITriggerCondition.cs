public interface ITriggerCondition
{
    string Id { get; }
    bool Evaluate(TriggerContext context);
}