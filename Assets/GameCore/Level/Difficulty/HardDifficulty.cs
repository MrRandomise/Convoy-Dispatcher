public class HardDifficulty : IDifficultyStrategy
{
    public LevelParameters GenerateParameters() => new LevelParameters
    {
        DeliveryPoints = 3,
        EventsCount = 8,
        TimeLimit = 180,
        FuelBudget = 100,
        AllowIntervention = false,
        Conditions = new[]
        {
            VictoryCondition.DeliverAllCargo,
            VictoryCondition.NoTruckLoss,
            VictoryCondition.WithinTimeLimit,
            VictoryCondition.WithinFuelBudget
        }
    };
}