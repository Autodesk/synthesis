using UnityEngine;
using System.Collections;

public class DynamicCamera : MonoBehaviour
{
	/// <summary>
	/// The scrolling enabled.
	/// </summary>
	private static bool movingEnabled = true;

	/// <summary>
	/// The state of the camera.
	/// </summary>
	CameraState _cameraState;

	/// <summary>
	/// Gets the state of the camera.
	/// </summary>
	/// <value>The state of the camera.</value>
	public CameraState cameraState
	{
		get
		{
			return _cameraState;
		}
	}

	/// <summary>
	/// Abstract class for defining various states of the camera.
	/// </summary>
	public abstract class CameraState
	{
		protected MonoBehaviour mono;

		/// <summary>
		/// Init this instance (will be called in SwitchCameraMode()).
		/// </summary>
		public abstract void Init ();

		/// <summary>
		/// Update this instance (will be called in Update()).
		/// </summary>
		public abstract void Update ();

		/// <summary>
		/// End this instance (will be called in SwitchCameraMode()).
		/// </summary>
		public abstract void End ();
	}

	/// <summary>
	/// Derives from CameraState to create a view from the Driver Station.
	/// </summary>
	public class DriverStationState : CameraState
	{

		GameObject robot;
		Quaternion startRotation;
		Quaternion lookingRotation;
		Quaternion currentRotation;

		public DriverStationState(MonoBehaviour mono)
		{
			this.mono = mono;
		}

		public override void Init()
		{
			robot = GameObject.Find("Robot");
			mono.transform.position = new Vector3 (0f, 1.5f, -9f);
			startRotation = Quaternion.LookRotation(Vector3.zero - mono.transform.position);
			currentRotation = startRotation;
		}

		public override void Update()
		{
			if (robot != null && robot.transform.childCount > 0)
			{
				lookingRotation = Quaternion.LookRotation(auxFunctions.TotalCenterOfMass(robot) - mono.transform.position);
				currentRotation = Quaternion.Lerp(startRotation, lookingRotation, 0.5f);
			}
			else
			{
				robot = GameObject.Find("Robot");
			}

			mono.transform.rotation = currentRotation;
		}

		public override void End()
		{
		}
	}

	/// <summary>
	/// Derives from CameraState to create a view that orbits and follows the robot.
	/// </summary>
	public class OrbitState : CameraState
	{
		Vector3 targetVector;
		Vector3 rotateVector;
		Vector3 lagVector;
		const float lagResponsiveness = 10f;
		float magnification = 5.0f;
		float cameraAngle = 45f;
		float panValue = 0f;
		GameObject robot;

		public OrbitState(MonoBehaviour mono)
		{
			this.mono = mono;
		}

		public override void Init()
		{
			robot = GameObject.Find ("Robot");
			rotateVector = new Vector3(0f, 1f, 0f);
			lagVector = rotateVector;
		}

		public override void Update()
		{
			if (robot != null && robot.transform.childCount > 0)
			{
				if(movingEnabled)
				{
					targetVector = auxFunctions.TotalCenterOfMass(robot);

					if (Input.GetMouseButton(1))
					{
						cameraAngle = Mathf.Max(Mathf.Min (cameraAngle - Input.GetAxis("Mouse Y") * 5f, 90f), 0f);
						panValue = -Input.GetAxis ("mouse_axis_0") / 5f;
					}
					else
					{
						panValue = 0f;

						if (Input.GetMouseButton(0))
						{
							magnification = Mathf.Max (Mathf.Min (magnification - ((Input.GetAxis ("mouse_axis_1") / 5f) * magnification), 12f), 0.1f);
						}
					}

					rotateVector = rotateXZ (rotateVector, targetVector, panValue, magnification);
					rotateVector.y = targetVector.y + magnification * Mathf.Sin (cameraAngle * Mathf.Deg2Rad);

					lagVector += (rotateVector - lagVector) * (lagResponsiveness * Time.deltaTime);
					mono.transform.position = lagVector;
					mono.transform.LookAt (targetVector);
				}
			}
			else
			{
				robot = GameObject.Find("Robot");
			}
		}

		public override void End ()
		{
		}
		
