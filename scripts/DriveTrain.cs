using UnityEngine;
using System.Collections.Generic;
using ErrorHandling;

public class DriveTrain : MonoBehaviour
{
		// A dictionary to hold all of the mecanum wheels and the two wheel colliders that correspond ot them
		public Dictionary<int, List<UnityRigidNode>> mecanumWheels = new Dictionary<int, List<UnityRigidNode>> ();

		// A dictionary to hold all of the normal wheels (each wheel has a single motor controller, will probably be depreicated soon because we have to accomodate things like chain drives)
		public Dictionary<int, List<UnityRigidNode>> wheels = new Dictionary<int, List<UnityRigidNode>> ();

		// Will drive a single motor with a given speed (and brakeTorque)
		public static void setMotor (UnityRigidNode wheel, float speed, float brakeTorque)
		{
				if (wheel.GetSkeletalJoint ().cDriver.GetInfo<WheelDriverMeta> ().type != WheelType.NOT_A_WHEEL) {
						wheel.GetWheelCollider ().brakeTorque = brakeTorque;
						wheel.GetWheelCollider ().motorTorque = speed;
						wheel.GetConfigJoint ().targetAngularVelocity = new Vector3 (speed * 6 * Time.deltaTime, 0, 0);
				}
		}

		// A helper function to set a set of motors assigned to a PWM port
		public static void setListOfMotors (List<UnityRigidNode> setOfWheels, float speed, float brakeTorque)
		{
				foreach (UnityRigidNode wheel in setOfWheels) {
						setMotor (wheel, speed, 0);
				}
		}

		// Will set all of the motors (in the given Dictionary) to a value
		public static void setAllWheels (Dictionary<int, List<UnityRigidNode>> dictionary, float speed, float brakeTorque, params int[] pwmPorts)
		{
				foreach (KeyValuePair<int, List<UnityRigidNode>> listOfMotors in dictionary) {
						foreach (UnityRigidNode motor in listOfMotors.Value) {
								if (motor.GetSkeletalJoint ().cDriver.GetInfo<WheelDriverMeta> ().type != WheelType.NOT_A_WHEEL) {
										motor.GetWheelCollider ().motorTorque = speed;
										motor.GetWheelCollider ().brakeTorque = brakeTorque;
								}
						}
				}
		}

		public static void stopAllWheelColliders (Dictionary<int, List<UnityRigidNode>> wheels)
		{
				foreach (KeyValuePair<int, List<UnityRigidNode>> wheelSet in wheels) {
						foreach (UnityRigidNode wheel in wheelSet.Value) {
								wheel.GetWheelCollider ().motorTorque = 0;
								wheel.GetWheelCollider ().brakeTorque = 500;
								wheel.GetConfigJoint ().targetAngularVelocity = new Vector3 (0, 0, 0);
						}
				}
		}

