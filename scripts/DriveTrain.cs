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

	public class Encoder
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
		wheel.GetWheelCollider().motorTorque = OzInToNm * (signal * 30f * (float)171.1);
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
	
	public static void UpdateAllWheels(RigidNode_Base skeleton, unityPacket.OutputStatePacket.DIOModule[] modules)
	{
		float[] pwm = modules[0].pwmValues;
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
	public static void updateSolenoids(RigidNode_Base skeleton, unityPacket.OutputStatePacket.SolenoidModule[] solenoidModules)
	{
		byte packet = solenoidModules[0].state;
		List<RigidNode_Base> listOfNodes = new List<RigidNode_Base>();
		skeleton.ListAllNodes(listOfNodes);
		
		foreach (RigidNode_Base subBase in listOfNodes)
		{
			UnityRigidNode unityNode = (UnityRigidNode)subBase;
			// If the rigidNodeBase contains a bumper_pneumatic joint driver (meaning that its a solenoid)
			if (subBase != null && subBase.GetSkeletalJoint() != null  && subBase.GetSkeletalJoint().cDriver != null && subBase.GetSkeletalJoint().cDriver.GetDriveType() == JointDriverType.BUMPER_PNEUMATIC)
			{
				
				 
				//It will shift the 1 over based on the port number, so it will take port 3 and check if it has a value of 1 or 0 at the third bit. This allows us to check if the state is "on" or "off"
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
