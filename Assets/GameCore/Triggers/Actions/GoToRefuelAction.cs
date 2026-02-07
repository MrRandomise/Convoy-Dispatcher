public class GoToRefuelAction : ITriggerAction
{
    public string Id => "go_to_refuel";

    public void Execute(TriggerContext context)
    {
        ServiceLocator.Get<IEventBus>().Publish(new RefuelRequestedEvent
        {
            ConvoyId = context.Convoy.Id
        });
    }
}