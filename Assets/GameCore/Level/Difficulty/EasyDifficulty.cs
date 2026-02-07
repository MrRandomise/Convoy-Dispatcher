public class EasyDifficulty : IDifficultyStrategy
{
    public LevelParameters GenerateParameters() => new LevelParameters
    {
        DeliveryPoints = 1,
        EventsCount = 3,
        TimeLimit = 240,
        FuelBudget = 150,
        AllowIntervention = true,
        Conditions = new[] { VictoryCondition.DeliverAllCargo, VictoryCondition.WithinTimeLimit }
    };
}