using System;

[Serializable]
public class ConvoyTrigger
{
    public string Id;
    public ITriggerCondition Condition;
    public ITriggerAction Action;
    public bool IsOneShot;
    public bool HasFired { get; private set; }

    public ConvoyTrigger(string id, ITriggerCondition condition, ITriggerAction action, bool oneShot = false)
    {
        Id = id;
        Condition = condition;
        Action = action;
        IsOneShot = oneShot;
    }

    public bool TryExecute(TriggerContext context)
    {
        if (IsOneShot && HasFired) return false;

        if (Condition.Evaluate(context))
        {
            Action.Execute(context);
            HasFired = true;
            return true;
        }
        return false;
    }

    public void Reset() => HasFired = false;
}