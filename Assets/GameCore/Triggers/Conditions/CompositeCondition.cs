using System.Collections.Generic;
using System.Linq;

public class AndCondition : ITriggerCondition
{
    public string Id => "and_composite";
    private readonly List<ITriggerCondition> _conditions;

    public AndCondition(params ITriggerCondition[] conditions)
    {
        _conditions = conditions.ToList();
    }

    public bool Evaluate(TriggerContext context)
    {
        return _conditions.All(c => c.Evaluate(context));
    }
}

public class OrCondition : ITriggerCondition
{
    public string Id => "or_composite";
    private readonly List<ITriggerCondition> _conditions;

    public OrCondition(params ITriggerCondition[] conditions)
    {
        _conditions = conditions.ToList();
    }

    public bool Evaluate(TriggerContext context)
    {
        return _conditions.Any(c => c.Evaluate(context));
    }
}