		Vector3 rotateXZ(Vector3 vector, Vector3 origin, float theta, float mag)
		{
			vector -= origin;
			Vector3 output = vector;
			output.x = Mathf.Cos (theta) * (vector.x ) - Mathf.Sin (theta) * (vector.z );
			output.z = Mathf.Sin (theta) * (vector.x ) + Mathf.Cos (theta) * (vector.z );
			
			return output.normalized * mag + origin;
		}
	}

	/// <summary>
	/// Derives from CameraState to create a first person view from the robot.
	/// </summary>
	public class FreeroamState : CameraState
	{
		Vector3 positionVector;
		Vector3 lagPosVector;
		Vector3 rotationVector;
		Vector3 lagRotVector;
		float zoomValue;
		float lagZoom;
		const float lagResponsiveness = 15f;
		float rotationSpeed;
		float transformSpeed;
		float scrollWheelSensitivity;

		public FreeroamState(MonoBehaviour mono)
		{
			this.mono = mono;
		}
		
		public override void Init()
		{
			positionVector = new Vector3(0f, 1f, 0f);
			lagPosVector = positionVector;
			rotationVector = Vector3.zero;
			lagRotVector = rotationVector;
			zoomValue = 60f;
			lagZoom = zoomValue;
			rotationSpeed = 1f;
			transformSpeed = 0.25f;
			scrollWheelSensitivity = 40f;
		}
		
		public override void Update()
		{	
			if (movingEnabled)
			{
				if (Input.GetMouseButton(0) && Input.GetMouseButton(1))
				{
					positionVector += (Input.GetAxis("mouse_axis_1") * mono.transform.up) * transformSpeed;
					positionVector += (Input.GetAxis("mouse_axis_0") * mono.transform.right) * transformSpeed;
				}
				else if (Input.GetMouseButton(0))
				{
					rotationVector.y += Input.GetAxis ("mouse_axis_0") * rotationSpeed;
					positionVector += (Input.GetAxis("mouse_axis_1") * mono.transform.forward) * transformSpeed;
				}
				else if (Input.GetMouseButton(1))
				{
					rotationVector.x -= Input.GetAxis ("mouse_axis_1") * rotationSpeed;
					rotationVector.y += Input.GetAxis ("mouse_axis_0") * rotationSpeed;
				}

				zoomValue = Mathf.Max (Mathf.Min(zoomValue - Input.GetAxis("mouse_axis_2") * scrollWheelSensitivity, 60.0f), 10.0f);

				lagPosVector += (positionVector - lagPosVector) * (lagResponsiveness * Time.deltaTime);
				lagRotVector += (rotationVector - lagRotVector) * (lagResponsiveness * Time.deltaTime);
				lagZoom += (zoomValue - lagZoom) * (lagResponsiveness * Time.deltaTime);

				mono.transform.position = lagPosVector;
				mono.transform.eulerAngles = lagRotVector;
				mono.camera.fieldOfView = lagZoom;
			}
		}

		public override void End()
		{
			mono.camera.fieldOfView = 60.0f;
		}

	}

	void Start ()
	{
		SwitchCameraState(new DriverStationState(this));
        transform.GetComponent<Camera>().backgroundColor = new Color(0.4f, 0.6f, 0.8f);
    }

	void LateUpdate ()
	{
        if (movingEnabled)
        {
            // Rotates between camera states.
            if (Input.GetKeyDown(Controls.ControlKey[(int)Controls.Control.CameraToggle]))
            {
                if (cameraState.GetType().Equals(typeof(DriverStationState))) SwitchCameraState(new OrbitState(this));
                else if (cameraState.GetType().Equals(typeof(OrbitState))) SwitchCameraState(new FreeroamState(this));
                else SwitchCameraState(new DriverStationState(this));
            }
		}
		
		if (_cameraState != null) _cameraState.Update ();
	}

	/// <summary>
	/// Switches the camera mode.
	/// </summary>
	/// <param name="state">State</param>
	public void SwitchCameraState(CameraState state)
	{
		if (_cameraState != null) _cameraState.End ();
		_cameraState = state;
		_cameraState.Init ();
	}

	/// <summary>
	/// Enables the scrolling.
	/// </summary>
	public void EnableMoving()
	{
		movingEnabled = true;
	}

	/// <summary>
	/// Disables the scrolling.
	/// </summary>
	public void DisableMoving()
	{
		movingEnabled = false;
	}
}