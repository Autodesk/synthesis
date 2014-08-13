using UnityEngine;
using System.Collections.Generic;
using ExceptionHandling;
using System;

//Data struct for packets sent to WPI server through Unity client
public class InputStatePacket
{
	public DIOModule[] dio = new DIOModule[2];
	public Encoder[] encoders = new Encoder[4];
	public AnalogValues[] ai = new AnalogValues[1];
	public Counter[] counter = new Counter[8];
	
	public InputStatePacket()
	{
		for(int i = 0; i < dio.Length; i++)
		{
			dio[i] = new DIOModule();		
		}for(int i = 0; i < encoders.Length; i++)
		{
			encoders[i] = new Encoder();		
		}
		for(int i = 0; i < ai.Length; i++)
		{
			ai[i] = new AnalogValues();		
		}
		for(int i = 0; i < counter.Length; i++)
		{
			counter[i] = new Counter();		
		}
		
	}
	
	public class  DIOModule
	{
		public const int LENGTH = 4;
		public UInt32 digitalInput;
	}

<<<<<<< HEAD
	public class Encoder
=======
	public class Encoders
>>>>>>> origin/Piston-Management
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
			
			Buffer.BlockCopy(new Int32[]{encoders [i].value}, 0, packet, head, Encoder.LENGTH);
			head += Encoder.LENGTH;
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
			wheel.GetConfigJoint().targetAngularVelocity = new Vector3(0, 0, 0);
		} else
		{
			wheel.GetWheelCollider().brakeTorque = 0;
		}
	
		// Maximum Torque of a Vex CIM Motor is 171.7 Oz-In, so we can multuply it by the signal to get the output torque. Note that we multiply it by a constant to convert it from an Oz-In to a unity NM 
<<<<<<< HEAD
		wheel.GetWheelCollider().motorTorque = OzInToNm * (signal * 30f * (float)171.1);
=======
		wheel.GetWheelCollider().motorTorque = OzInToNm * (signal * 171.1f);
>>>>>>> origin/Piston-Management
		wheel.GetConfigJoint().targetAngularVelocity = new Vector3(wheel.GetWheelCollider().rpm * 6 * Time.deltaTime, 0, 0);
	}

	// A function to handle solenoids
	// We will have accurate velocity measures later, but for now, we need something that works.
	public static void SetSolenoid(UnityRigidNode node, bool forward, float pistonDiameter, float psi)
	{
		// Since Unity Uses metric units, we will need to convert psi to N/Mm^2 (pounds => Newtons and in^2 => mm^2)
		float psiToNMm2 = 0.00689475728f;
		float pistonForce = (psiToNMm2 * psi) * (Mathf.PI * Mathf.Pow((pistonDiameter / 2), 2));
		float acceleration = pistonForce / node.GetConfigJoint().rigidbody.mass * (forward?1:-1);
		// Dot product is reversed, so we need to negate it
		float velocity = acceleration * (Time.deltaTime) - Vector3.Dot(node.GetConfigJoint().rigidbody.velocity, node.unityObject.transform.TransformDirection(node.GetConfigJoint().axis));

		// Setting the maximum force of the piston.
		JointDrive newDriver = new JointDrive();
		newDriver.maximumForce = pistonForce;
		newDriver.mode = JointDriveMode.Velocity;
		node.GetConfigJoint().xDrive = newDriver;

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
		
	// Set a given configurable joint's target velocity
	public static void SetConfigJointMotorX(UnityRigidNode configJoint, float speed)
	{
		configJoint.GetConfigJoint().targetVelocity = new Vector3(speed, 0, 0);
	}

	// Gets the linear position of a UnityRigidNode relative to its parent (intended to be used with pistons, but it could be used elsewhere)
	public static float GetLinearPositionRelativeToParent(UnityRigidNode baseNode) {
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
						//Debug.Log("MOTOR: " + i + ": " + pwm[i]);

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
	public static void UpdateSolenoids(RigidNode_Base skeleton, byte packet)
	{
		List<RigidNode_Base> listOfNodes = new List<RigidNode_Base>();
		skeleton.ListAllNodes(listOfNodes);
		
		foreach (RigidNode_Base subBase in listOfNodes)
		{
			
			UnityRigidNode unityNode = (UnityRigidNode)subBase;
			
			// If the rigidNodeBase contains a bumper_pneumatic joint driver (meaning that its a solenoid)
			if (subBase.GetSkeletalJoint() != null && (subBase.GetSkeletalJoint().cDriver.GetDriveType() == JointDriverType.BUMPER_PNEUMATIC || subBase.GetSkeletalJoint().cDriver.GetDriveType() == JointDriverType.RELAY_PNEUMATIC))
			{
				// We use bitwise operators to check if the port is open.
				int stateA = packet & (1 << (subBase.GetSkeletalJoint().cDriver.portA - 1));
				int stateB = packet & (1 << (subBase.GetSkeletalJoint().cDriver.portB - 1));

				float linearPositionAlongAxis = GetLinearPositionRelativeToParent(unityNode);

				// Checking to see if custom PSI and diameter values exist
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

				Debug.Log(linearPositionAlongAxis);

				// If the piston hits its upper limit, stop it from extending any farther.
				if (Mathf.Abs(unityNode.GetConfigJoint().linearLimit.limit - linearPositionAlongAxis) < (.03f * unityNode.GetConfigJoint().linearLimit.limit)) 
				{
					// Since we still want it to retract, however, we will only stop the piston if its velocity if positive. If its not, (its going backwards), we won't need to stop it
					if (unityNode.GetConfigJoint().targetVelocity.x > 0) {
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
}
		

