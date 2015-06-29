using UnityEngine;
using System.Collections;

public class Camera : MonoBehaviour
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
			robot = GameObject.Find("Robot");
		}

		public override void Update()
		{
			magnification = (int)Mathf.Max(Mathf.Min(magnification - Input.GetAxis ("Mouse ScrollWheel")*10, 8f), 1f);

			rotateVector = rotateXZ (rotateVector, targetvector, Input.GetMouseButton(2) ? Input.GetAxis ("Mouse X") / 5f : 0f, (float)magnification);
			rotateVector = rotateYZ (rotateVector, targetvector, Input.GetMouseButton(2) ? Input.GetAxis ("Mouse Y") / 5f : 0f, (float)magnification);
			mono.transform.position = rotateVector;
			
			targetvector = auxFunctions.TotalCenterOfMass (robot);
			mono.transform.LookAt (targetvector);
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
			// Left and right mouse buttons -> pan left and right; mouse wheel -> look up and down.
			if (Input.GetMouseButton (1)) {
				mono.transform.Rotate (0, 1, 0);
			} else if (Input.GetMouseButton (0)) {
				mono.transform.Rotate (0, -1, 0);
			}
			mono.transform.Rotate(Input.GetAxis("Mouse ScrollWheel") * 20f, 0, 0);
		}

		public override void End() {
			mono.transform.parent = null;
		}

	}

	CameraState cameraState;

	void Start ()
	{
		SwitchCameraState(new DriverStationState(this));
	}

	void Update ()
	{
		// Will switch the camera state if certain keys are pressed.
		if (Input.GetKey (KeyCode.D))
		{
			if (!cameraState.GetType().Equals(typeof(DriverStationState))) SwitchCameraState(new DriverStationState(this));
		}
		else if (Input.GetKey (KeyCode.R))
		{
			if (!cameraState.GetType().Equals(typeof(OrbitState))) SwitchCameraState(new OrbitState(this));
		}
		else if (Input.GetKey (KeyCode.F))
		{
			if (!cameraState.GetType().Equals(typeof(FPVState))) SwitchCameraState(new FPVState(this));
		}
		
		if (cameraState != null) cameraState.Update ();
	}

	/// <summary>
	/// Switches the camera mode.
	/// </summary>
	/// <param name="state">State</param>
	public void SwitchCameraState(CameraState state)
	{
		if (cameraState != null) cameraState.End ();
		cameraState = state;
		cameraState.Init ();
	}

}
