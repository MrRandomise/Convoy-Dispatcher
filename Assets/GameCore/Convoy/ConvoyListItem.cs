using System;
using UnityEngine;
using UnityEngine.UI;

public class ConvoyListItem : MonoBehaviour
{
    [SerializeField] private Text _nameText;
    [SerializeField] private Text _statsText;
    [SerializeField] private Image _iconImage;
    [SerializeField] private Button _selectButton;
    [SerializeField] private GameObject _selectedIndicator;

    private IConvoy _convoy;
    private Action<IConvoy> _onSelected;

    public void Initialize(IConvoy convoy, Action<IConvoy> onSelected)
    {
        _convoy = convoy;
        _onSelected = onSelected;

        _nameText.text = convoy.Id;
        _statsText.text = $"HP: {convoy.Stats.Health} | SPD: {convoy.Stats.Speed}";

        _selectButton.onClick.AddListener(() => _onSelected?.Invoke(_convoy));
    }

    public void SetSelected(bool selected)
    {
        _selectedIndicator.SetActive(selected);
    }
}