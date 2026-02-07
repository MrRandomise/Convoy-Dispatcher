using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RouteNodeUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Text _labelText;
    [SerializeField] private Image _iconImage;
    [SerializeField] private Sprite[] _nodeTypeSprites;

    private Vector3 _worldPosition;
    private RouteNodeType _nodeType;

    public void Initialize(string label, Vector3 worldPosition, RouteNodeType nodeType)
    {
        _labelText.text = label;
        _worldPosition = worldPosition;
        _nodeType = nodeType;

        if (_nodeTypeSprites != null && _nodeTypeSprites.Length > (int)nodeType)
        {
            _iconImage.sprite = _nodeTypeSprites[(int)nodeType];
        }

        UpdateScreenPosition();
    }

    private void Update()
    {
        UpdateScreenPosition();
    }

    private void UpdateScreenPosition()
    {
        var screenPos = Camera.main.WorldToScreenPoint(_worldPosition);
        transform.position = screenPos;

        // Скрываем если за камерой
        gameObject.SetActive(screenPos.z > 0);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Выбор точки для добавления в маршрут
        ServiceLocator.Get<IEventBus>().Publish(new RouteNodeSelectedEvent
        {
            Position = _worldPosition,
            NodeType = _nodeType
        });
    }
}