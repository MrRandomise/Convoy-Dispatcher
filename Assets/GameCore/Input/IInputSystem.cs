using System;
using UnityEngine;

public interface IInputSystem
{
    event Action<Vector2> OnPan;
    event Action<float> OnRotate;
    event Action<float> OnZoom;
    void Update();
}