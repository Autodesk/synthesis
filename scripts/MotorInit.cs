using UnityEngine;
using System.Collections.Generic;
using ErrorHandling;
using System;

// A class to hold all motor/pnuematic/joint initialization functions
public class MotorInit : MonoBehaviour
{
		
		
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
	
		// Rough Draft
		public static Dictionary<Tuple<int, int>, UnityRigidNode> assignSolenoids (RigidNode_Base skeleton)
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
										//} else if (solenoidData.Key.Item1 == unityNode.GetPortB () || solenoidData.Key.Item2 == unityNode.GetPortB ()) {
										//		throw new SolenoidConflictException (unityNode.GetPortB ());
										} else {
												//output[unityNode.GetPortA(), unityNode.GetPortB()] = unityNode;
										}

								}
						}
				}
				return output;
		}

		// Soon
		public void assignAll ()
		{

		}
}

