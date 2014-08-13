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
	// Set all of the wheelColliders in a given list to a motorTorque value corresponding to the signal and maximum Torque Output of a Vex Motor.
	// We have to multiply the conversion factor by 100, however, because it seems that the values have to be "fudged until they "feel right", even if they appear to be correct (NewtonMeters are the units that PhysX uses).
	public static void SetWheel(UnityRigidNode wheel, float signal)
	{
		// The conversion factor from Oz-In to NM, with our multiplier
		float OzInToNm = 100 * .00706155183333f;
		
		if (signal == 0)
		{
			// If no motor torque is applied, the breaks are applied
			wheel.GetWheelCollider().brakeTorque = OzInToNm * 343.3f;
			wheel.GetConfigJoint().targetAngularVelocity = Vector3.zero;
		} else
		{
			wheel.GetWheelCollider().brakeTorque = 0;
		}
		
		// Maximum Torque of a Vex CIM Motor is 171.7 Oz-In, so we can multuply it by the signal to get the output torque. Note that we multiply it by a constant to convert it from an Oz-In to a unity NM 
		wheel.GetWheelCollider().motorTorque = OzInToNm * (signal * 171.1f);
		wheel.GetConfigJoint().targetAngularVelocity = new Vector3(wheel.GetWheelCollider().rpm * 6 * Time.deltaTime, 0, 0);
	}

	// A function to handle solenoids
	// We will have accurate velocity measures later, but for now, we need something that works.
	public static void SetSolenoid(UnityRigidNode node, bool forward, float pistonDiameter, float psi)
	{
		// First, we calculate force --  DON'T GET RID OF ME YET D:
		// Since Unity Uses metric units, we will need to convert psi to N/Mm^2 (pounds => Newtons and in^2 => mm^2)
		float psiToNMm2 = 0.00689475728f;
		float pistonForce = (psiToNMm2 * psi) * (Mathf.PI * Mathf.Pow((pistonDiameter / 2), 2));
		float acceleration = pistonForce / node.GetConfigJoint().rigidbody.mass;
		
		// This will have an accurate time value later, but for now, this will be an arbitrary number
		float velocity = 5;
		
		// Setting the maximum force of the piston.
		JointDrive newDriver = new JointDrive();
		newDriver.maximumForce = pistonForce;
		newDriver.mode = JointDriveMode.Velocity;
		node.GetConfigJoint().xDrive = newDriver;
		
		if (forward == true)
		{
			node.GetConfigJoint().targetVelocity = new Vector3(velocity, 0, 0);
			//UnityEngine.Debug.Log(lNode.currentLinearPosition);
		} else if (forward == false)
		{
			node.GetConfigJoint().targetVelocity = new Vector3(-1 * (velocity), 0, 0);
			//UnityEngine.Debug.Log(lNode.currentLinearPosition);
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

	// Get Angle between two up vectors
	public static float GetAngleBetweenChildAndParent(UnityRigidNode child)
	{
		UnityRigidNode parent = (UnityRigidNode)child.GetParent();
		return (180f / Mathf.PI) * (Mathf.Acos(Vector3.Dot(child.unityObject.transform.up, parent.unityObject.transform.up) / (child.unityObject.transform.up.magnitude * parent.unityObject.transform.up.magnitude)));
	}
		
	// Set a given configurable joint's target velocity
	public static void SetConfigJointMotorX(UnityRigidNode configJoint, float speed)
	{
		configJoint.GetConfigJoint().targetVelocity = new Vector3(speed, 0, 0);
	}
	
	public static void UpdateAllMotors(RigidNode_Base skeleton, float[] pwm)
	{
		List<RigidNode_Base> listOfSubNodes = new List<RigidNode_Base>();
		skeleton.ListAllNodes(listOfSubNodes);
		
		// Cycles through the packet
		for (int i = 0; i<pwm.Length; i++)
		{
			foreach (RigidNode_Base node in listOfSubNodes)
			{
				
				// Typcasting RigidNode to UnityRigidNode to use UnityRigidNode functions
				UnityRigidNode unitySubNode = (UnityRigidNode)node;
				
				// Checking if there is a joint (and a joint driver) attatched to each joint
				if (unitySubNode.GetSkeletalJoint() != null && unitySubNode.GetSkeletalJoint().cDriver != null && unitySubNode.GetSkeletalJoint().cDriver.GetDriveType().IsMotor())
				{
					
					// Special Case for wheels. 
					// If port A matches the index of the array in the packet, (A.K.A: the packet index is reffering to the wheelCollider on the subNode0), then that specific wheel Collider is set.
					if (unitySubNode.IsWheel && unitySubNode.GetPortA() == i + 1)
					{
						SetWheel(unitySubNode, pwm [i]);
						// If its not a wheel, it checks to see if it the motor is assigned to the current pwm value, and if it is, it also checks to make sure that it has an xdrive
					} else if (unitySubNode.GetPortA() == i + 1 && !unitySubNode.IsWheel)
					{
						// Something Arbitrary for now. 4 radians/second
						unitySubNode.GetConfigJoint().targetAngularVelocity = new Vector3(4 * pwm [i], 0, 0);
						
						float angularPosition = GetAngleBetweenChildAndParent(unitySubNode);
						// Stopping the configurable joint if it approaches its limits
						
						//Debug.Log(unitySubNode.unityObject.transform.up.x);
						if ((unitySubNode.GetConfigJoint().highAngularXLimit.limit - angularPosition) < (0.5f * unitySubNode.GetConfigJoint().highAngularXLimit.limit))
						{
							if (unitySubNode.unityObject.transform.up.x < 0 && unitySubNode.GetConfigJoint().targetAngularVelocity.x > 0)
							{
								unitySubNode.GetConfigJoint().targetAngularVelocity = Vector3.zero;
							} else if (unitySubNode.unityObject.transform.up.x > 0 && unitySubNode.GetConfigJoint().targetAngularVelocity.x < 0)
							{
								unitySubNode.GetConfigJoint().targetAngularVelocity = Vector3.zero;
							}
						}
					} else if (unitySubNode.GetPortA() == i + 1)
					{
						Debug.Log("There's an issue: We have an active motor not set (even though it should be set).");
					}
				}
			}
		}
	}


	// I had a bit of trouble grasping how this system worked in the beginning, so I will explain.
	// This function takes a skeleton and byte (a packet) as input, and will use both to check if each solenoid port is open.
	public static void UpdateSolenoids(RigidNode_Base skeleton, byte packet)
	{
		
		List<RigidNode_Base> listOfNodes = new List<RigidNode_Base>();
		skeleton.ListAllNodes(listOfNodes);
		
		foreach (RigidNode_Base subBase in listOfNodes)
		{
			
			UnityRigidNode unityNode = (UnityRigidNode)subBase;
			//LinearJoint_Base lJoint = (LinearJoint_Base)unityNode.GetSkeletalJoint();
			
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
					SetSolenoid(unityNode, true, unityNode.GetSkeletalJoint().cDriver.GetInfo<PneumaticDriverMeta>().widthMM, unityNode.GetSkeletalJoint().cDriver.GetInfo<PneumaticDriverMeta>().pressurePSI);
					//SetSolenoid(unityNode, true, 25f, 60f);
					//DriveJoints.SetConfigJointMotorX(unityNode, 30);
					//Debug.Log("A solenoid is set to forward");
				} else if (stateB > 0)
				{
					// Port B is open, so the solenoid will be set to reverse
					SetSolenoid(unityNode, false, 25f, 60f);
					//DriveJoints.SetConfigJointMotorX(unityNode, -30);
					//Debug.Log("A solenoid is in reverse");
				} else
				{
					DriveJoints.SetConfigJointMotorX(unityNode, 0);
				}
			}
		}
		
	}
}
