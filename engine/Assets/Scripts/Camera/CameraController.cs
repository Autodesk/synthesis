using System;
using System.Collections.Generic;
using UnityEngine;
using Synthesis.UI.Panels.Variant;
using Synthesis.PreferenceManager;

public class CameraController : MonoBehaviour {
    private ICameraMode _cameraMode;
    public ICameraMode CameraMode
    {
        get { return _cameraMode; }
        set
        {
            if (_cameraMode != null && _cameraMode != value)
            {
                _cameraMode.End(this);
            }
            var previous = _cameraMode;
            _cameraMode = value;
        }
    }
    
    public static Dictionary<string, ICameraMode> CameraModes = new Dictionary<string, ICameraMode>();

    static CameraController()
    {
        CameraModes.Add("Orbit", new OrbitCameraMode());
        CameraModes.Add("Freecam", new FreeCameraMode());
        CameraModes.Add("Overview", new OverviewCameraMode());
        CameraModes.Add("Driver Station", new DriverStationCameraMode());
    }

    public static bool isOverGizmo = false;
    
    public const string ZOOM_SENSITIVITY_PREF = "Zoom Sensitivity";//camera settings
    public const string YAW_SENSITIVITY_PREF = "Yaw Sensitivity";
    public const string PITCH_SENSITIVITY_PREF = "Pitch Sensitivity";
    public const int ZOOM_SENSITIVITY_DEFAULT = 5;
    public const int YAW_SENSITIVITY_DEFAULT = 10;
    public const int PITCH_SENSITIVITY_DEFAULT = 3;
    [SerializeField, Range(1f, 15.0f)] public static float PitchSensitivity;
    [SerializeField, Range(1f, 15.0f)] public static float YawSensitivity;
    [SerializeField, Range(0.1f, 5f)] public static float ZoomSensitivity;
    [SerializeField] public float PitchLowerLimit;
    [SerializeField] public float PitchUpperLimit;
    [SerializeField] public float ZoomLowerLimit;
    [SerializeField] public float ZoomUpperLimit;
    [SerializeField, Range(0.005f, 1.0f)] public float OrbitalAcceleration;
    [SerializeField, Range(0.005f, 1.0f)] public float ZoomAcceleration;
    
    [SerializeField] public Renderer GroundRenderer;
    
    private void Start() {
        //Set Camera and Screen Settings
        CameraMode = CameraModes["Orbit"];

        PitchSensitivity = TryGetPref<float>(PITCH_SENSITIVITY_PREF, PITCH_SENSITIVITY_DEFAULT);
        YawSensitivity = TryGetPref<float>(YAW_SENSITIVITY_PREF, YAW_SENSITIVITY_DEFAULT);
        ZoomSensitivity = TryGetPref<float>(ZOOM_SENSITIVITY_PREF, ZOOM_SENSITIVITY_DEFAULT);
    }

    public T TryGetPref<T>(string key, T defaultVal) {
        if (PreferenceManager.ContainsPreference(key))
            return PreferenceManager.GetPreference<T>(key);
        return defaultVal;
    }

    public void Update() {
        // if (FollowTransform != null && transform.parent != FollowTransform)
        //     transform.parent = FollowTransform;

        CameraMode.Update(this);
    }

    public void FixedUpdate() {
        CameraMode.FixedUpdate(this);
    }

    public void LateUpdate() {
        CameraMode.LateUpdate(this);
    }
}
