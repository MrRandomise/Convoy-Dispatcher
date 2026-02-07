public class ConvoyFactory : IConvoyFactory
{
    public IConvoy Create(ConvoyType type)
    {
        return type switch
        {
            ConvoyType.Light => new Convoy("light", new ConvoyStats
            {
                Health = 80,
                Speed = 70,
                FuelCapacity = 80
            }),
            ConvoyType.Standard => new Convoy("standard", new ConvoyStats()),
            ConvoyType.Heavy => new Convoy("heavy", new ConvoyStats
            {
                Health = 150,
                Speed = 35,
                FuelCapacity = 150
            }),
            _ => new Convoy("standard", new ConvoyStats())
        };
    }
}