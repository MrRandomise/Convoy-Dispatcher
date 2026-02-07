using System;
using System.Collections.Generic;

public interface IPlanningUI
{
    event Action OnStartSimulation;
    event Action<string, ConvoyRules> OnConvoyRulesChanged;
    event Action<string, ConvoyTrigger> OnTriggerAdded;
    
    void Initialize(LevelData levelData, List<IConvoy> availableConvoys);
    void Show();
    void Hide();
}