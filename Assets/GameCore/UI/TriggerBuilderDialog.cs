using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TriggerBuilderDialog : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject _dialogPanel;
    [SerializeField] private Dropdown _conditionDropdown;
    [SerializeField] private Dropdown _actionDropdown;
    [SerializeField] private InputField _parameterInput;
    [SerializeField] private Toggle _oneShotToggle;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private Text _descriptionText;

    private IConvoy _convoy;
    private Action<ConvoyTrigger> _onConfirm;
    private ILocalizationSystem _localization;

    private readonly List<ConditionTemplate> _conditionTemplates = new()
    {
        new ConditionTemplate("fuel_low", "trigger_fuel_low", typeof(FuelBelowCondition), true),
        new ConditionTemplate("obstacle", "trigger_obstacle", typeof(ObstacleAheadCondition), false),
        new ConditionTemplate("congestion", "trigger_congestion", typeof(RoadCongestedCondition), false),
        new ConditionTemplate("lagging", "trigger_lagging", typeof(ConvoyLaggingCondition), false),
        new ConditionTemplate("under_attack", "trigger_attack", typeof(UnderAttackCondition), false)
    };

    private readonly List<ActionTemplate> _actionTemplates = new()
    {
        new ActionTemplate("switch_route", "action_switch_route", typeof(SwitchRouteAction), true),
        new ActionTemplate("refuel", "action_refuel", typeof(GoToRefuelAction), false),
        new ActionTemplate("slow_down", "action_slow_down", typeof(ChangeSpeedAction), false),
        new ActionTemplate("stop", "action_stop", typeof(EmergencyStopAction), false)
    };

    private void Awake()
    {
        _localization = ServiceLocator.Get<ILocalizationSystem>();
        SetupDropdowns();

        _confirmButton.onClick.AddListener(OnConfirmClicked);
        _cancelButton.onClick.AddListener(Hide);
        _conditionDropdown.onValueChanged.AddListener(OnConditionChanged);
        _actionDropdown.onValueChanged.AddListener(OnActionChanged);
    }

    private void SetupDropdowns()
    {
        _conditionDropdown.ClearOptions();
        var conditionOptions = new List<string>();
        foreach (var template in _conditionTemplates)
        {
            conditionOptions.Add(_localization.Get(template.LocalizationKey));
        }
        _conditionDropdown.AddOptions(conditionOptions);

        _actionDropdown.ClearOptions();
        var actionOptions = new List<string>();
        foreach (var template in _actionTemplates)
        {
            actionOptions.Add(_localization.Get(template.LocalizationKey));
        }
        _actionDropdown.AddOptions(actionOptions);
    }

    public void Show(IConvoy convoy, Action<ConvoyTrigger> onConfirm)
    {
        _convoy = convoy;
        _onConfirm = onConfirm;
        _dialogPanel.SetActive(true);
        UpdateDescription();
    }

    public void Hide()
    {
        _dialogPanel.SetActive(false);
    }

    private void OnConditionChanged(int index)
    {
        var template = _conditionTemplates[index];
        _parameterInput.gameObject.SetActive(template.HasParameter);
        UpdateDescription();
    }

    private void OnActionChanged(int index)
    {
        var template = _actionTemplates[index];
        UpdateDescription();
    }

    private void UpdateDescription()
    {
        var conditionTemplate = _conditionTemplates[_conditionDropdown.value];
        var actionTemplate = _actionTemplates[_actionDropdown.value];

        _descriptionText.text = string.Format(
            _localization.Get("trigger_description_format"),
            _localization.Get(conditionTemplate.LocalizationKey),
            _localization.Get(actionTemplate.LocalizationKey)
        );
    }

    private void OnConfirmClicked()
    {
        var condition = CreateCondition();
        var action = CreateAction();

        if (condition != null && action != null)
        {
            var trigger = new ConvoyTrigger(
                $"trigger_{Guid.NewGuid():N}",
                condition,
                action,
                _oneShotToggle.isOn
            );

            _onConfirm?.Invoke(trigger);
            Hide();
        }
    }

    private ITriggerCondition CreateCondition()
    {
        var template = _conditionTemplates[_conditionDropdown.value];

        if (template.Id == "fuel_low")
        {
            float threshold = 20f;
            if (float.TryParse(_parameterInput.text, out var value))
            {
                threshold = value;
            }
            return new FuelBelowCondition(threshold);
        }

        return template.Id switch
        {
            "obstacle" => new ObstacleAheadCondition(),
            "congestion" => new RoadCongestedCondition(),
            "lagging" => new ConvoyLaggingCondition(),
            "under_attack" => new UnderAttackCondition(),
            _ => null
        };
    }

    private ITriggerAction CreateAction()
    {
        var template = _actionTemplates[_actionDropdown.value];

        return template.Id switch
        {
            "switch_route" => new SwitchRouteAction(_parameterInput.text),
            "refuel" => new GoToRefuelAction(),
            "slow_down" => new ChangeSpeedAction(SpeedMode.Slow),
            "stop" => new EmergencyStopAction(),
            _ => null
        };
    }

    private class ConditionTemplate
    {
        public string Id;
        public string LocalizationKey;
        public Type ConditionType;
        public bool HasParameter;

        public ConditionTemplate(string id, string key, Type type, bool hasParam)
        {
            Id = id;
            LocalizationKey = key;
            ConditionType = type;
            HasParameter = hasParam;
        }
    }

    private class ActionTemplate
    {
        public string Id;
        public string LocalizationKey;
        public Type ActionType;
        public bool HasParameter;

        public ActionTemplate(string id, string key, Type type, bool hasParam)
        {
            Id = id;
            LocalizationKey = key;
            ActionType = type;
            HasParameter = hasParam;
        }
    }
}