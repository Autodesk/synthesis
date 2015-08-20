using UnityEngine;
using System.Collections;

public class ElevatorScript : MonoBehaviour {

	public float currentTorque;
	public ElevatorType eType;
	public bool breakOn = false;
	// Use this for initialization
	void Start () {
        rigidbody.useGravity = false;
	}
	
	void Update () 
	{
		//TODO this is mostly placeholder stuff
		Vector3 forceDirection = Vector3.up;
		Vector3 force = forceDirection * currentTorque * 5;
		int stageOffset = (int)eType;// 1 + ((int)eType)%2;
		for (int i = 0; i < transform.parent.childCount; i++) 
		{
			Rigidbody rbody = transform.parent.GetChild(i).rigidbody;
			if(rbody.GetComponent<ConfigurableJoint>()!=null && rbody.GetComponent<ConfigurableJoint>().connectedBody == rigidbody)
			{
				//rbody.useGravity= false;
				rbody.AddForce(force*3+Physics.gravity*stageOffset, ForceMode.Acceleration);
			}
		}
		rigidbody.AddForce (force*3+Physics.gravity*stageOffset, ForceMode.Acceleration);

		if (rigidbody.velocity.magnitude > 1) {
			rigidbody.velocity = rigidbody.velocity.normalized * 1;
		}
		else if (rigidbody.velocity.magnitude < 1.1f) {
			rigidbody.velocity = rigidbody.velocity.normalized * 0;
		}
	}
}
