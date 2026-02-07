using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int Coins;
    public string Language = "ru";
    public List<ConvoySaveData> OwnedConvoys = new();
    public List<string> CompletedLevels = new();
    public bool TutorialCompleted;
    public int CurrentTutorialStep;
}

[Serializable]
public class ConvoySaveData
{
    public string Id;
    public List<string> InstalledUpgrades = new();
    public int UpgradeSlots = 15;
}