		public static void initiateMecanum (List<UnityRigidNode> wheels)
		{
				foreach (UnityRigidNode wheel in wheels) {
						//wheel.GetWheelCollider ().transform.rotation = Quaternion.Euler(45, 90, 270);
						wheel.GetWheelCollider ().transform.Rotate (0, -315, 0);
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
								//try {
								// If its a unique port, we won't have problems
								if (!functionOutput.ContainsKey (unityNode.GetPortA ())) {
										functionOutput.Add (unityNode.GetPortA (), unityNode);
										//Debug.Log (string.Format ("Wheel Name: {0}, PWM Port {1}.", unityNode.GetWheelCollider ().name, unityNode.GetPortA ()));
						
										// If it isn't however, the user will have issues
								} else {
										//throw new PWMAssignedException (string.Format ("Error, PWM port {0} has already been assigned! (A.K.A: your trying to break the rules)", unityNode.GetPortA ()));
								}
								//} catch (PWMAssignedException e) {
								//		Debug.Log (e);
								//}
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
	

		// Goes through each wheel and edits the physics material of each wheel to match that of a real mecanum wheel
		// If we use wheelColliders, this might not be necessary anymore.
		/*public static void initiateMecanumFriction (Dictionary<int, UnityRigidNode> nodes)
		{
		
				foreach (KeyValuePair<int, UnityRigidNode> node in nodes) {
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
		}*/


		/*************_________Drive_Controllers________*************/


		// Example Tank drive for 4-wheeled vehicles
		public static void TankDrive (Dictionary<int, List<UnityRigidNode>> wheels, float speed, int frontLeft, int frontRight, int backLeft, int backRight)
		{

				// Equations
				DriveTrain.setListOfMotors (wheels [frontLeft], 5 * (Input.GetAxis ("Vertical") + Input.GetAxis ("Horizontal")), 0);
				DriveTrain.setListOfMotors (wheels [frontRight], -5 * (Input.GetAxis ("Vertical") - Input.GetAxis ("Horizontal")), 0);
				DriveTrain.setListOfMotors (wheels [backLeft], 5 * (Input.GetAxis ("Vertical") - Input.GetAxis ("Horizontal")), 0);
				DriveTrain.setListOfMotors (wheels [backRight], -5 * (Input.GetAxis ("Vertical") + Input.GetAxis ("Horizontal")), 0);

				// In case the user is using custom controls... It still need strafing... We will work on that later
				if (Input.GetKey (Controls.forward)) {
						setAllWheels (wheels, speed, 0, frontLeft, frontRight, backLeft, backRight);
				}
				if (Input.GetKey (Controls.backward)) {
						setAllWheels (wheels, speed, 0, frontLeft, frontRight, backLeft, backRight);
				}
				if (Input.GetKey (Controls.left)) {
						setListOfMotors (wheels [frontLeft], -speed, 0);
						setListOfMotors (wheels [backLeft], -speed, 0);
						setListOfMotors (wheels [frontRight], speed, 0);
						setListOfMotors (wheels [backRight], speed, 0);
				}
				if (Input.GetKey (Controls.right)) {
						setListOfMotors (wheels [frontLeft], speed, 0);
						setListOfMotors (wheels [backLeft], speed, 0);
						setListOfMotors (wheels [frontRight], -speed, 0);
						setListOfMotors (wheels [backRight], -speed, 0);
				}
		}

		// Example Drive Controller for swerve drive. I assume you are not going to do a swerve mecanum drive, but who knows...
		public static void SwerveDrive (Dictionary<int, List<UnityRigidNode>> wheels, float speed, float rotationSpeed)
		{
				// Each wheel will be driven, with a given speed, and roated, with a given rotation speed, in the same manner.
				foreach (KeyValuePair<int, List<UnityRigidNode>> wheelSet in wheels) {
						foreach (UnityRigidNode wheel in wheelSet.Value) {
								// Rotates each wheel at a given rate as the user uses the horizontal axese
								wheel.GetWheelCollider ().brakeTorque = 0;
								wheel.GetWheelCollider ().transform.Rotate (0, rotationSpeed * Time.deltaTime * Input.GetAxis ("Horizontal"), 0);
								wheel.GetConfigJoint ().transform.Rotate (0, 0, rotationSpeed * Time.deltaTime * Input.GetAxis ("Horizontal"));
				
								// Then it drives each wheel another given rate
								wheel.GetWheelCollider ().motorTorque = speed * Input.GetAxis ("Vertical");
				
								// In case the user is using custom controls. This looks messy, but it should work
								if (Input.GetKey (Controls.forward)) {
										wheel.GetWheelCollider ().motorTorque = speed;
								}
								if (Input.GetKey (Controls.backward)) {
										wheel.GetWheelCollider ().motorTorque = -speed;
								}
								if (Input.GetKey (Controls.left)) {
										wheel.GetWheelCollider ().brakeTorque = 0;
										wheel.GetWheelCollider ().transform.Rotate (0, -rotationSpeed * Time.deltaTime, 0);
								}
								if (Input.GetKey (Controls.right)) {
										wheel.GetWheelCollider ().brakeTorque = 0;
										wheel.GetWheelCollider ().transform.Rotate (0, rotationSpeed * Time.deltaTime, 0);
								}
						}

				}
		}

		public static void MecanumDrive (Dictionary<int, List<UnityRigidNode>> wheels, float speed, int frontLeft, int frontRight, int backLeft, int backRight)
		{
		
				// Equations
				setListOfMotors (wheels [frontLeft], speed * (Input.GetAxis ("Vertical") - Input.GetAxis ("Horizontal")), 0);
				setListOfMotors (wheels [backLeft], speed * (Input.GetAxis ("Vertical") - Input.GetAxis ("Horizontal")), 0);
				setListOfMotors (wheels [frontRight], speed * (Input.GetAxis ("Vertical") + Input.GetAxis ("Horizontal")), 0);
				setListOfMotors (wheels [backRight], speed * (Input.GetAxis ("Vertical") + Input.GetAxis ("Horizontal")), 0);
		
				// In case the user is using custom controls
				if (Input.GetKey (Controls.forward)) {
						setAllWheels (wheels, speed, 0, frontLeft, frontRight, backLeft, backRight);
				}
				if (Input.GetKey (Controls.backward)) {
						setAllWheels (wheels, speed, 0, frontLeft, frontRight, backLeft, backRight);
				}
				if (Input.GetKey (Controls.left)) {
						setListOfMotors (wheels [frontLeft], -speed, 0);
						setListOfMotors (wheels [backLeft], -speed, 0);
						setListOfMotors (wheels [frontRight], speed, 0);
						setListOfMotors (wheels [backRight], speed, 0);
				}
				if (Input.GetKey (Controls.right)) {
						setListOfMotors (wheels [frontLeft], speed, 0);
						setListOfMotors (wheels [backLeft], speed, 0);
						setListOfMotors (wheels [frontRight], -speed, 0);
						setListOfMotors (wheels [backRight], -speed, 0);
				}
		}

		// Don't Question It
		public static void DiscoDrive (Dictionary<int, List<UnityRigidNode>> dictionary, int frontLeft, int frontRight, int backLeft, int backRight)
		{
				setListOfMotors (dictionary [frontLeft], -150, 0);
				setListOfMotors (dictionary [backLeft], -150, 0);
				setListOfMotors (dictionary [frontRight], 150, 0);
				setListOfMotors (dictionary [backRight], 150, 0);
		}

		// Soon
		public static void updateMotors (Dictionary<int, UnityRigidNode> motor, params float[] values)
		{
			
		}
}


// A class to hold functions for all other joints
public class otherJoints : MonoBehaviour
{

		// Set a given configurable joint's target velocity
		public static void setConfigJointMotorX (UnityRigidNode configJoint, float speed)
		{
				configJoint.GetConfigJoint ().targetVelocity = new Vector3 (speed, 0, 0);
		}
	
		// Delegate Experimentation. These may or may not be helpful in the future. (I still have yet to master lambdas, but I will someday)
		//public static delegate void mydelegate (UnityRigidNode node,float speed);

		//static mydelegate setServo = new mydelegate (setConfigJointMotorX);
		//static mydelegate setPneumatic = new mydelegate (setConfigJointMotorX);

		public static void freeXDrive (UnityRigidNode node)
		{
				JointDrive newJointDrive = new JointDrive ();
				newJointDrive.maximumForce = Mathf.Infinity; // Probably not the wisest thing to do, but I shouldn't need to use this function at all.
				newJointDrive.mode = JointDriveMode.Velocity;

				node.GetConfigJoint ().angularXDrive = newJointDrive;
		}
}


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
	
		// Updates a wheel at the given PWM port with a given value
		// Note that this also returns true or false, because if a motor is set to 
		public void updateWheel (Dictionary<int, UnityRigidNode> wheels, int givenPWM, float speed)
		{
				if (speed != 0) {
						DriveTrain.setMotor (wheels [givenPWM], speed, 0);
						
				} else {
						DriveTrain.setMotor (wheels [givenPWM], 0, 100);
				}
		}

		// Defaults to a value of 10 (60 PSI)... This is not a realistic value, because velocity depends on the volume of the tank as well as the air pressure. We can figure this out, but it might be something for super happy fun time.
		public void updatePiston (Dictionary<Tuple<int, int>, UnityRigidNode> piston, float pistonPort1, float pistonPort2)
		{
				otherJoints.setConfigJointMotorX (piston [new Tuple<int, int> ((int)pistonPort1, (int)pistonPort2)], 10);
		}

		public void updateAllWheels (Dictionary<int, UnityRigidNode> wheels, float[] pwm)
		{
				for (int i = 0; i<pwm.Length; i++) {
						if (wheels.ContainsKey (i+1)) {
								updateWheel (wheels, i+1, pwm [i]);
						}
				}
				// For each float in struct?
				// I may have to do this explicitly
		}
}
