using System;
using UnityEngine;

public interface ISimulationSystem
{
    SimulationState State { get; }
    float CurrentTime { get; }
    float TimeScale { get; set; }

    void StartSimulation(LevelData levelData);
    void PauseSimulation();
    void ResumeSimulation();
    void StopSimulation();
    void Update(float deltaTime);

    event Action<SimulationState> OnStateChanged;
    event Action<float> OnTimeUpdated;
    event Action<SimulationResult> OnSimulationEnded;
}