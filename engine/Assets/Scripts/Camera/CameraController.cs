using UnityEngine;
using Synthesis.UI.Panels.Variant;

public class CameraController : MonoBehaviour
{
    public ICameraMode cameraMode = new FreeCameraMode();
    
    
    public static bool isOverGizmo = false;
    
    [SerializeField, Range(1f, 15.0f)] public float PitchSensitivity;
    [SerializeField, Range(1f, 15.0f)] public float YawSensitivity;
    [SerializeField, Range(0.1f, 5f)] public float ZoomSensitivity;
    [SerializeField] public float PitchLowerLimit;
    [SerializeField] public float PitchUpperLimit;
    [SerializeField] public float ZoomLowerLimit;
    [SerializeField] public float ZoomUpperLimit;
    [SerializeField, Range(0.005f, 1.0f)] public float OrbitalAcceleration;
    [SerializeField, Range(0.005f, 1.0f)] public float ZoomAcceleration;
    
    [SerializeField] public Renderer GroundRenderer;
    
    private void Start()
    { //Set Camera and Screen Settings
        SettingsPanel.LoadSettings();
        SettingsPanel.MaximizeScreen();
        
        cameraMode.Start(this);
    }
    public void Update() {
  //      if (FollowTransform != null && transform.parent != FollowTransform)
  //          transform.parent = FollowTransform;

        cameraMode.Update(this);
    }

    public void LateUpdate() {
        cameraMode.LateUpdate(this);
    }
}
