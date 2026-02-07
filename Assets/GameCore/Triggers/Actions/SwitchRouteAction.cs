public class SwitchRouteAction : ITriggerAction
{
    public string Id => "switch_route";
    private readonly string _targetRouteId;

    public SwitchRouteAction(string targetRouteId)
    {
        _targetRouteId = targetRouteId;
    }

    public void Execute(TriggerContext context)
    {
        ServiceLocator.Get<IEventBus>().Publish(new RouteChangeRequestedEvent
        {
            ConvoyId = context.Convoy.Id,
            NewRouteId = _targetRouteId
        });
    }
}