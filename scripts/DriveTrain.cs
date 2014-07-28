using UnityEngine;
using System.Collections.Generic;
using ExceptionHandling;

public class DriveJoints : MonoBehaviour
{

	// Set all of the wheelColliders in a given list to a motorTorque value corresponding to the signal and maximum Torque Output of a Vex Motor
	public static void SetListOfMotors(List<UnityRigidNode> setOfWheels, float signal, float brakeTorque)
	{

		// The conversion factor from Oz-In to NM
		float OzInToNm = .00706155183333f;
				
		foreach (UnityRigidNode wheel in setOfWheels)
		{
			if (wheel.GetSkeletalJoint().cDriver.GetInfo<WheelDriverMeta>().type != WheelType.NOT_A_WHEEL)
			{
				// Applies a given brakeTorque (value is input as Oz-In but converted to N-M before use)
				wheel.GetWheelCollider().brakeTorque = OzInToNm * brakeTorque;
				// Maximum Torque of a Vex CIM Motor is 171.7 Oz-In, so we can multuply it by the signal to get the output torque. Note that we multiply it by a constant to convert it from an Oz-In to a unity NM 
				wheel.GetWheelCollider().motorTorque = OzInToNm * (signal * 100 * (float)171.1);
				;
				wheel.GetConfigJoint().targetAngularVelocity = new Vector3(wheel.GetWheelCollider().rpm * 6 * Time.deltaTime, 0, 0);
			}
						
		}
	}
		
	// Rotates a wheel 45 degress to act as a mecanum wheel
	public static void RotateWheel45(List<UnityRigidNode> wheels)
	{
		foreach (UnityRigidNode wheel in wheels)
		{
			//wheel.GetWheelCollider ().transform.rotation = Quaternion.Euler(45, 90, 270);
			wheel.GetWheelCollider().transform.Rotate(0, -315, 0);
		}
	}
		
	// Set a given configurable joint's target velocity
	public static void SetConfigJointMotorX(UnityRigidNode configJoint, float speed)
	{
		configJoint.GetConfigJoint().targetVelocity = new Vector3(speed, 0, 0);
	}
	
	// Updates a wheel at the given PWM port with a given value
	// Note that this also returns true or false, because if a motor is set to 
	public static void UpdateWheel(Dictionary<int, List<UnityRigidNode>> wheels, int pwmPort, float speed)
	{
		if (speed != 0)
		{
			DriveJoints.SetListOfMotors(wheels [pwmPort], speed, 0);
			
		} else
		{
			// The maximum brakeTorque of a vex motor is 343.3 oz-in
			DriveJoints.SetListOfMotors(wheels [pwmPort], 0, 343.3f);
		}
	}
	
	// Defaults to a value of 10 (60 PSI)... This is not a realistic value, because velocity depends on the volume of the tank as well as the air pressure. We can figure this out, but it might be something for super happy fun time.
	public static void UpdatePiston(Dictionary<List<int>, UnityRigidNode> piston, int pistonPort1, int pistonPort2, float targetVelocity)
	{
		// When we get to superHappyFun Time, we will need to calculate some realistic PSI forces and find a way to use that, and the volume of a given tank to calculate an accurate target velocity value for the piston (Hopefull, we can store the data in the UnityRigidNode)
		SetConfigJointMotorX(piston [new List<int>{pistonPort1, pistonPort2}], 30);
	}
	
	public static void UpdateAllWheels(Dictionary<int, List<UnityRigidNode>> wheels, float[] pwm)
	{
		for (int i = 0; i<pwm.Length; i++)
		{
			if (wheels.ContainsKey(i + 1))
			{
				UpdateWheel(wheels, i + 1, pwm [i]);
			}
		}
	}

	// I had a bit of trouble grasping how this system worked in the beginning, so I will explain.
	// This function takes a skeleton and byte (a packet) as input, and will use both to check if each solenoid port is open.
	public static void updateSolenoids(RigidNode_Base skeleton, byte packet)
	{
		
		List<RigidNode_Base> listOfNodes = new List<RigidNode_Base>();
		skeleton.ListAllNodes(listOfNodes);
		
		foreach (RigidNode_Base subBase in listOfNodes)
		{
			
			UnityRigidNode unityNode = (UnityRigidNode)subBase;
			
			// If the rigidNodeBase contains a bumper_pneumatic joint driver (meaning that its a solenoid)
			if (subBase.GetSkeletalJoint() != null && subBase.GetSkeletalJoint().cDriver.GetDriveType() == JointDriverType.BUMPER_PNEUMATIC)
			{
				// It will use bitwise operators to check if the port is open.
				/* Full Explanation:
				 	* bool StateX is a boolean that will return True if port X is open
				 	* the packet is the byte we take as input
				 	* The "1 << subBase.GetSkeletalJoint().cDriver.portA" does the following:
				 		* It gets the port numbers for solenoid A (and subtracts 1 from it so that the solenoid port is base 0 instead of base 0)
				 		* And then it uses bitwise operators to create a byte
				 		* It does this by shifting the integer 1 over to the left by the value of port A.
				 		* Heres an example of the bitwise lefshift operators in use: "1 << 5" will result in 100000.
				 		* So in our case, if port = 3, 
				 			* "1 << subBase.GetSkeletalJoint().cDriver.portA - 1" 
				 			* = "1 << 3 - 1"
				 			* = "1 << 2"
				 			* = 100  --- which is also a bit 
				 	* Now that is has that information, it compares that byte to the packet the function recieves using the & operator
				 	* Here is how the & operator works:
				 		* "The bitwise AND operator (&) compares each bit of the first operand to the corresponding bit of the second operand. If both bits are 1, the corresponding result bit is set to 1. Otherwise, the corresponding result bit is set to 0." - msdn.microsoft.com
				 		*  So if the bit of the first operand is	"1111"
				 		*  And the bit of the second operand is		"1010"
				 		*  The resulting bit will be:				"1010"
				 		*  As you can see, it oompares the data in each bit, and if they both match (in our case, if they both are 1) at the given position,
				 		*  The resulting byte will have a 1 at the given position. If they don't match however, the result at the given will be 0.
			 		* So in our case, if the byte at solenoid port 0 (the first bit) has a value of 1 in our packet, and our code is checking to see if port 1 is open, the following operation is executed: "00000001 & 00000001".
			 		* This will result in a byte value of "00000001".
			 		* Now, since bits can evaluate to integers, a byte that results in a value greater than 0 indicates that the byte contains a bit with a value of 1 as opposed to 0.
			 		* In our case, that would indicate that the solenoid port we checked for with the bitwise & operator is open.
				 */
				int stateA = packet & (1 << (subBase.GetSkeletalJoint().cDriver.portA - 1));
				int stateB = packet & (1 << (subBase.GetSkeletalJoint().cDriver.portB - 1));
				// Now, if both solenoid ports are open
				if (stateA > 0)
				{
					// Port A is open, so the solenoid will be set to forward
					DriveJoints.SetConfigJointMotorX(unityNode, 30);
					Debug.Log("A solenoid is set to forward");
				} else if (stateB > 0)
				{
					// Port B is open, so the solenoid will be set to reverse
					DriveJoints.SetConfigJointMotorX(unityNode, -30);
					Debug.Log("A solenoid is in reverse");
				} else
				{
					DriveJoints.SetConfigJointMotorX(unityNode, 0);
				}
			}
		}
		
	}
}




