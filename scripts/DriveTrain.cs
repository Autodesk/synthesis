using UnityEngine;
using System.Collections.Generic;
using ExceptionHandling;
using System;


//Data struct for packets sent to WPI server through Unity client
public class InputStatePacket
{
	public DIOModule[] dio = new DIOModule[2];
	public Encoders[] encoder = new Encoders[4];
	public AnalogValues[] ai = new AnalogValues[1];
	public Counter[] counter = new Counter[8];
	
	public class  DIOModule
	{
		public UInt32 digitalInput;
	}
	public class Encoders
	{
		public Int32 value;
	}
	public class AnalogValues
	{
		public const int LENGTH = 4 * (8);
		public Int32[] analogValues = new Int32[8];
	}
	public class Counter
	{
		public Int32 value;
	}
	
	public int Write(byte[] packet)
	{

		return packet.Length;
	}
}


public class DriveJoints : MonoBehaviour
{
	// Set all of the wheelColliders in a given list to a motorTorque value corresponding to the signal and maximum Torque Output of a Vex Motor
	public static void SetMotor(UnityRigidNode wheel, float signal)
	{
		// The conversion factor from Oz-In to NM.
		float OzInToNm = .00706155183333f;
		
		if (signal == 0)
		{
			// If no motor torque is applied, the breaks are applied
			// The maximum brakeTorque of a vex motor is 343.3 oz-in
			wheel.GetWheelCollider().brakeTorque = OzInToNm * 343.3f;
			wheel.GetConfigJoint().targetAngularVelocity = new Vector3(0,0,0);
		} else
		{
			wheel.GetWheelCollider().brakeTorque = 0;
		}
	
		// Maximum Torque of a Vex CIM Motor is 171.7 Oz-In, so we can multuply it by the signal to get the output torque. Note that we multiply it by a constant to convert it from an Oz-In to a unity NM 
        wheel.GetWheelCollider().motorTorque = OzInToNm * (signal * 30f * (float) 171.1);
		wheel.GetConfigJoint().targetAngularVelocity = new Vector3(wheel.GetWheelCollider().rpm * 6 * Time.deltaTime, 0, 0);
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
	
	public static void UpdateAllWheels(RigidNode_Base skeleton, float[] pwm)
	{
		List<RigidNode_Base> test = new List<RigidNode_Base>();
		skeleton.ListAllNodes(test);

		// Cycles through the packet
		for (int i = 0; i<pwm.Length; i++)
		{
			// Cycles through the skeleton
			foreach (RigidNode_Base subNode in test)
			{
				UnityRigidNode unitySubNode = (UnityRigidNode)subNode;

				// If port A matches the index of the array in the packet, (A.K.A: the packet index is reffering to the wheelCollider on the subNode0), then that specific wheel Collider is set.
				// Note that it also checks to make sure that the node is a wheel (and not a solenoid)
				try
				{
					if (unitySubNode.GetSkeletalJoint().cDriver != null && unitySubNode.GetSkeletalJoint().cDriver.GetInfo<WheelDriverMeta>().type != WheelType.NOT_A_WHEEL && unitySubNode.GetPortA() == i + 1)
					{
						SetMotor(unitySubNode, pwm [i]);
						Debug.Log("MOTOR: " + i + ": " + pwm[i]);

					}
				} catch (NullReferenceException)
				{
					// Do nothing. There is an object in the skeleton which has not joint, and to compensate for that.
				}
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
