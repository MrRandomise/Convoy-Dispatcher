public interface ITriggerSystem
{
    void RegisterTrigger(string convoyId, ConvoyTrigger trigger);
    void RemoveTrigger(string convoyId, string triggerId);
    void EvaluateTriggers(string convoyId, TriggerContext context);
    void ClearTriggers(string convoyId);
}