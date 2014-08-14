using UnityEngine;
using System.Collections.Generic;
using ExceptionHandling;
using System;

//Data struct for packets sent to WPI server through Unity client
public class InputStatePacket
{
	public DIOModule[] dio = new DIOModule[2];
	public Encoders[] encoders = new Encoders[4];
	public AnalogValues[] ai = new AnalogValues[1];
	public Counter[] counter = new Counter[8];
	
	public InputStatePacket()
	{
		for (int i = 0; i < dio.Length; i++)
		{
			dio [i] = new DIOModule();		
		}
		for (int i = 0; i < encoders.Length; i++)
		{
			encoders [i] = new Encoders();		
		}
		for (int i = 0; i < ai.Length; i++)
		{
			ai [i] = new AnalogValues();		
		}
		for (int i = 0; i < counter.Length; i++)
		{
			counter [i] = new Counter();		
		}
		
	}
	
	public class  DIOModule
	{
		public const int LENGTH = 4;
		public UInt32 digitalInput;
	}

	public class Encoders
	{
		public const int LENGTH = 4;
		public Int32 value;
	}

	public class AnalogValues
	{
		public const int LENGTH = 4 * (8);
		public Int32[] analogValues = new Int32[8];
	}

	public class Counter
	{
		public const int LENGTH = 4;
		public Int32 value;
	}
	public int Write(byte[] packet)
	{
		int head = 0;
		for (int i = 0; i < dio.Length; i++)
		{
			
			Buffer.BlockCopy(new UInt32[]{dio [i].digitalInput}, 0, packet, head, DIOModule.LENGTH);
			head += DIOModule.LENGTH;
		}
		for (int i = 0; i < encoders.Length; i++)
		{
			
			Buffer.BlockCopy(new Int32[]{encoders [i].value}, 0, packet, head, Encoders.LENGTH);
			head += Encoders.LENGTH;
		}
		for (int i = 0; i < ai.Length; i++)
		{
			
			
			Buffer.BlockCopy(ai [i].analogValues, 0, packet, head, AnalogValues.LENGTH);
			head += AnalogValues.LENGTH;
			
		}
		for (int i = 0; i < counter.Length; i++)
		{
			
			Buffer.BlockCopy(new Int32[]{counter [i].value}, 0, packet, head, Counter.LENGTH);
			head += Counter.LENGTH;
		}
		return head;
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
		float acceleration = node.GetConfigJoint().xDrive.maximumForce / node.GetConfigJoint().rigidbody.mass * (forward ? 1 : -1);
		// Dot product is reversed, so we need to negate it
		float velocity = acceleration * (Time.deltaTime) - Vector3.Dot(node.GetConfigJoint().rigidbody.velocity, node.unityObject.transform.TransformDirection(node.GetConfigJoint().axis));

		node.GetConfigJoint().targetVelocity = new Vector3(velocity, 0, 0);
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

	// Gets the linear position of a UnityRigidNode relative to its parent (intended to be used with pistons, but it could be used elsewhere)
	public static float GetLinearPositionRelativeToParent(UnityRigidNode baseNode)
	{
		Vector3 baseDirection = baseNode.unityObject.transform.rotation * baseNode.GetConfigJoint().axis;
		baseDirection.Normalize();
		UnityRigidNode parentNode = (UnityRigidNode)(baseNode.GetParent());

		// Vector difference between the world positions of the node, and the parent of the node
		Vector3 difference = baseNode.unityObject.transform.position - parentNode.unityObject.transform.position;

		// Find the magnitude of 'difference' along the baseDirection
		float linearPositionAlongAxis = Vector3.Dot(baseDirection, difference);

		// The dot product we get is inverted, so we need to invert it again before we return it.
		return -linearPositionAlongAxis;
	}

	// Get Angle between two up vectors
	public static float GetAngleBetweenChildAndParent(UnityRigidNode child)
	{
		UnityRigidNode parent = (UnityRigidNode)child.GetParent();
		return (180f / Mathf.PI) * (Mathf.Acos(Vector3.Dot(child.unityObject.transform.up, parent.unityObject.transform.up) / (child.unityObject.transform.up.magnitude * parent.unityObject.transform.up.magnitude)));
	}

	// Drive All Motors Associated with a PWM port
	public static void UpdateAllMotors(RigidNode_Base skeleton, unityPacket.OutputStatePacket.DIOModule[] modules)
	{
		float[] pwm = modules [0].pwmValues;
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

						// We will need this to tell when the joint is very near a limit
						float angularPosition = GetAngleBetweenChildAndParent(unitySubNode);

						// Stopping the configurable joint if it approaches its limits (if its within 5% of its limit)
						if ((unitySubNode.GetConfigJoint().highAngularXLimit.limit - angularPosition) < (0.05f * unitySubNode.GetConfigJoint().highAngularXLimit.limit))
						{
							// This prevents the motor from rotating toward its limit again after we have gotten close enough to the limit that we need to stop it.
							// We will need it to be able to rotate away from the limit however (hence, the if-else statements)
							// If the local up Vector of the unityObject is negative, the joint is approaching its positive limit (I am not sure if this will work in all cases, so its testing time!)
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
						// Should we throw an exception here?
						Debug.Log("There's an issue: We have an active motor not set (even though it should be set).");
					}
				}
			}
		}
	}
		
	// This function takes a skeleton and byte (a packet) as input, and will use both to check if each solenoid port is open.

	public static void updateSolenoids(RigidNode_Base skeleton, unityPacket.OutputStatePacket.SolenoidModule[] solenoidModules)
	{
		byte packet = solenoidModules [0].state;
		List<RigidNode_Base> listOfNodes = new List<RigidNode_Base>();
		skeleton.ListAllNodes(listOfNodes);
		
		foreach (RigidNode_Base subBase in listOfNodes)
		{
			UnityRigidNode unityNode = (UnityRigidNode)subBase;

			// Make sure piston and skeletalJoint exist
			// If the rigidNodeBase contains a bumper_pneumatic joint driver (meaning that its a solenoid)
			if (subBase != null && subBase.GetSkeletalJoint() != null && subBase.GetSkeletalJoint().cDriver != null && (subBase.GetSkeletalJoint().cDriver.GetDriveType() == JointDriverType.BUMPER_PNEUMATIC || subBase.GetSkeletalJoint().cDriver.GetDriveType() == JointDriverType.RELAY_PNEUMATIC))
			{
				// It will use bitwise operators to check if the port is open (see wiki for full explanation).
				int stateA = packet & (1 << (subBase.GetSkeletalJoint().cDriver.portA - 1));
				int stateB = packet & (1 << (subBase.GetSkeletalJoint().cDriver.portB - 1));

				float linearPositionAlongAxis = GetLinearPositionRelativeToParent(unityNode);
				if (stateA > 0)
				{
					try
					{
						SetSolenoid(unityNode, true, unityNode.GetSkeletalJoint().cDriver.GetInfo<PneumaticDriverMeta>().widthMM, unityNode.GetSkeletalJoint().cDriver.GetInfo<PneumaticDriverMeta>().pressurePSI);
						//Debug.Log(linearPositionAlongAxis);
					} catch
					{
						SetSolenoid(unityNode, true, 12.7f, 60f);
					}
				} else if (stateB > 0)
				{
					try
					{
						SetSolenoid(unityNode, false, unityNode.GetSkeletalJoint().cDriver.GetInfo<PneumaticDriverMeta>().widthMM, unityNode.GetSkeletalJoint().cDriver.GetInfo<PneumaticDriverMeta>().pressurePSI);
						//Debug.Log(linearPositionAlongAxis);
					} catch
					{
						SetSolenoid(unityNode, false, 12.7f, 60f);
					}
				}

				// If the piston hits its upper limit, stop it from extending any farther.
				if (Mathf.Abs(unityNode.GetConfigJoint().linearLimit.limit - linearPositionAlongAxis) < (.03f * unityNode.GetConfigJoint().linearLimit.limit))
				{
					// Since we still want it to retract, however, we will only stop the piston if its velocity if positive. If its not, (its going backwards), we won't need to stop it
					if (unityNode.GetConfigJoint().targetVelocity.x > 0)
					{
						unityNode.GetConfigJoint().targetVelocity = Vector3.zero;
					}
					// Otherwise, if the piston has reached its lower limit, we need to stop it from attempting retract farther.
				} else if (Mathf.Abs(-1 * unityNode.GetConfigJoint().linearLimit.limit - linearPositionAlongAxis) < (.03f * unityNode.GetConfigJoint().linearLimit.limit))
				{
					if (unityNode.GetConfigJoint().targetVelocity.x < 0)
					{
						unityNode.GetConfigJoint().targetVelocity = Vector3.zero;
					}
				}
			}
		}
	}

	//public static void UpdateAllJoints(RigidNode_Base skeleton, float[] pwmAssignments, byte solenoidAssignments) {
	//	UpdateAllWheels(skeleton, pwmAssignments);
	//	UpdateSolenoids(skeleton, solenoidAssignments);
	//}
}
		

