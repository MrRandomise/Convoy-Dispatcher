public class ChangeSpeedAction : ITriggerAction
{
    public string Id => "change_speed";
    private readonly SpeedMode _newSpeed;

    public ChangeSpeedAction(SpeedMode newSpeed)
    {
        _newSpeed = newSpeed;
    }

    public void Execute(TriggerContext context)
    {
        context.Convoy.Rules.Speed = _newSpeed;
        ServiceLocator.Get<IEventBus>().Publish(new ConvoySpeedChangedEvent
        {
            ConvoyId = context.Convoy.Id,
            NewSpeed = _newSpeed
        });
    }
}