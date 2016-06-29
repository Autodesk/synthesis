using UnityEngine;
using System.Collections;

public class ElevatorScript : MonoBehaviour {

	public const float FORCE_MULTIPLIER = 5f;
	public float currentTorque;
	public ElevatorType eType;
	public bool breakOn = false;

	// Use this for initialization
	void Start () {
        rigidbody.useGravity = true;
	}
	
	void Update () 
	{
		//TODO this is mostly placeholder stuff
		Vector3 forceDirection = Vector3.up;
		Vector3 force = forceDirection * currentTorque * FORCE_MULTIPLIER;
		int stageOffset = (int)eType;// 1 + ((int)eType)%2;
		for (int i = 0; i < transform.parent.childCount; i++) 
		{
			Rigidbody rbody = transform.parent.GetChild(i).rigidbody;
			if(rbody.GetComponent<ConfigurableJoint>()!=null && rbody.GetComponent<ConfigurableJoint>().connectedBody == rigidbody)
			{
				rbody.useGravity = false;
				rbody.drag = FORCE_MULTIPLIER;
				rbody.AddRelativeForce(force+Physics.gravity*stageOffset, ForceMode.Acceleration);
			}
		}
		rigidbody.AddRelativeForce (force+Physics.gravity*stageOffset, ForceMode.Acceleration);
	}
}
