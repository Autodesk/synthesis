using UnityEngine;
using System.Collections.Generic;
using ErrorHandling;

public class Controls : MonoBehaviour
{
		// For four wheel drive - will have to be modified to accomodate X-wheel/joint/piston designs
		public static KeyCode forward;
		public static KeyCode backward;
		public static KeyCode left;
		public static KeyCode right;

		/*  This is a bit complicated.
            In order for Unity to recognize input, the user has to press a keyboard button that corresponds to a KeyCode
            This actually makes it harder for the user to select custom keys.
            For example, what if you want to set the F key to forward?
            In order for that to happen, you will need to use: Input.GetKey(KeyCode.F)
            (Sadly, you can't use Input.GetKey("F") without first setting it up in the input manager first,
            which would require user input and a GUI because you can't access the input manager via script)
            We have to use KeyCode.GetKey(KeyCode.F).
            Simple enought right?
            No.
            You can't put Input.GetKey(KeyCode."F") (or whatever key the user wants to use), 
            because KeyCode.F is its own object (all compatible keyboard commands are).
            This is where the following function comes in.

            Credit for this one goes to Westin Miller (Double check with him if I get this explanation wrong)
        */
		public static KeyCode GetKeyCodeFormString (string userInput)
		{
				// Firstly, we are going to grab an array of all of the different KeyCode objects
				System.Array codes = System.Enum.GetValues (typeof(KeyCode));
                
				// for example, it will create a string "Enter").
				// It then compares it to the input the user typed in.
				foreach (KeyCode code in codes) {
						// If the name of the object matches the user string, it will return the KeyCode object that we need.
						if (System.Enum.GetName (typeof(KeyCode), code).Equals (userInput)) {
								return code;
						}
				}
				return KeyCode.None;
		}


		// Allows you to set custom buttons, but sadly, this is not dynamic yet, and it based on a basic forward, back, left, and right principal.
		public static void setControls (string vForward, string vBackward, string vLeft, string vRight)
		{
				forward = GetKeyCodeFormString (vForward);
				backward = GetKeyCodeFormString (vBackward);
				left = GetKeyCodeFormString (vLeft);
				right = GetKeyCodeFormString (vRight);

				// Will be used to make sure that all of the variables are assigned correctly
				Dictionary<string, KeyCode> temp = new Dictionary<string, KeyCode> (){{"forward", forward}, {"backward", backward}, {"left", left}, {"right", right}};

				// If any of the keys are not assigned correctly (that is, they have no set value), it print an error message to the console
				foreach (KeyValuePair<string, KeyCode> key in temp) {
						if (key.Value == KeyCode.None) {
								Debug.Log ("Error, Key " + key.Key + " was not set correctly");
						}
				}
		}

		public static void driveOmniWheel (Dictionary<int, UnityRigidNode> wheels, float translation, float rotation)
		{
				foreach (KeyValuePair<int, UnityRigidNode> wheel in wheels) {
						wheel.Value.GetWheelCollider ().transform.Translate (0, 0, translation);
						wheel.Value.GetWheelCollider ().transform.Rotate (0, rotation, 0);
				}
		}


		// Halts all motors by applying a log of brakeTorque at once
		public static void stopAllMotors (Dictionary<int, UnityRigidNode> wheels)
		{
				foreach (KeyValuePair<int, UnityRigidNode> wheel in wheels) {
						if (wheel.Value.GetWheelCollider() != null) {
							wheel.Value.GetWheelCollider ().motorTorque = 0;
							wheel.Value.GetWheelCollider ().brakeTorque = 40;
							wheel.Value.GetConfigJoint ().targetAngularVelocity = new Vector3 (0, 0, 0);
						}
				}
		}
		
		// Drive Controller for swerve drive
		public static void driveSwerve(Dictionary<int, UnityRigidNode> wheels, float speed, float rotationSpeed) {
				foreach (KeyValuePair<int, UnityRigidNode> wheel in wheels) 
				{
						// Rotates each wheel at a given rate as the user uses the horizontal axese
						wheel.Value.GetWheelCollider ().brakeTorque = 0;
						wheel.Value.GetWheelCollider ().transform.Rotate (0, rotationSpeed * Time.deltaTime * Input.GetAxis ("Horizontal"), 0);
						wheel.Value.GetConfigJoint ().transform.Rotate (0, 0, rotationSpeed * Time.deltaTime * Input.GetAxis ("Horizontal"));

						// Then it drives each wheel another given rate
						wheel.Value.GetWheelCollider ().motorTorque = speed * Input.GetAxis ("Vertical");
				}
		}
		
