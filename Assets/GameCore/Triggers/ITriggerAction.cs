public interface ITriggerAction
{
    string Id { get; }
    void Execute(TriggerContext context);
}