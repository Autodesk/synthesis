using UnityEngine;
using System.Collections.Generic;
using ErrorHandling;

public class DriveJoints : MonoBehaviour
{

		// Will drive a single motor with a given speed (and brakeTorque)
		public static void SetMotor (UnityRigidNode wheel, float speed, float brakeTorque)
		{
				
		}

		// A helper function to set a set of motors assigned to a PWM port
		public static void SetListOfMotors (List<UnityRigidNode> setOfWheels, float speed, float brakeTorque)
		{
				foreach (UnityRigidNode wheel in setOfWheels) {
						if (wheel.GetSkeletalJoint ().cDriver.GetInfo<WheelDriverMeta> ().type != WheelType.NOT_A_WHEEL) {
								wheel.GetWheelCollider ().brakeTorque = brakeTorque;
								wheel.GetWheelCollider ().motorTorque = speed;
								wheel.GetConfigJoint ().targetAngularVelocity = new Vector3 (speed * 6 * Time.deltaTime, 0, 0);
						}
						SetMotor (wheel, speed, 0);
				}
		}

		public static void StopAllWheelColliders (Dictionary<int, List<UnityRigidNode>> wheels)
		{
				foreach (KeyValuePair<int, List<UnityRigidNode>> wheelSet in wheels) {
						foreach (UnityRigidNode wheel in wheelSet.Value) {
								wheel.GetWheelCollider ().motorTorque = 0;
								wheel.GetWheelCollider ().brakeTorque = 500;
								wheel.GetConfigJoint ().targetAngularVelocity = new Vector3 (0, 0, 0);
						}
				}
		}
		
		// Rotates a wheel 45 degress to act as a mecanum wheel
		public static void RotateWheel45 (List<UnityRigidNode> wheels)
		{
				foreach (UnityRigidNode wheel in wheels) {
						//wheel.GetWheelCollider ().transform.rotation = Quaternion.Euler(45, 90, 270);
						wheel.GetWheelCollider ().transform.Rotate (0, -315, 0);
				}
		}
		
		// Set a given configurable joint's target velocity
		public static void SetConfigJointMotorX (UnityRigidNode configJoint, float speed)
		{
				configJoint.GetConfigJoint ().targetVelocity = new Vector3 (speed, 0, 0);
		}
	
		// Updates a wheel at the given PWM port with a given value
		// Note that this also returns true or false, because if a motor is set to 
		public static void UpdateWheel (Dictionary<int, List<UnityRigidNode>> wheels, int givenPWM, float speed)
		{
				if (speed != 0) {
						DriveJoints.SetListOfMotors (wheels [givenPWM], speed, 0);
			
				} else {
						DriveJoints.SetListOfMotors (wheels [givenPWM], 0, 100);
				}
		}
	
		// Defaults to a value of 10 (60 PSI)... This is not a realistic value, because velocity depends on the volume of the tank as well as the air pressure. We can figure this out, but it might be something for super happy fun time.
		public static void UpdatePiston (Dictionary<Tuple<int, int>, UnityRigidNode> piston, float pistonPort1, float pistonPort2)
		{
				SetConfigJointMotorX (piston [new Tuple<int, int> ((int)pistonPort1, (int)pistonPort2)], 10);
		}
	
		public static void UpdateAllWheels (Dictionary<int, List<UnityRigidNode>> wheels, float[] pwm)
		{
				for (int i = 0; i<pwm.Length; i++) {
						if (wheels.ContainsKey (i + 1)) {
								UpdateWheel (wheels, i + 1, pwm [i]);
						}
				}
				// For each float in struct?
				// I may have to do this explicitly
		}
}




// A class to hold all motor/pnuematic/joint initialization functions
public class InitializeMotors : MonoBehaviour
{	
		// Combs through all of the skeleton data and finds the pwm port numbers of each wheel (by looking through the UnityRigidNode that it is attatched to)
		// The differene here however, is that more than one wheel can be assigned per motor port (in the case that the team has a chain drive, for example).
		public static Dictionary<int, List<UnityRigidNode>> AssignListOfMotors (RigidNode_Base skeleton)
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
							