		// Combs through all of the skeleton data and finds the pwm port numbers of each wheel (by looking through the UnityRigidNode that it is attatched to)
		public static Dictionary<int, UnityRigidNode> assignMotors (RigidNode_Base skeleton)
		{
				// This dictionary will hold all of the data we gather from the skeleton and store it
				Dictionary<int, UnityRigidNode> functionOutput = new Dictionary<int, UnityRigidNode> ();

				// We will need this to grab and comb through all of the nodes (parts) or the skeleton
				List<RigidNode_Base> listAllNodes = new List<RigidNode_Base> ();
				skeleton.ListAllNodes (listAllNodes);

				foreach (RigidNode_Base dropTheBase in listAllNodes) {
						// UnityRigidNodes have the functions we need (it inherits from RigidNodeBase so we can typecast it)
						UnityRigidNode unityNode = (UnityRigidNode)dropTheBase;
                       
						// If it finds a wheelCollider
						if (unityNode.GetWheelCollider () != null) {
								// The code will now attempt to assign each unityNode containing a wheelCollider a unique PWM port value. If it does not, however, it will throw an exception (see ErrorStuff.CS)
								try {
										// If its a unique port, we won't have problems
										if (!functionOutput.ContainsKey (unityNode.GetPortA ())) {
												functionOutput.Add (unityNode.GetPortA (), unityNode);
												Debug.Log (unityNode.GetPortA ());
												//Debug.Log (string.Format ("Wheel Name: {0}, PWM Port {1}.", unityNode.GetWheelCollider ().name, unityNode.GetPortA ()));

												// If it isn't however, the user will have issues
										} else {
												throw new PWMAssignedException (string.Format ("Error, PWM port {0} has already been assigned! (A.K.A: your trying to break the rules)", unityNode.GetPortA ()));
										}
								} catch (PWMAssignedException e) {
										Debug.Log (e);
								}
						}
				}

				// Finally, it is done
				return functionOutput;
		}

	// Combs through all of the skeleton data and finds the pwm port numbers of each wheel (by looking through the UnityRigidNode that it is attatched to)
	// The differene here however, is that more than one wheel can be assigned per motor port (in the case that the team has a chain drive, for example).
	public static Dictionary<int, List<UnityRigidNode>> assignListOfMotors (RigidNode_Base skeleton)
	{
		// This dictionary will hold all of the data we gather from the skeleton and store it
		Dictionary<int, List<UnityRigidNode>> functionOutput = new Dictionary<int, List<UnityRigidNode>> ();
		
		// We will need this to grab and comb through all of the nodes (parts) or the skeleton
		List<RigidNode_Base> listAllNodes = new List<RigidNode_Base> ();
		skeleton.ListAllNodes (listAllNodes);
		
		foreach (RigidNode_Base dropTheBase in listAllNodes) {
			// UnityRigidNodes have the functions we need (it inherits from RigidNodeBase so we can typecast it)
			UnityRigidNode unityNode = (UnityRigidNode)dropTheBase;
			
			// If it finds a wheelCollider
			if (unityNode.GetWheelCollider () != null) {
				// The code will now attempt to assign each unityNode containing a wheelCollider a unique PWM port value. If it does not, however, it will throw an exception (see ErrorStuff.CS)
				if (functionOutput.ContainsKey (unityNode.GetPortA ())) {
					functionOutput[unityNode.GetPortA ()].Add (unityNode);
					Debug.Log (unityNode.GetPortA ());
					//Debug.Log (string.Format ("Wheel Name: {0}, PWM Port {1}.", unityNode.GetWheelCollider ().name, unityNode.GetPortA ()));
					
					// If it isn't however, the user will have issues
				} else {
					functionOutput[unityNode.GetPortA ()] = new List<UnityRigidNode>();
					functionOutput[unityNode.GetPortA ()].Add (unityNode);
					//Debug.Log (unityNode.GetPortA ());
				}
			}
		}

		return functionOutput;
	}

	// Goes through each wheel and edits the physics material of each wheel to match that of a real mecanum wheel
	public static void initiateMecanumFriction(Dictionary<int, UnityRigidNode> nodes) {

		foreach(KeyValuePair<int, UnityRigidNode> node in nodes) {
			node.Value.GetConfigJoint ().collider.attachedRigidbody.Sleep ();

			// With 60lb of downward force
			PhysicMaterial myMaterial = new PhysicMaterial ();
			myMaterial.staticFriction = 0.6f;
			myMaterial.dynamicFriction = 0.7f;
			myMaterial.bounciness = 0f;
			myMaterial.staticFriction2 = 0.06f;
			myMaterial.dynamicFriction2 = 0.7f;
			node.Value.GetConfigJoint ().collider.material = myMaterial;
			
			node.Value.GetConfigJoint ().collider.attachedRigidbody.WakeUp ();
		}
	}	
}