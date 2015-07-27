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
	
	// Update is called once per frame
<<<<<<< HEAD
	void Update () {
		//TODO this is all placeholder stuff
		Vector3 forceDirection = Vector3.up;//transform.localToWorldMatrix*Vector3.up;
		Vector3 force = forceDirection * currentTorque;
		rigidbody.AddForce (force*10+Physics.gravity, ForceMode.Acceleration);
	}


}
=======
	void Update () 
	{
		//TODO this is mostly placeholder stuff
		Vector3 forceDirection = Vector3.up;
		Vector3 force = forceDirection * currentTorque;
		int stageOffset = (int)eType;// 1 + ((int)eType)%2;
		for (int i = 0; i < transform.parent.childCount; i++) 
		{
			Rigidbody rbody = transform.parent.GetChild(i).rigidbody;
			if(rbody.GetComponent<ConfigurableJoint>()!=null && rbody.GetComponent<ConfigurableJoint>().connectedBody == rigidbody)
			{
				rbody.useGravity= false;
				rbody.AddForce(force*3+Physics.gravity*stageOffset/4, ForceMode.Acceleration);
			}
		}
		rigidbody.AddForce (force*3+Physics.gravity*stageOffset/4, ForceMode.Acceleration);

	}
}  
>>>>>>> origin/forpatrick