// A class to hold all motor/pnuematic/joint initialization functions
public class InitializeMotors : MonoBehaviour
{	
	// Combs through all of the skeleton data and finds the pwm port numbers of each wheel (by looking through the UnityRigidNode that it is attatched to)
	// The differene here however, is that more than one wheel can be assigned per motor port (in the case that the team has a chain drive, for example).
	public static Dictionary<int, List<UnityRigidNode>> AssignListOfMotors(RigidNode_Base skeleton)
	{
		// This dictionary will hold all of the data we gather from the skeleton and store it
		Dictionary<int, List<UnityRigidNode>> functionOutput = new Dictionary<int, List<UnityRigidNode>>();
		
		// We will need this to grab and comb through all of the nodes (parts) or the skeleton
		List<RigidNode_Base> listAllNodes = new List<RigidNode_Base>();
		skeleton.ListAllNodes(listAllNodes);
		
		foreach (RigidNode_Base dropTheBase in listAllNodes)
		{
			// UnityRigidNodes have the functions we need (it inherits from RigidNodeBase so we can typecast it)
			UnityRigidNode unityNode = (UnityRigidNode)dropTheBase;
			
			// If it finds a wheelCollider
			if (unityNode.GetWheelCollider() != null)
			{
							
				// Calculating Wheel Friction (I am not sure if this is already set, but in case it is not,  here it is)
				WheelFrictionCurve ForwardFriction = new WheelFrictionCurve();
				ForwardFriction.asymptoteSlip = unityNode.GetSkeletalJoint().cDriver.GetInfo<WheelDriverMeta>().forwardAsympSlip;
				ForwardFriction.asymptoteValue = unityNode.GetSkeletalJoint().cDriver.GetInfo<WheelDriverMeta>().forwardAsympValue;
				ForwardFriction.extremumSlip = unityNode.GetSkeletalJoint().cDriver.GetInfo<WheelDriverMeta>().forwardExtremeSlip;
				ForwardFriction.extremumValue = unityNode.GetSkeletalJoint().cDriver.GetInfo<WheelDriverMeta>().forwardExtremeValue;
				ForwardFriction.stiffness = 1;

				
				WheelFrictionCurve SidewaysFriction = new WheelFrictionCurve();
				SidewaysFriction.asymptoteSlip = unityNode.GetSkeletalJoint().cDriver.GetInfo<WheelDriverMeta>().sideAsympSlip;
				SidewaysFriction.asymptoteValue = unityNode.GetSkeletalJoint().cDriver.GetInfo<WheelDriverMeta>().sideAsympValue;
				SidewaysFriction.extremumSlip = unityNode.GetSkeletalJoint().cDriver.GetInfo<WheelDriverMeta>().sideExtremeSlip;
				SidewaysFriction.extremumValue = unityNode.GetSkeletalJoint().cDriver.GetInfo<WheelDriverMeta>().sideExtremeValue;
				SidewaysFriction.stiffness = 1;
								
				unityNode.GetWheelCollider().forwardFriction = ForwardFriction;
				unityNode.GetWheelCollider().sidewaysFriction = SidewaysFriction;

				// The code will now attempt to assign each unityNode containing a wheelCollider a unique PWM port value. If it does not, however, it will throw an exception (see ErrorStuff.CS)
				if (functionOutput.ContainsKey(unityNode.GetPortA()))
				{
					functionOutput [unityNode.GetPortA()].Add(unityNode);
					//Debug.Log (string.Format ("Wheel Name: {0}, PWM Port {1}.", unityNode.GetWheelCollider ().name, unityNode.GetPortA ()));
					
					// If it isn't however, the user will have issues
				} else
				{
					functionOutput [unityNode.GetPortA()] = new List<UnityRigidNode>();
					functionOutput [unityNode.GetPortA()].Add(unityNode);
				}
			}
		}
		
		return functionOutput;
	}


}
