using UnityEngine;

/// <summary>
///  онтроллер камеры дл€ изометрического вида
/// </summary>
public class MapCameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Camera _camera;
    [SerializeField] private float _followSpeed = 5f;
    [SerializeField] private float _cameraHeight = 40f;
    [SerializeField] private float _cameraAngle = 45f;
    [SerializeField] private float _cameraDistance = 30f;

    [Header("Map Size Camera Presets")]
    [SerializeField] private CameraPreset _smallMapPreset = new() { Height = 30f, Angle = 50f, Distance = 20f };
    [SerializeField] private CameraPreset _mediumMapPreset = new() { Height = 40f, Angle = 45f, Distance = 30f };
    [SerializeField] private CameraPreset _largeMapPreset = new() { Height = 50f, Angle = 40f, Distance = 40f };

    private Transform _followTarget;
    private Vector3 _currentVelocity;

    private void Awake()
    {
        if (_camera == null)
        {
            _camera = Camera.main;
        }
    }

    public void SetupForMapSize(MapSize size)
    {
        var preset = size switch
        {
            MapSize.Small => _smallMapPreset,
            MapSize.Medium => _mediumMapPreset,
            MapSize.Large => _largeMapPreset,
            _ => _mediumMapPreset
        };

        _cameraHeight = preset.Height;
        _cameraAngle = preset.Angle;
        _cameraDistance = preset.Distance;

        UpdateCameraRotation();
    }

    public void SetFollowTarget(Transform target)
    {
        _followTarget = target;
    }

    public void SetPosition(Vector3 worldPosition)
    {
        var offset = CalculateCameraOffset();
        _camera.transform.position = worldPosition + offset;
        UpdateCameraRotation();
    }

    private void LateUpdate()
    {
        if (_followTarget == null) return;

        var targetPosition = _followTarget.position + CalculateCameraOffset();
        _camera.transform.position = Vector3.SmoothDamp(
            _camera.transform.position,
            targetPosition,
            ref _currentVelocity,
            1f / _followSpeed
        );
    }

    private Vector3 CalculateCameraOffset()
    {
        float radAngle = _cameraAngle * Mathf.Deg2Rad;
        return new Vector3(0f, _cameraHeight, -_cameraDistance);
    }

    private void UpdateCameraRotation()
    {
        _camera.transform.rotation = Quaternion.Euler(_cameraAngle, 0f, 0f);
    }

    [System.Serializable]
    public class CameraPreset
    {
        public float Height = 40f;
        public float Angle = 45f;
        public float Distance = 30f;
    }
}