								// Calculating Wheel Friction (I am not sure if this is already set, but in case it is not,  here it is)
								WheelFrictionCurve ForwardFriction = new WheelFrictionCurve ();
								ForwardFriction.asymptoteSlip = unityNode.GetSkeletalJoint ().cDriver.GetInfo<WheelDriverMeta> ().forwardAsympSlip;
								ForwardFriction.asymptoteValue = unityNode.GetSkeletalJoint ().cDriver.GetInfo<WheelDriverMeta> ().forwardAsympValue;
								ForwardFriction.extremumSlip = unityNode.GetSkeletalJoint ().cDriver.GetInfo<WheelDriverMeta> ().forwardExtremeSlip;
								ForwardFriction.extremumValue = unityNode.GetSkeletalJoint ().cDriver.GetInfo<WheelDriverMeta> ().forwardExtremeValue;
								ForwardFriction.stiffness = 1;

				
								WheelFrictionCurve SidewaysFriction = new WheelFrictionCurve ();
								SidewaysFriction.asymptoteSlip = unityNode.GetSkeletalJoint ().cDriver.GetInfo<WheelDriverMeta> ().sideAsympSlip;
								SidewaysFriction.asymptoteValue = unityNode.GetSkeletalJoint ().cDriver.GetInfo<WheelDriverMeta> ().sideAsympValue;
								SidewaysFriction.extremumSlip = unityNode.GetSkeletalJoint ().cDriver.GetInfo<WheelDriverMeta> ().sideExtremeSlip;
								SidewaysFriction.extremumValue = unityNode.GetSkeletalJoint ().cDriver.GetInfo<WheelDriverMeta> ().sideExtremeValue;
								SidewaysFriction.stiffness = 1;
								
								unityNode.GetWheelCollider ().forwardFriction = ForwardFriction;
								unityNode.GetWheelCollider ().sidewaysFriction = SidewaysFriction;

								// The code will now attempt to assign each unityNode containing a wheelCollider a unique PWM port value. If it does not, however, it will throw an exception (see ErrorStuff.CS)
								if (functionOutput.ContainsKey (unityNode.GetPortA ())) {
										functionOutput [unityNode.GetPortA ()].Add (unityNode);
										//Debug.Log (string.Format ("Wheel Name: {0}, PWM Port {1}.", unityNode.GetWheelCollider ().name, unityNode.GetPortA ()));
					
										// If it isn't however, the user will have issues
								} else {
										functionOutput [unityNode.GetPortA ()] = new List<UnityRigidNode> ();
										functionOutput [unityNode.GetPortA ()].Add (unityNode);
								}
						}
				}
		
				return functionOutput;
		}

		// Assign Solenoids
		public static Dictionary<Tuple<int, int>, UnityRigidNode> AssignSolenoids (RigidNode_Base skeleton)
		{
				// Will store a pair of solenoid ports. Key will be Port1, the value will be port2
				Dictionary<Tuple<int, int>, UnityRigidNode> output = new Dictionary<Tuple<int, int>, UnityRigidNode> ();
		
				// We will need this to grab and comb through all of the nodes (parts) or the skeleton
				List<RigidNode_Base> listAllNodes = new List<RigidNode_Base> ();
				skeleton.ListAllNodes (listAllNodes);
		
				foreach (RigidNode_Base node in listAllNodes) {
						UnityRigidNode unityNode = (UnityRigidNode)node;
			
						// It will now check to see if the joint is pneumatic (Linear). If it is, we will store it, along with its solenoid ports, in a dictionary.
						if (node.GetSkeletalJoint ().GetJointType () == SkeletalJointType.LINEAR) {
								foreach (KeyValuePair<Tuple<int, int>, UnityRigidNode> solenoidData in output) {
					
										// This checks to see if any of the solenoid ports have already been assigned.  If they are, an exception will be thrown
										// I regret trying to use Tuples, but it seems like the most orderly way. I don't think multidimensional arrays would be any better at this point.
										// I am considering a revamp of everything. Perhaps it may be better to organize each motor, solenoid, etc as its own class.
										// Make sure to uncomment the GetPortB functions once its added back in to UnityRigidNode
										if (solenoidData.Key.Item1 == unityNode.GetPortA () || solenoidData.Key.Item2 == unityNode.GetPortA ()) {
												throw new SolenoidConflictException (unityNode.GetPortA ());
										} else if (solenoidData.Key.Item1 == unityNode.GetPortB () || solenoidData.Key.Item2 == unityNode.GetPortB ()) {
												throw new SolenoidConflictException (unityNode.GetPortB ());
										} else {
												output [new Tuple<int, int> (unityNode.GetPortA (), unityNode.GetPortB ())] = unityNode;
										}
					
								}
						}
				}
				return output;
		}
}




// I would like to use a tuple, but .NET 3.5 does not have any... Lets make one
// Some explanation will follow (because I did not know what this was, and writing things down helps me remember)
// The Tuple class is a template class, and the two variables (Item2 and Item2) can be set to any variable upon creation
// Note that X and Y are types. When you create this class in particular, a variable, Item1, is set to the type X (which your provide when you create an instance of the class). Item2 works in the same way, except that it is assigned to the type you specify in Y
// For example, if i create a instance of the Tuple class, Tuple<int, string>, Item1 (which is assigned the type of X) wil be declared as a public integer. Likewise, Item2 will be declared as a public string.
// You can set them to use any type of variable in this way (though I am using to make a Tuple-like variable)

public class Tuple<X,Y> {
	public X Item1 { get; private set;}
	public Y Item2 { get; private set;}
	
	public Tuple(X item1, Y item2) {
		Item1 = item1;
		Item2 = item2;
	}
}
