using UnityEngine;
using System.Collections.Generic;
using ErrorHandling;

public class Controls : MonoBehaviour
{
		// For four wheel drive - might be outdated
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

		public static void setMotor (UnityRigidNode wheel, float speed)
		{
				wheel.GetWheelCollider ().motorTorque = speed;
				wheel.GetConfigJoint ().targetAngularVelocity = new Vector3 (wheel.GetWheelCollider ().rpm * 6 * Time.deltaTime, 0, 0);
		}

		public static void setSetOfMotors (Dictionary<int, UnityRigidNode> dictionary, int speed, params int[] pwmPorts)
		{
				foreach (int pwmPort in pwmPorts) {
						setMotor (dictionary [pwmPort], speed);
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
												Debug.Log (string.Format ("Wheel Name: {0}, PWM Port {1}.", unityNode.GetWheelCollider ().name, unityNode.GetPortA ()));

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


}