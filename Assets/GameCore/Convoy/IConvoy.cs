using System.Collections.Generic;
using UnityEngine.SocialPlatforms;

public interface IConvoy
{
    string Id { get; }
    ConvoyStats Stats { get; }
    List<ITruck> Trucks { get; }
    List<IEscort> Escorts { get; }
    ConvoyRules Rules { get; set; }
    void AddUpgrade(IUpgrade upgrade);
    bool CanAddUpgrade();
}