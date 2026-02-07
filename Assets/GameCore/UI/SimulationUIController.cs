using System;
using UnityEngine;
using UnityEngine.UI;

public class SimulationUIController : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private Text _timeText;
    [SerializeField] private Text _fuelText;
    [SerializeField] private Slider _progressSlider;
    [SerializeField] private GameObject _convoyStatusPanel;

    [Header("Controls")]
    [SerializeField] private Button _pauseButton;
    [SerializeField] private Button _speedUpButton;
    [SerializeField] private Button _emergencyStopButton;
    [SerializeField] private Slider _timeScaleSlider;

    [Header("Result Panel")]
    [SerializeField] private GameObject _resultPanel;
    [SerializeField] private Text _resultTitleText;
    [SerializeField] private Text _resultDetailsText;
    [SerializeField] private Button _continueButton;

    [Header("Convoy Markers")]
    [SerializeField] private Transform _markersContainer;
    [SerializeField] private GameObject _convoyMarkerPrefab;

    private ISimulationSystem _simulation;
    private IEventBus _eventBus;
    private ILocalizationSystem _localization;
    private bool _isPaused;

    private void Start()
    {
        _simulation = ServiceLocator.Get<ISimulationSystem>();
        _eventBus = ServiceLocator.Get<IEventBus>();
        _localization = ServiceLocator.Get<ILocalizationSystem>();

        SetupButtons();
        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void SetupButtons()
    {
        _pauseButton.onClick.AddListener(TogglePause);
        _speedUpButton.onClick.AddListener(ToggleSpeedUp);
        _emergencyStopButton.onClick.AddListener(RequestEmergencyStop);
        _continueButton.onClick.AddListener(OnContinueClicked);

        _timeScaleSlider.onValueChanged.AddListener(OnTimeScaleChanged);
        _timeScaleSlider.minValue = 0.5f;
        _timeScaleSlider.maxValue = 3f;
        _timeScaleSlider.value = 1f;
    }

    private void SubscribeToEvents()
    {
        _simulation.OnTimeUpdated += UpdateTimeDisplay;
        _simulation.OnStateChanged += OnSimulationStateChanged;
        _simulation.OnSimulationEnded += ShowResult;

        _eventBus.Subscribe<ConvoyPositionUpdatedEvent>(OnConvoyPositionUpdated);
        _eventBus.Subscribe<ConvoyDamagedEvent>(OnConvoyDamaged);
        _eventBus.Subscribe<ConvoyUnderAttackEvent>(OnConvoyUnderAttack);
    }

    private void UnsubscribeFromEvents()
    {
        _simulation.OnTimeUpdated -= UpdateTimeDisplay;
        _simulation.OnStateChanged -= OnSimulationStateChanged;
        _simulation.OnSimulationEnded -= ShowResult;

        _eventBus.Unsubscribe<ConvoyPositionUpdatedEvent>(OnConvoyPositionUpdated);
        _eventBus.Unsubscribe<ConvoyDamagedEvent>(OnConvoyDamaged);
        _eventBus.Unsubscribe<ConvoyUnderAttackEvent>(OnConvoyUnderAttack);
    }

    private void UpdateTimeDisplay(float time)
    {
        var minutes = Mathf.FloorToInt(time / 60);
        var seconds = Mathf.FloorToInt(time % 60);
        _timeText.text = $"{minutes:00}:{seconds:00}";
    }

    private void OnSimulationStateChanged(SimulationState state)
    {
        _pauseButton.GetComponentInChildren<Text>().text = state == SimulationState.Paused
            ? _localization.Get("btn_resume")
            : _localization.Get("btn_pause");
    }

    private void TogglePause()
    {
        if (_simulation.State == SimulationState.Running)
        {
            _simulation.PauseSimulation();
            _isPaused = true;
        }
        else if (_simulation.State == SimulationState.Paused)
        {
            _simulation.ResumeSimulation();
            _isPaused = false;
        }
    }

    private void ToggleSpeedUp()
    {
        _simulation.TimeScale = _simulation.TimeScale >= 2f ? 1f : 2f;
        _speedUpButton.GetComponentInChildren<Text>().text =
            _simulation.TimeScale >= 2f ? "x2" : "x1";
    }

    private void OnTimeScaleChanged(float value)
    {
        _simulation.TimeScale = value;
    }

    private void RequestEmergencyStop()
    {
        // Запрашиваем экстренную остановку всех конвоев
        _eventBus.Publish(new EmergencyStopEvent { ConvoyId = "*" });
    }

    private void OnConvoyPositionUpdated(ConvoyPositionUpdatedEvent evt)
    {
        // Обновляем позицию маркера конвоя на карте
        UpdateConvoyMarker(evt.ConvoyId, evt.Position);
    }

    private void OnConvoyDamaged(ConvoyDamagedEvent evt)
    {
        // Показываем уведомление о повреждении
        ShowNotification($"{_localization.Get("convoy_damaged")}: {evt.Damage:F0}");
    }

    private void OnConvoyUnderAttack(ConvoyUnderAttackEvent evt)
    {
        // Показываем предупреждение об атаке
        ShowNotification(_localization.Get("convoy_under_attack"), NotificationType.Warning);
    }

    private void UpdateConvoyMarker(string convoyId, Vector3 position)
    {
        // Логика обновления маркера на UI карте
    }

    private void ShowNotification(string message, NotificationType type = NotificationType.Info)
    {
        // Показываем уведомление
        Debug.Log($"[{type}] {message}");
    }

    private void ShowResult(SimulationResult result)
    {
        _resultPanel.SetActive(true);

        _resultTitleText.text = result.IsSuccess
            ? _localization.Get("mission_success")
            : _localization.Get("mission_failed");

        var details = $"{_localization.Get("time")}: {result.TotalTime:F1}s\n";
        details += $"{_localization.Get("fuel_used")}: {result.FuelUsed:F1}\n";
        details += $"{_localization.Get("deliveries")}: {result.CompletedDeliveries.Count}\n";

        if (result.IsSuccess)
        {
            details += $"\n{_localization.Get("coins_earned")}: {result.CoinsEarned}";
        }
        else
        {
            details += $"\n{_localization.Get("failed_conditions")}:\n";
            foreach (var condition in result.FailedConditions)
            {
                details += $"- {_localization.Get($"condition_{condition.ToLower()}")}\n";
            }
        }

        _resultDetailsText.text = details;
    }

    private void OnContinueClicked()
    {
        _resultPanel.SetActive(false);
        // Возврат к меню или следующий уровень
    }

    private enum NotificationType { Info, Warning, Error }
}