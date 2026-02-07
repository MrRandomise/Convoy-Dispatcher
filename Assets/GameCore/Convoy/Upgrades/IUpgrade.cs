public interface IUpgrade
{
    string Id { get; }
    void Apply(ConvoyStats stats);
    void Remove(ConvoyStats stats);
}