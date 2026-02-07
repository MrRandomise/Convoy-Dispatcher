using UnityEngine;

public class DebugUI : MonoBehaviour
{
    private ISimulationSystem _simulation;
    private LevelData _currentLevel;

    private void Start()
    {
        _simulation = ServiceLocator.Get<ISimulationSystem>();
    }

    public void SetLevelData(LevelData level)
    {
        _currentLevel = level;
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 350, 300));

        GUI.Box(new Rect(0, 0, 350, 300), "");

        GUILayout.Label("<b>Convoy Dispatch - Debug</b>");
        GUILayout.Space(5);

        GUILayout.Label($"Состояние: <color=yellow>{_simulation.State}</color>");
        GUILayout.Label($"Время: {_simulation.CurrentTime:F1}s");
        GUILayout.Label($"Масштаб времени: {_simulation.TimeScale:F1}x");

        GUILayout.Space(10);
        GUILayout.Label("<b>Управление:</b>");
        GUILayout.Label("ПРОБЕЛ - Старт/Пауза");
        GUILayout.Label("R - Перезапустить");

        GUILayout.Space(10);

        // Кнопки управления скоростью
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("0.5x")) _simulation.TimeScale = 0.5f;
        if (GUILayout.Button("1x")) _simulation.TimeScale = 1f;
        if (GUILayout.Button("2x")) _simulation.TimeScale = 2f;
        if (GUILayout.Button("5x")) _simulation.TimeScale = 5f;
        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }
}