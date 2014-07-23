using UnityEngine;
using System.Collections.Generic;

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

		public static void initiateMecanum (UnityRigidNode wheel)
		{

				//wheel.GetWheelCollider ().transform.rotation = Quaternion.Euler(45, 90, 270);
				wheel.GetWheelCollider ().transform.Rotate (0, -315, 0);

				// Friction stuff will go here
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
		public static void tankDrive (Dictionary<int, List<UnityRigidNode>> wheels, float speed, int frontLeft, int frontRight, int backLeft, int backRight)
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
		public static void swerveDrive (Dictionary<int, List<UnityRigidNode>> wheels, float speed, float rotationSpeed)
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

		public void MecanumDrive (Dictionary<int, List<UnityRigidNode>> wheels, float speed, int frontLeft, int frontRight, int backLeft, int backRight)
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
		public static void discoDrive (Dictionary<int, List<UnityRigidNode>> dictionary, int frontLeft, int frontRight, int backLeft, int backRight)
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

