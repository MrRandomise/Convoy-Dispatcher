using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSystem : ITutorialSystem
{
    private List<TutorialStep> _steps;
    public bool IsActive { get; private set; }
    public int CurrentStep { get; private set; }
    public event Action<TutorialStep> OnStepChanged;

    public TutorialSystem()
    {
        _steps = new List<TutorialStep>(Resources.LoadAll<TutorialStep>("Tutorial"));
    }

    public void StartTutorial()
    {
        IsActive = true;
        CurrentStep = ServiceLocator.Get<ISaveSystem>().GetData().CurrentTutorialStep;
        if (CurrentStep < _steps.Count)
        {
            OnStepChanged?.Invoke(_steps[CurrentStep]);
        }
    }

    public void NextStep()
    {
        CurrentStep++;
        var saveData = ServiceLocator.Get<ISaveSystem>().GetData();
        saveData.CurrentTutorialStep = CurrentStep;

        if (CurrentStep >= _steps.Count)
        {
            CompleteTutorial();
            return;
        }
        OnStepChanged?.Invoke(_steps[CurrentStep]);
    }

    public void CompleteTutorial()
    {
        IsActive = false;
        var saveData = ServiceLocator.Get<ISaveSystem>().GetData();
        saveData.TutorialCompleted = true;
        ServiceLocator.Get<ISaveSystem>().Save();
    }
}