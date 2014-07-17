using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class Init : MonoBehaviour
{		
		// We will need these
		public Dictionary<int, UnityRigidNode> PWMAssignments = new Dictionary<int, UnityRigidNode> ();
		public float speed = 10;
		public int[] motors = {1,2,3,4};

		void Start ()
		{
				string homePath = (System.Environment.OSVersion.Platform == PlatformID.Unix || System.Environment.OSVersion.Platform == PlatformID.MacOSX) ? System.Environment.GetEnvironmentVariable ("HOME") : System.Environment.ExpandEnvironmentVariables ("%HOMEDRIVE%%HOMEPATH%");
				//Now you can use a default directory to load all of the files
				Directory.CreateDirectory (homePath + "/Documents/Skeleton/Skeleton/");
				string path = homePath + "/Documents/Skeleton/Skeleton/";
				
	
				List<RigidNode_Base> names = new List<RigidNode_Base> ();
				RigidNode_Base.NODE_FACTORY = new UnityRigidNodeFactory ();
				RigidNode_Base skeleton = BXDJSkeleton.ReadSkeleton (homePath + "/Documents/Skeleton/Skeleton/skeleton.bxdj");

				skeleton.ListAllNodes (names);
				foreach (RigidNode_Base node in names) {
						UnityRigidNode uNode = (UnityRigidNode)node;
						uNode.CreateTransform (transform);
									
						uNode.CreateMesh (path + uNode.GetModelFileName ());
						uNode.CreateJoint ();
						

						
					
				}
				transform.Rotate (new Vector3 (-90.0f, 0.0f, 0.0f));
				//transform.localScale = new Vector3 (0.01f, 0.01f, 0.01f);

				PWMAssignments = Controls.assignMotors (skeleton);

				// These can be whatever you want
				Controls.setControls ("T", "G", "F", "H");

		}

		void FixedUpdate ()
		{
				
				// If you want to use the default vertical/horizontal axese
				if (Input.GetAxis ("Vertical") != 0 || Input.GetAxis ("Horizontal") != 0) {
						// For use if you just want to work with analog joysticks and the default vertical/horizontal axis
						Controls.setMotor (PWMAssignments [4], 10 * (Input.GetAxis ("Vertical") + Input.GetAxis ("Horizontal")), 0);
						Controls.setMotor (PWMAssignments [3], 10 * (Input.GetAxis ("Vertical") - Input.GetAxis ("Horizontal")), 0);
						Controls.setMotor (PWMAssignments [1], 10 * (Input.GetAxis ("Vertical") + Input.GetAxis ("Horizontal")), 0);
						Controls.setMotor (PWMAssignments [2], 10 * (Input.GetAxis ("Vertical") - Input.GetAxis ("Horizontal")), 0);

						// For use with JoySticks if you want to turn left and right using the two bumpers
						if (Input.GetKey (KeyCode.JoystickButton4)) {
							Controls.setMotor (PWMAssignments [4], -speed, 0);
							Controls.setMotor (PWMAssignments [1], -speed, 0);
							Controls.setMotor (PWMAssignments [2], speed, 0);
							Controls.setMotor (PWMAssignments [3], speed, 0);
						} else if (Input.GetKey (KeyCode.JoystickButton5)) {
							Controls.setMotor (PWMAssignments [4], speed, 0);
							Controls.setMotor (PWMAssignments [1], speed, 0);
							Controls.setMotor (PWMAssignments [2], -speed, 0);
							Controls.setMotor (PWMAssignments [3], -speed, 0);
						}

				// If you are using manual controls
				} else if (Input.GetKey (Controls.forward) || Input.GetKey (Controls.backward) || Input.GetKey (Controls.left) || Input.GetKey (Controls.right)) {
						// Uncomment to use as necessary
					
						// To be used with custom keys (which have no axis values)
						// These have a defined value of True or False, I have not found a way to treat them as axes like the WSAD keys
						if (Input.GetKey (Controls.forward)){
										Controls.setSetOfMotors (PWMAssignments, speed, 0, motors);
										//Controls.setMotor (PWMAssignments [1], speed);
										//Controls.setMotor (PWMAssignments [2], speed);
										//Controls.setMotor (PWMAssignments [3], speed);
										//Controls.setMotor (PWMAssignments [4], speed);
								}
								if (Input.GetKey (Controls.backward)) {
										Controls.setSetOfMotors (PWMAssignments, -speed, 0, motors);
										//Controls.setMotor (PWMAssignments [1], -speed);
										//Controls.setMotor (PWMAssignments [2], -speed);
										//Controls.setMotor (PWMAssignments [3], -speed);
										//Controls.setMotor (PWMAssignments [4], -speed);
								}
								if (Input.GetKey (Controls.left)) {
										Controls.setMotor (PWMAssignments [4], -speed, 0);
										Controls.setMotor (PWMAssignments [1], -speed, 0);
										Controls.setMotor (PWMAssignments [2], speed, 0);
										Controls.setMotor (PWMAssignments [3], speed, 0);
								}
								if (Input.GetKey (Controls.right)) {
										Controls.setMotor (PWMAssignments [4], speed, 0);
										Controls.setMotor (PWMAssignments [1], speed, 0);
										Controls.setMotor (PWMAssignments [2], -speed, 0);
										Controls.setMotor (PWMAssignments [3], -speed, 0);
								}

					
						// If you aren't pressing anything, the wheels will begin to stop
				} else {
						Controls.stopAllMotors (PWMAssignments);
				}


		}
}

