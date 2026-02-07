using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class MobileInputSystem : IInputSystem
{
    public event Action<Vector2> OnPan;
    public event Action<float> OnRotate;
    public event Action<float> OnZoom;

    private Vector2 _lastPanPosition;
    private float _lastPinchDistance;
    private float _lastRotationAngle;

    public MobileInputSystem()
    {
        EnhancedTouchSupport.Enable();
    }

    public void Update()
    {
        // Мобильные тачи
        if (Touch.activeTouches.Count == 1)
        {
            HandlePan(Touch.activeTouches[0]);
        }
        else if (Touch.activeTouches.Count == 2)
        {
            HandlePinchAndRotate(Touch.activeTouches[0], Touch.activeTouches[1]);
        }

        // Поддержка мыши для редактора
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.isPressed)
        {
            var delta = mouse.delta.ReadValue();
            if (delta.sqrMagnitude > 0.1f)
            {
                OnPan?.Invoke(delta);
            }
        }

        // Зум колёсиком мыши
        if (mouse != null)
        {
            var scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.1f)
            {
                OnZoom?.Invoke(scroll * 0.01f);
            }
        }
    }

    private void HandlePan(Touch touch)
    {
        if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
        {
            _lastPanPosition = touch.screenPosition;
            return;
        }

        if (touch.phase == UnityEngine.InputSystem.TouchPhase.Moved)
        {
            var delta = touch.screenPosition - _lastPanPosition;
            OnPan?.Invoke(delta);
            _lastPanPosition = touch.screenPosition;
        }
    }

    private void HandlePinchAndRotate(Touch touch0, Touch touch1)
    {
        var currentDistance = Vector2.Distance(touch0.screenPosition, touch1.screenPosition);
        var currentAngle = Mathf.Atan2(
            touch1.screenPosition.y - touch0.screenPosition.y,
            touch1.screenPosition.x - touch0.screenPosition.x
        ) * Mathf.Rad2Deg;

        if (touch0.phase == UnityEngine.InputSystem.TouchPhase.Began ||
            touch1.phase == UnityEngine.InputSystem.TouchPhase.Began)
        {
            _lastPinchDistance = currentDistance;
            _lastRotationAngle = currentAngle;
            return;
        }

        var zoomDelta = currentDistance - _lastPinchDistance;
        if (Mathf.Abs(zoomDelta) > 1f)
        {
            OnZoom?.Invoke(zoomDelta * 0.01f);
        }

        var rotationDelta = Mathf.DeltaAngle(_lastRotationAngle, currentAngle);
        if (Mathf.Abs(rotationDelta) > 0.5f)
        {
            OnRotate?.Invoke(rotationDelta);
        }

        _lastPinchDistance = currentDistance;
        _lastRotationAngle = currentAngle;
    }
}