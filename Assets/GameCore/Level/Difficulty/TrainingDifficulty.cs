public class TrainingDifficulty : IDifficultyStrategy
{
    public LevelParameters GenerateParameters() => new LevelParameters
    {
        DeliveryPoints = 1,
        EventsCount = 1,
        TimeLimit = 300,
        FuelBudget = 200,
        AllowIntervention = true,
        Conditions = new[] { VictoryCondition.DeliverAllCargo }
    };
}