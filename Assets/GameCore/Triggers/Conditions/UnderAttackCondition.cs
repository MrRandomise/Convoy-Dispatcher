public class UnderAttackCondition : ITriggerCondition
{
    public string Id => "under_attack";

    public bool Evaluate(TriggerContext context)
    {
        return context.IsUnderAttack;
    }
}