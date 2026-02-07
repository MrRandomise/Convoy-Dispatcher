public class EmergencyStopAction : ITriggerAction
{
    public string Id => "emergency_stop";

    public void Execute(TriggerContext context)
    {
        ServiceLocator.Get<IEventBus>().Publish(new EmergencyStopEvent
        {
            ConvoyId = context.Convoy.Id
        });
    }
}