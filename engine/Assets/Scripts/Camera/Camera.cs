using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Synthesis.Camera
{
	public class Camera : MonoBehaviour
	{
        [SerializeField]
        Transform _target;
        public Transform Target {
            get => _target;
            set => _target = value;
        }

        [SerializeField, Min(0f)]
        float _distance = 50;

        public float Distance { get => _distance; set => _distance = value; }

        [SerializeField, Range(0.01f, 0.5f)]
        float _zoomSensitivity = 0.1f;

        public float ZoomSensitivity { get => _zoomSensitivity; set => _zoomSensitivity = value; }

        //[SerializeField, Range(1, 5)]
        //float zoomDamp = 2;

        [SerializeField, Range(1, 5)]
        float _orbitSpeed = 2;

        public float OrbitSpeed { get => _orbitSpeed; set => _orbitSpeed = value; }

        [SerializeField, Range(1,20)]
        float _rotationSensitvity = 4;

        public float RotationSensitivity { get => _rotationSensitvity; set => _rotationSensitvity = value; }

        [SerializeField, Range(0, 180)]
        float _yawLimit = 180;

        public float YawLimit { get => _yawLimit; set => _yawLimit = value; }

        [SerializeField, Range(0, 90)]
        float _pitchLimit = 90;

        public float PitchLimit { get => _pitchLimit; set => _pitchLimit = value; }

        [SerializeField]
        bool _freeze = false;

        public bool Freeze { get => _freeze; set => _freeze = value; }

        private Quaternion _pitch; //up and down
        private Quaternion _yaw; //left and right 

        private Quaternion _targetRotation;
        private Vector3 _targetPosition;
        //private float _targetDistance;

        private GameObject targetObject;

        void Awake()
        {
            targetObject = new GameObject(gameObject.name + " Target");
            if (!_target)
            {
                _target = targetObject.transform;
                _target.position = new Vector3(0, 1, 0); //arbitrary
            }
            _pitch = Quaternion.Euler(this.transform.rotation.eulerAngles.x, 0, 0);
            _yaw = Quaternion.Euler(0, this.transform.rotation.eulerAngles.y, 0);
        }

        private void Update()
        {
            float x = 0, y = 0, scroll = 0;
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                x = Input.GetAxis("Mouse X");
                y = -Input.GetAxis("Mouse Y");
                scroll = Input.mouseScrollDelta.y;
            }

            if (!_freeze)
            {
                if (Input.GetMouseButton(0))
                    Orbit(x * _orbitSpeed, y * _orbitSpeed);
                _distance = Mathf.Max(_distance + scroll * _zoomSensitivity, 0);
            }
        }

        void LateUpdate()
        {
            _targetRotation = _yaw * _pitch; //target angle from (0,0,0)

            //interpolate between current rotation to target rotation for smoothness
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, _targetRotation, Mathf.Clamp01(Time.smoothDeltaTime * _orbitSpeed));

            Vector3 offset = this.transform.rotation * (-Vector3.forward * _distance); //magnitude of offset (offset - origin) is equal to distance 
            this.transform.position = _target.position + offset;
        }

        public void Orbit(float yawDelta, float pitchDelta)
        {
            _yaw = _yaw * Quaternion.Euler(0, yawDelta, 0);
            _pitch = _pitch * Quaternion.Euler(pitchDelta, 0, 0);
            ApplyConstraints();
        }

        private void ApplyConstraints()
        {
            Quaternion targetYaw = Quaternion.Euler(0, _target.rotation.eulerAngles.y, 0);
            Quaternion targetPitch = Quaternion.Euler(_target.rotation.eulerAngles.x, 0, 0);

            float yawDifference = Quaternion.Angle(_yaw, targetYaw);
            float pitchDifference = Quaternion.Angle(_pitch, targetPitch);

            float yawOverflow = yawDifference - _yawLimit;
            float pitchOverflow = pitchDifference - _pitchLimit;

            if (yawOverflow > 0) { _yaw = Quaternion.Slerp(_yaw, targetYaw, yawOverflow / yawDifference); }
            if (pitchOverflow > 0) { _pitch = Quaternion.Slerp(_pitch, targetPitch, pitchOverflow / pitchDifference); }
        }

        public void LookAt(GameObject g)
        {
            _target = g.transform;
        }
        public void LookAt(Vector3 v)
        {
            targetObject.transform.position = v;
            _target = targetObject.transform;
        }
        public void LookAt(Transform t)
        {
            _target = t;
        }
    }
}
