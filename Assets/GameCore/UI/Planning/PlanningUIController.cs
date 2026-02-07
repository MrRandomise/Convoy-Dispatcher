using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanningUIController : MonoBehaviour, IPlanningUI
{
    [Header("Panels")]
    [SerializeField] private GameObject _mainPanel;
    [SerializeField] private GameObject _routePanel;
    [SerializeField] private GameObject _convoyPanel;
    [SerializeField] private GameObject _triggersPanel;

    [Header("Route Planning")]
    [SerializeField] private Transform _routeNodesContainer;
    [SerializeField] private GameObject _routeNodePrefab;
    [SerializeField] private LineRenderer _routeLineRenderer;

    [Header("Convoy Settings")]
    [SerializeField] private Dropdown _distanceDropdown;
    [SerializeField] private Dropdown _speedDropdown;
    [SerializeField] private Dropdown _priorityDropdown;
    [SerializeField] private Dropdown _threatBehaviorDropdown;
    [SerializeField] private Transform _convoyListContainer;
    [SerializeField] private GameObject _convoyItemPrefab;

    [Header("Triggers")]
    [SerializeField] private Transform _triggerListContainer;
    [SerializeField] private GameObject _triggerItemPrefab;
    [SerializeField] private Button _addTriggerButton;

    [Header("Actions")]
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _resetButton;

    public event Action OnStartSimulation;
    public event Action<string, ConvoyRules> OnConvoyRulesChanged;
    public event Action<string, ConvoyTrigger> OnTriggerAdded;

    private LevelData _levelData;
    private List<IConvoy> _convoys;
    private IConvoy _selectedConvoy;
    private Route _selectedRoute;
    private List<RouteNode> _customNodes = new();
    private ILocalizationSystem _localization;

    private void Awake()
    {
        _localization = ServiceLocator.Get<ILocalizationSystem>();
        SetupDropdowns();
        SetupButtons();
    }

    public void Initialize(LevelData levelData, List<IConvoy> availableConvoys)
    {
        _levelData = levelData;
        _convoys = availableConvoys;

        PopulateConvoyList();
        PopulateRouteNodes();

        if (_convoys.Count > 0)
        {
            SelectConvoy(_convoys[0]);
        }
    }

    public void Show()
    {
        _mainPanel.SetActive(true);
    }

    public void Hide()
    {
        _mainPanel.SetActive(false);
    }

    private void SetupDropdowns()
    {
        // Distance
        _distanceDropdown.ClearOptions();
        _distanceDropdown.AddOptions(new List<string>
        {
            _localization.Get("distance_tight"),
            _localization.Get("distance_standard"),
            _localization.Get("distance_far")
        });
        _distanceDropdown.onValueChanged.AddListener(OnDistanceChanged);

        // Speed
        _speedDropdown.ClearOptions();
        _speedDropdown.AddOptions(new List<string>
        {
            _localization.Get("speed_slow"),
            _localization.Get("speed_normal"),
            _localization.Get("speed_fast")
        });
        _speedDropdown.onValueChanged.AddListener(OnSpeedChanged);

        // Priority
        _priorityDropdown.ClearOptions();
        _priorityDropdown.AddOptions(new List<string>
        {
            _localization.Get("priority_safety"),
            _localization.Get("priority_balanced"),
            _localization.Get("priority_time")
        });
        _priorityDropdown.onValueChanged.AddListener(OnPriorityChanged);

        // Threat Behavior
        _threatBehaviorDropdown.ClearOptions();
        _threatBehaviorDropdown.AddOptions(new List<string>
        {
            _localization.Get("threat_stop"),
            _localization.Get("threat_evade"),
            _localization.Get("threat_wait")
        });
        _threatBehaviorDropdown.onValueChanged.AddListener(OnThreatBehaviorChanged);
    }

    private void SetupButtons()
    {
        _startButton.onClick.AddListener(() => OnStartSimulation?.Invoke());
        _resetButton.onClick.AddListener(ResetPlanning);
        _addTriggerButton.onClick.AddListener(ShowAddTriggerDialog);
    }

    private void PopulateConvoyList()
    {
        foreach (Transform child in _convoyListContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var convoy in _convoys)
        {
            var item = Instantiate(_convoyItemPrefab, _convoyListContainer);
            var convoyItem = item.GetComponent<ConvoyListItem>();
            convoyItem.Initialize(convoy, OnConvoySelected);
        }
    }

    private void PopulateRouteNodes()
    {
        foreach (Transform child in _routeNodesContainer)
        {
            Destroy(child.gameObject);
        }

        // Показываем точки доставки
        foreach (var delivery in _levelData.DeliveryPoints)
        {
            var node = Instantiate(_routeNodePrefab, _routeNodesContainer);
            var nodeUI = node.GetComponent<RouteNodeUI>();
            nodeUI.Initialize(delivery.Name, delivery.Position, RouteNodeType.DeliveryPoint);
        }

        // Показываем базу
        var baseNode = Instantiate(_routeNodePrefab, _routeNodesContainer);
        var baseUI = baseNode.GetComponent<RouteNodeUI>();
        baseUI.Initialize(_localization.Get("base"), _levelData.BaseSpawnPoint.Position, RouteNodeType.Waypoint);

        UpdateRouteVisualization();
    }

    private void OnConvoySelected(IConvoy convoy)
    {
        SelectConvoy(convoy);
    }

    private void SelectConvoy(IConvoy convoy)
    {
        _selectedConvoy = convoy;
        UpdateConvoyUI();
        UpdateTriggersList();
    }

    private void UpdateConvoyUI()
    {
        if (_selectedConvoy == null) return;

        var rules = _selectedConvoy.Rules;
        _distanceDropdown.value = (int)rules.Distance;
        _speedDropdown.value = (int)rules.Speed;
        _priorityDropdown.value = (int)rules.Priority;
        _threatBehaviorDropdown.value = (int)rules.ThreatResponse;
    }

    private void OnDistanceChanged(int value)
    {
        if (_selectedConvoy == null) return;
        _selectedConvoy.Rules.Distance = (DistanceMode)value;
        NotifyRulesChanged();
    }

    private void OnSpeedChanged(int value)
    {
        if (_selectedConvoy == null) return;
        _selectedConvoy.Rules.Speed = (SpeedMode)value;
        NotifyRulesChanged();
    }

    private void OnPriorityChanged(int value)
    {
        if (_selectedConvoy == null) return;
        _selectedConvoy.Rules.Priority = (PriorityMode)value;
        NotifyRulesChanged();
    }

    private void OnThreatBehaviorChanged(int value)
    {
        if (_selectedConvoy == null) return;
        _selectedConvoy.Rules.ThreatResponse = (ThreatBehavior)value;
        NotifyRulesChanged();
    }

    private void NotifyRulesChanged()
    {
        OnConvoyRulesChanged?.Invoke(_selectedConvoy.Id, _selectedConvoy.Rules);
    }

    private void UpdateRouteVisualization()
    {
        if (_selectedRoute == null && _levelData.Routes.Count > 0)
        {
            _selectedRoute = _levelData.Routes[0];
        }

        if (_selectedRoute == null || _routeLineRenderer == null) return;

        var positions = new Vector3[_selectedRoute.Nodes.Count];
        for (int i = 0; i < _selectedRoute.Nodes.Count; i++)
        {
            positions[i] = _selectedRoute.Nodes[i].Position;
        }

        _routeLineRenderer.positionCount = positions.Length;
        _routeLineRenderer.SetPositions(positions);
    }

    private void UpdateTriggersList()
    {
        foreach (Transform child in _triggerListContainer)
        {
            Destroy(child.gameObject);
        }

        // Здесь отображаем существующие триггеры для конвоя
    }

    private void ShowAddTriggerDialog()
    {
        // Показываем диалог добавления триггера
        var dialog = FindFirstObjectByType<TriggerBuilderDialog>();
        if (dialog != null)
        {
            dialog.Show(_selectedConvoy, OnTriggerCreated);
        }
    }

    private void OnTriggerCreated(ConvoyTrigger trigger)
    {
        if (_selectedConvoy != null)
        {
            OnTriggerAdded?.Invoke(_selectedConvoy.Id, trigger);
            UpdateTriggersList();
        }
    }

    private void ResetPlanning()
    {
        _customNodes.Clear();
        foreach (var convoy in _convoys)
        {
            convoy.Rules = new ConvoyRules();
        }
        UpdateConvoyUI();
        UpdateRouteVisualization();
    }
}