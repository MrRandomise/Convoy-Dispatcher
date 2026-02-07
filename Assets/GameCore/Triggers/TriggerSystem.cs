using System.Collections.Generic;

public class TriggerSystem : ITriggerSystem
{
    private readonly Dictionary<string, List<ConvoyTrigger>> _convoyTriggers = new();

    public void RegisterTrigger(string convoyId, ConvoyTrigger trigger)
    {
        if (!_convoyTriggers.ContainsKey(convoyId))
        {
            _convoyTriggers[convoyId] = new List<ConvoyTrigger>();
        }
        _convoyTriggers[convoyId].Add(trigger);
    }

    public void RemoveTrigger(string convoyId, string triggerId)
    {
        if (_convoyTriggers.TryGetValue(convoyId, out var triggers))
        {
            triggers.RemoveAll(t => t.Id == triggerId);
        }
    }

    public void EvaluateTriggers(string convoyId, TriggerContext context)
    {
        if (!_convoyTriggers.TryGetValue(convoyId, out var triggers)) return;

        foreach (var trigger in triggers.ToArray())
        {
            if (trigger.TryExecute(context))
            {
                ServiceLocator.Get<IEventBus>().Publish(new TriggerActivatedEvent
                {
                    TriggerId = trigger.Id,
                    ConvoyId = convoyId
                });

                if (trigger.IsOneShot && trigger.HasFired)
                {
                    triggers.Remove(trigger);
                }
            }
        }
    }

    public void ClearTriggers(string convoyId)
    {
        if (_convoyTriggers.ContainsKey(convoyId))
        {
            _convoyTriggers[convoyId].Clear();
        }
    }
}