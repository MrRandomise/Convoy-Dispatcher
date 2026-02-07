using System;

public interface ITutorialSystem
{
    void StartTutorial();
    void NextStep();
    void CompleteTutorial();
    bool IsActive { get; }
    int CurrentStep { get; }
    event Action<TutorialStep> OnStepChanged;
}