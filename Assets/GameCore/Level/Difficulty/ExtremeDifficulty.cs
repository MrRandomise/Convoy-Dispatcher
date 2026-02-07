public class ExtremeDifficulty : IDifficultyStrategy
{
    public LevelParameters GenerateParameters() => new LevelParameters
    {
        DeliveryPoints = 3,
        EventsCount = 12,
        TimeLimit = 150,
        FuelBudget = 80,
        AllowIntervention = false,
        Conditions = new[]
        {
            VictoryCondition.DeliverAllCargo,
            VictoryCondition.NoTruckLoss,
            VictoryCondition.WithinTimeLimit,
            VictoryCondition.WithinFuelBudget,
            VictoryCondition.NoDamage
        }
    };
}