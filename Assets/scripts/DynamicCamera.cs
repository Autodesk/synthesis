using UnityEngine;
using System.Collections;

public class DynamicCamera : MonoBehaviour
{
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

		public DriverStationState(MonoBehaviour mono)
		{
			this.mono = mono;
		}

		public override void Init()
		{
			mono.transform.position = new Vector3 (0f, 1.5f, -9.5f);
			mono.transform.LookAt (new Vector3 (0f, 0f, 0f));
		}

		public override void Update()
		{
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

		Vector3 targetvector;
		Vector3 rotateVector;
		int magnification = 5;
		GameObject robot;

		public OrbitState(MonoBehaviour mono)
		{
			this.mono = mono;	
		}

		public override void Init()
		{
			robot = GameObject.Find ("Robot");
		}

		public override void Update()
		{
			if (robot != null)
			{
				if (robot.transform.childCount > 0) {
					magnification = (int)Mathf.Max (Mathf.Min (magnification - Input.GetAxis ("Mouse ScrollWheel") * 10, 8f), 1f);

					rotateVector = rotateXZ (rotateVector, targetvector, Input.GetMouseButton (2) ? Input.GetAxis ("Mouse X") / 5f : 0f, (float)magnification);
					rotateVector = rotateYZ (rotateVector, targetvector, Input.GetMouseButton (2) ? Input.GetAxis ("Mouse Y") / 5f : 0f, (float)magnification);
					mono.transform.position = rotateVector;
		
					targetvector = auxFunctions.TotalCenterOfMass (robot);
					mono.transform.LookAt (targetvector);
				}
			}
			else
			{
				robot = GameObject.Find("Robot");
			}
		}

		public override void End () {
		}

		Vector3 rotateXZ(Vector3 vector, Vector3 origin, float theta, float mag)
		{
			vector -= origin;
			Vector3 output = vector;
			output.x = Mathf.Cos (theta) * (vector.x ) - Mathf.Sin (theta) * (vector.z ) ;
			output.z = Mathf.Sin (theta) * (vector.x ) + Mathf.Cos (theta) * (vector.z ) ;
			
			return output.normalized*mag + origin;
		}
		
		Vector3 rotateYZ(Vector3 vector, Vector3 origin, float theta, float mag)
		{
			vector -= origin;
			Vector3 output = vector;
			output.y = Mathf.Cos (theta) * (vector.y ) - Mathf.Sin (theta) * (vector.z ) ;
			output.z = Mathf.Sin (theta) * (vector.y ) + Mathf.Cos (theta) * (vector.z ) ;
			output.y = output.y > 0.1f ? output.y : 0.1f;

			return output.normalized*mag + origin;
		}
		
	}

	/// <summary>
	/// Derives from CameraState to create a first person view from the robot.
	/// </summary>
	public class FPVState : CameraState
	{
		GameObject robot;
		Rigidbody skeleton;
		Vector3 offset;
		Vector3 mouseAxis;

		public FPVState(MonoBehaviour mono)
		{
			this.mono = mono;
		}
		
		public override void Init()
		{
			robot = GameObject.Find ("Robot");
			skeleton = (Rigidbody) robot.GetComponentsInChildren<Rigidbody> ().GetValue(0);
			offset = new Vector3 (0f, 1.25f, 0f);
			mono.transform.position = skeleton.transform.position + offset;
			mono.transform.rotation = skeleton.transform.rotation;
			mono.transform.parent = skeleton.transform;
		}
		
		public override void Update()
		{
			/*
			 * The below code is temporary for FPV demonstration purposes. Production code
			 * might allow the user to define a "camera" component on their robot, which will
			 * be used to determine the position of the FPV camera. For now, 'W' and 'S' pan
			 * the camera up and down.
			 */

			if (robot.transform.childCount > 0)
			{
				if (Input.GetKey (KeyCode.W)) {
					mono.transform.Rotate (1, 0, 0);
				} else if (Input.GetKey (KeyCode.S)) {
					mono.transform.Rotate (-1, 0, 0);
				}
			}
		}

		public override void End()
		{
			mono.transform.parent = null;
		}

	}

	CameraState _cameraState;

	public CameraState cameraState
	{
		get
		{
			return _cameraState;
		}
	}

	void Start ()
	{
		SwitchCameraState(new DriverStationState(this));
	}

	void Update ()
	{
		// Will switch the camera state if certain keys are pressed.
		if (Input.GetKey (KeyCode.D))
		{
			if (!_cameraState.GetType().Equals(typeof(DriverStationState))) SwitchCameraState(new DriverStationState(this));
		}
		else if (Input.GetKey (KeyCode.R))
		{
			if (!_cameraState.GetType().Equals(typeof(OrbitState))) SwitchCameraState(new OrbitState(this));
		}
		else if (Input.GetKey (KeyCode.F))
		{
			if (!_cameraState.GetType().Equals(typeof(FPVState))) SwitchCameraState(new FPVState(this));
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
}
