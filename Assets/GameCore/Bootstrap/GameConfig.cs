using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Game Config")]
public class GameConfig : ScriptableObject
{
    [Header("Convoy Settings")]
    public int MaxUpgradesPerTruck = 15;
    public float BaseConvoySpeed = 50f;
    public float BaseFuelConsumption = 0.1f;

    [Header("Economy")]
    public int StartingCoins = 1000;
    public int LevelCompletionReward = 100;

    [Header("Gameplay")]
    public float SimulationSpeedMultiplier = 1f;
    public bool AllowPauseInSimulation = true;
}