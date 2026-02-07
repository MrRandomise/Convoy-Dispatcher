public class NormalDifficulty : IDifficultyStrategy
{
    public LevelParameters GenerateParameters() => new LevelParameters
    {
        DeliveryPoints = 2,
        EventsCount = 5,
        TimeLimit = 200,
        FuelBudget = 120,
        AllowIntervention = false,
        Conditions = new[]
        {
            VictoryCondition.DeliverAllCargo,
            VictoryCondition.WithinTimeLimit,
            VictoryCondition.NoConvoyLoss
        }
    };
}