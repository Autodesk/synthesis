using UnityEngine;
using System.Collections;
public class Camera : MonoBehaviour {

	Vector3 COM;
	Vector3 rotateVect;
	GameObject gameobj;
	static float camMag = 4;
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
		output.y = output.y > .1f ? output.y : .1f;
		return output.normalized*mag + origin;
	}

	void Update ()
	{

		camMag -= Input.GetAxis ("Mouse ScrollWheel")*10;
		rotateVect = rotateXZ (rotateVect, COM, 0, camMag);
		rotateVect = rotateYZ (rotateVect, COM, 0, camMag);
		transform.position = rotateVect;

		if (Input.GetMouseButton (2)) 
		{
			rotateVect = rotateXZ (rotateVect, COM, Input.GetAxis("Mouse X")/5f, camMag);
			rotateVect = rotateYZ (rotateVect, COM, Input.GetAxis("Mouse Y")/5f, camMag);
			transform.position = rotateVect;

			COM = UnityRigidNode.TotalCenterOfMass (gameobj);
			Debug.Log(COM);
			transform.LookAt (COM);
		}

	}
	
	void Start ()
	{
		rotateVect = - transform.position;
		gameobj = GameObject.Find("GameObject");
	}
}
