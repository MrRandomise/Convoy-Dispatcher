using System;

[Serializable]
public class ConvoyRules
{
    public DistanceMode Distance = DistanceMode.Standard;
    public SpeedMode Speed = SpeedMode.Normal;
    public PriorityMode Priority = PriorityMode.Balanced;
    public ThreatBehavior ThreatResponse = ThreatBehavior.Wait;
}

public enum DistanceMode { Tight, Standard, Far }
public enum SpeedMode { Slow, Normal, Fast }
public enum PriorityMode { Safety, Balanced, Time }
public enum ThreatBehavior { Stop, Evade, Wait }