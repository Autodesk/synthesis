using UnityEngine;
using System.Collections;
//<Summary>
//Attach this script to an item that should bend from the bottom and be solid from the top like a clip on an elevator
//</Summary>
public class ExampleClass : MonoBehaviour {
	void OnCollisionStay(Collision collisionInfo) {
		if (collisionInfo.relativeVelocity.y < 0)//this condition can be changed, it might not be the best case available
			Physics.IgnoreCollision (collisionInfo.collider, this.collider, true);//should egate the collisio if the condition is met properly
		else
			Physics.IgnoreCollision (collisionInfo.collider, this.collider, false);
		}
	}