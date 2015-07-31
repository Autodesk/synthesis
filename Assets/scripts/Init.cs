using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class Init : MonoBehaviour
{
    // We will need these
    public const float PHYSICS_MASS_MULTIPLIER = 0.001f;

	public const float FORMAT_3DS_SCALE = 0.2558918f;

    private GUIController gui;

	private PhysicMaterial chuteMaterial;

    private RigidNode_Base skeleton;
    private GameObject activeRobot;
	private GameObject activeField;
	private GameObject cameraObject;
	private DynamicCamera dynamicCamera;
	private GameObject light;
	private UnityFieldDefinition field;
	private List<GameObject> totes;

    private unityPacket udp;
	private string filePath;
	//main node of robot from which speed and other stats are derived
	private GameObject mainNode;
	//sizes and places window and repositions it based on screen size
	private Rect statsWindowRect;
	private Rect helpWindowRect;
	private GUIContent helpButtonContent;
	private Rect helpButtonRect;
	private float acceleration;
	private float angvelo;
	private float speed;
	private float weight;
	private float time;
	private bool time_stop;
	private float oldSpeed;
	private bool showStatWindow;
	private bool showHelpWindow;
	private Quaternion rotation;

    /// <summary>
    /// Frames before the robot gets reloaded, or -1 if no reload is queued.
    /// </summary>
    /// <remarks>
    /// This allows reloading the robot to be delayed until a "Loading" dialog can be drawn.
    /// </remarks>
    private volatile int reloadRobotInFrames;

	private FileBrowser fieldBrowser = null;
	private bool fieldLoaded = false;

    public Init()
    {
		udp = new unityPacket ();
		Debug.Log (filePath);

		statsWindowRect = new Rect (Screen.width - 320, 20, 300, 150);
		helpWindowRect = new Rect (300, 100, 400, 150);
		helpButtonRect = new Rect (Screen.width - 103, 0, 100, 25);
		time_stop = false;

		reloadRobotInFrames = -1;
		showStatWindow = false;
		showHelpWindow = true;
		rotation = Quaternion.identity;
    }

	//displays stats like speed and acceleration
	public void StatsWindow(int windowID) {
		
		GUI.Label (new Rect (10, 20, 300, 50), "Speed: " + Math.Round(speed, 1).ToString() + " m/s");
		GUI.Label (new Rect (150, 20, 300, 50),Math.Round(speed*3.28084, 1).ToString() + " ft/s");
		GUI.Label (new Rect (10, 40, 300, 50), "Acceleration: " + Math.Round(acceleration, 1).ToString() + " m/s^2");
		GUI.Label (new Rect (175, 40, 300, 50),Math.Round(acceleration*3.28084, 1).ToString() + " ft/s^2");
		GUI.Label (new Rect (10, 60, 300, 50), "Angular Velocity: " + Math.Round(angvelo, 1).ToString() + " rad/s");
		GUI.Label (new Rect (10, 80, 300, 50), "Weight: " + weight.ToString() + " lbs");
		GUI.Label (new Rect (10, 120, 300, 50), "Timer: " + Math.Round (time, 1).ToString() + " sec");
		if(GUI.Button (new Rect (120, 120, 80, 25), "Start/Stop"))
		{
			time_stop = !time_stop;
		}
		
		if (GUI.Button (new Rect (210, 120, 80, 25), "Reset")) 
		{
			time = 0;
		}

		GUI.DragWindow (new Rect (0, 0, 10000, 10000));
	}

	/// <summary>
	/// Exits the window.
	/// </summary>
	/// <param name="windowID">Window I.</param>
	public void HelpWindow(int windowID)
	{
		float topGap = 10;
		float buttonGap = 20;
		float buttonWidth = (helpWindowRect.width - (buttonGap * 3)) / 2.0f;
		float buttonHeight = helpWindowRect.height - (buttonGap * 2) - topGap;
		int leftX = 75;
		int leftXOffset = 400;
		int heightGap = 45;
		int underlineGap = 4;

		GUIStyle labelSkin = new GUIStyle (GUI.skin.label);
		labelSkin.fontSize = 24;
	
		GUI.Label (new Rect (leftX, 1 * heightGap, 300, 50), "Action", labelSkin);
		GUI.Label (new Rect (leftX, (1 * heightGap) + underlineGap, 300, 50), "_____", labelSkin);
		GUI.Label (new Rect (leftX, 2 * heightGap, 300, 50), "Menu:", labelSkin);
		GUI.Label (new Rect (leftX, 3 * heightGap, 300, 50), "Reset Robot:", labelSkin);
		GUI.Label (new Rect (leftX, 4 * heightGap, 300, 50), "Driverstation View:", labelSkin);
		GUI.Label (new Rect (leftX, 5 * heightGap, 300, 50), "Orbit View:", labelSkin);
		GUI.Label (new Rect (leftX, 6 * heightGap, 300, 50), "Free Roam View:", labelSkin);
		GUI.Label (new Rect (leftX, 7 * heightGap, 300, 50), "To Drive Robot:", labelSkin);
		GUI.Label (new Rect (leftX, 8 * heightGap, 300, 50), "Toggle stats window:", labelSkin);
		GUI.Label (new Rect (leftXOffset, 1 * heightGap, 300, 50), "Key", labelSkin);
		GUI.Label (new Rect (leftXOffset, (1 * heightGap) + underlineGap, 300, 50), "___", labelSkin);
		GUI.Label (new Rect (leftXOffset, 2 * heightGap, 300, 50), "[ESC]", labelSkin);
		GUI.Label (new Rect (leftXOffset, 3 * heightGap, 300, 50), "[R]", labelSkin);
		GUI.Label (new Rect (leftXOffset, 4 * heightGap, 300, 50), "[D]", labelSkin);
		GUI.Label (new Rect (leftXOffset, 5 * heightGap, 300, 50), "[O]", labelSkin);
		GUI.Label (new Rect (leftXOffset, 6 * heightGap, 300, 50), "[F]", labelSkin);
		GUI.Label (new Rect (leftXOffset, 7 * heightGap, 300, 50), "[Arrow Keys]", labelSkin);
		GUI.Label (new Rect (leftXOffset, 8 * heightGap, 300, 50), "[S]", labelSkin);
	}

	public void ShowOrient()
	{
		List<string> titles = new List<string> ();
		titles.Add ("Left");
		titles.Add ("Right");
		titles.Add ("Forward");
		titles.Add ("Back");
		titles.Add ("Save Orientation");
		titles.Add ("Close");
		titles.Add ("Default");
		
		List<Rect> rects = new List<Rect> ();
		rects.Add (new Rect(50, 150, 75, 30));
		rects.Add (new Rect(175, 150, 75, 30));
		rects.Add (new Rect(112, 115, 75, 30));
		rects.Add (new Rect(112, 185, 75, 30));
		rects.Add (new Rect (95, 55, 110, 30));
		rects.Add (new Rect (230, 20, 50, 30));
		rects.Add (new Rect (20, 20, 70, 30));

		TextWindow oWindow = new TextWindow ("Orient Robot", new Rect ((Screen.width / 2) - 150, (Screen.height / 2) - 125, 300, 250),
		                                     new string[0], new Rect[0], titles.ToArray (), rects.ToArray ());

		gui.AddWindow("Orient Robot", oWindow, (object o)=>{
			switch((int)o)
			{
			case 0:
				activeRobot.transform.Rotate(new Vector3(activeRobot.transform.rotation.x, activeRobot.transform.rotation.y,activeRobot.transform.rotation.z + 90));
                break;
			case 1:	
				activeRobot.transform.Rotate (new Vector3(activeRobot.transform.rotation.x, activeRobot.transform.rotation.y,activeRobot.transform.rotation.z - 90));
				break;
			case 2:
				activeRobot.transform.Rotate(new Vector3(activeRobot.transform.rotation.x + 90, activeRobot.transform.rotation.y,activeRobot.transform.rotation.z));
				break;
			case 3:;
				activeRobot.transform.Rotate(new Vector3(activeRobot.transform.rotation.x - 90, activeRobot.transform.rotation.y,activeRobot.transform.rotation.z));
				break;
			case 4:
				rotation = activeRobot.transform.rotation;
				Debug.Log(rotation);
				break;
			case 5:
				oWindow.Active = false;
				break;
			case 6:
				activeRobot.transform.rotation = Quaternion.identity;
				break;

			}			
		});
	}

	void HideGuiSidebar()
	{
		gui.guiVisible = false;
		dynamicCamera.EnableMoving ();
	}

	void ShowGuiSidebar()
	{
		dynamicCamera.DisableMoving();
		showHelpWindow = false;
	}

	[STAThread]
    void OnGUI()
    {
		// Draws stats window on to GUI
		if(showStatWindow)
			GUI.Window(0, statsWindowRect, StatsWindow, "Stats");

		// Draws stats window on to GUI
		if (showHelpWindow)
		{
			int windowWidth = 900;
			int windowHeight = 450;
			float paddingX = (Screen.width - windowWidth) / 2.0f;
			float paddingY = (Screen.height - windowHeight) / 2.0f;
			helpWindowRect = new Rect (paddingX, paddingY, windowWidth, windowHeight);
			GUI.Window (0, helpWindowRect, HelpWindow, "Help");
		}

        if (gui == null)
        {
            gui = new GUIController();
			gui.hideGuiCallback = HideGuiSidebar;
			gui.showGuiCallback = ShowGuiSidebar;

            gui.AddWindow("Load Robot", new FileBrowser("Load Robot"), (object o) =>
            {
                string fileLocation = (string) o;
                // If dir was selected...
                if (File.Exists(fileLocation + "\\skeleton.bxdj"))
				{
                    fileLocation += "\\skeleton.bxdj";
				}
                DirectoryInfo parent = Directory.GetParent(fileLocation);
                if (parent != null && parent.Exists && File.Exists(parent.FullName + "\\skeleton.bxdj"))
                {
                    this.filePath = parent.FullName + "\\";
                    reloadRobotInFrames = 2;
                }
                else
                {
                    UserMessageManager.Dispatch("Invalid selection!", 10f);
                }

				dynamicCamera.EnableMoving();
			});

            gui.AddAction("Reset Robot", () =>
            {
                resetRobot();
            });
			//shows button to manually orient the robot
			ShowOrient();

            if (!File.Exists(filePath + "\\skeleton.bxdj"))
            {
                gui.DoAction("Load Model");
            }

			gui.AddWindow ("Switch View", new DialogWindow("Switch View",
			    "Driver Station [D]", "Orbit Robot [R]", "Freeroam [F]"), (object o) =>
			    {
					HideGuiSidebar();

					switch ((int) o) {
					case 0:
						dynamicCamera.SwitchCameraState(new DynamicCamera.DriverStationState(dynamicCamera));
						break;
					case 1:
						dynamicCamera.SwitchCameraState(new DynamicCamera.OrbitState(dynamicCamera));
						dynamicCamera.EnableMoving();
						break;
					case 2:
						dynamicCamera.SwitchCameraState(new DynamicCamera.FreeroamState(dynamicCamera));
						break;
					default:
						Debug.Log("Camera state not found: " + (string) o);
						break;
					}
				});


			gui.AddWindow ("Quit Simulation", new DialogWindow ("Exit?", "Yes", "No"), (object o) =>
			               {
				if ((int) o == 0) {
					Application.Quit();
				}
			});
        }

		if (fieldLoaded) 
		{
			// The Menu bottom on the top left corner
			GUI.Window (1, new Rect (0, 0, gui.GetSidebarWidth(), 25), 
	        	(int windowID) =>
			{
				if (GUI.Button (new Rect (0, 0, gui.GetSidebarWidth(), 25), "Menu"))
					gui.EscPressed ();

			},
				""
			);
		}

		helpButtonRect = new Rect (Screen.width - 25, 0, 25, 25);

		// The Help button on top right corner
		GUI.Window (2, helpButtonRect, 
        	(int windowID) =>
            {
				if (GUI.Button (new Rect (0, 0, 25, 25), helpButtonContent))
				{	
					showHelpWindow = !showHelpWindow;
				}
			},
		""
		);

		if (Input.GetMouseButtonUp (0) && !gui.ClickedInsideWindow ())
		{
			HideGuiSidebar();
			gui.HideAllWindows();
		}

		if (fieldBrowser == null) {
			fieldBrowser = new FileBrowser ("Load Field", false);
			fieldBrowser.Active = true;
			fieldBrowser.OnComplete += (object obj) => 
			{
				fieldBrowser.Active = true;
				string fileLocation = (string) obj;
				// If dir was selected...
				if (File.Exists(fileLocation + "\\definition.bxdf"))
				{
					fileLocation += "\\definition.bxdf";
				}
				DirectoryInfo parent = Directory.GetParent(fileLocation);
				if (parent != null && parent.Exists && File.Exists(parent.FullName + "\\definition.bxdf"))
				{
					this.filePath = parent.FullName + "\\";
					activeField = new GameObject("Field");
					
					FieldDefinition_Base.FIELDDEFINITION_FACTORY = delegate()
					{
						return new UnityFieldDefinition();
					};

					Debug.Log (filePath);
					field = (UnityFieldDefinition)BXDFProperties.ReadProperties(filePath + "definition.bxdf");
					field.CreateTransform(activeField.transform);
					field.CreateMesh(filePath + "mesh.bxda");
					fieldLoaded = true;
					fieldBrowser.Active = false;
					reloadRobotInFrames = 2;
				}
				else
				{
					UserMessageManager.Dispatch("Invalid selection!", 10f);
				}
			};
		}

		if (showHelpWindow && Input.GetMouseButtonUp (0) && !auxFunctions.MouseInWindow (helpWindowRect) && !auxFunctions.MouseInWindow (helpButtonRect))
			showHelpWindow = false;

		gui.guiBackgroundVisible = showHelpWindow;

		fieldBrowser.Render ();

		if(fieldLoaded)
        	gui.Render();

		UserMessageManager.Render();

		if (reloadRobotInFrames >= 0)
        {
            GUI.backgroundColor = new Color(1, 1, 1, 0.5f);
            GUI.Box(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 25, 200, 50), "Loading... Please Wait", gui.BlackBoxStyle);
        }
    }

    /// <summary>
    /// Repositions the robot so it is aligned at the center of the field, and resets all the
    /// joints, velocities, etc..
    /// </summary>
    private void resetRobot()
    {	
        if (activeRobot != null && skeleton != null)
        {
            var unityWheelData = new List<GameObject>();
			var unityWheels = new List<UnityRigidNode>();
            // Invert the position of the root object
            activeRobot.transform.localPosition = new Vector3(1f, 1f, -0.5f);
			activeRobot.transform.rotation = rotation;
			activeRobot.transform.localRotation = Quaternion.identity;
            var nodes = skeleton.ListAllNodes();
            foreach (RigidNode_Base node in nodes)
            {
                UnityRigidNode uNode = (UnityRigidNode) node;
				uNode.unityObject.transform.localPosition = new Vector3(0f, 0f, -5f);
				uNode.unityObject.transform.localRotation = Quaternion.identity;
                if (uNode.unityObject.rigidbody != null)
                {
                    uNode.unityObject.rigidbody.velocity = Vector3.zero;
                    uNode.unityObject.rigidbody.angularVelocity = Vector3.zero;
                }
                if (uNode.HasDriverMeta<WheelDriverMeta>()&& uNode.wheelCollider != null && uNode.GetDriverMeta<WheelDriverMeta>().isDriveWheel)
                {
                    unityWheelData.Add(uNode.wheelCollider);
					unityWheels.Add (uNode);
                }
            }

			bool isMecanum = false;

            if (unityWheelData.Count > 0)
            {
                auxFunctions.OrientRobot(unityWheelData, activeRobot.transform);
				foreach (RigidNode_Base node in nodes)
				{
					UnityRigidNode uNode = (UnityRigidNode) node;			
					if (uNode.HasDriverMeta<WheelDriverMeta>()&& uNode.wheelCollider != null)
					{
						unityWheelData.Add(uNode.wheelCollider);

						if(uNode.GetDriverMeta<WheelDriverMeta>().GetTypeString().Equals("Mecanum"))
						{
							isMecanum = true;
							uNode.unityObject.GetComponent<BetterWheelCollider>().wheelType = (int)WheelType.MECANUM;
						}

						if(uNode.GetDriverMeta<WheelDriverMeta>().GetTypeString().Equals("Omni Wheel"))
							uNode.unityObject.GetComponent<BetterWheelCollider>().wheelType = (int)WheelType.OMNI;
					}
				}
				auxFunctions.rightRobot(unityWheelData, activeRobot.transform);

				if(isMecanum)
			   	{
					float sumX = 0;
					float sumZ = 0;

					foreach(UnityRigidNode uNode in unityWheels)
					{
						sumX += uNode.wheelCollider.transform.localPosition.x;
						sumZ += uNode.wheelCollider.transform.localPosition.z;
					}

					float avgX = sumX / unityWheels.Count;
					float avgZ = sumZ / unityWheels.Count;

					foreach(UnityRigidNode uNode in unityWheels)
					{
						if(uNode.unityObject.GetComponent<BetterWheelCollider>().wheelType == (int)WheelType.MECANUM)
						{
							if((avgX > uNode.wheelCollider.transform.localPosition.x && avgZ < uNode.wheelCollider.transform.localPosition.z) ||
							   (avgX < uNode.wheelCollider.transform.localPosition.x && avgZ > uNode.wheelCollider.transform.localPosition.z))
								uNode.unityObject.GetComponent<BetterWheelCollider>().wheelAngle = -45;

							else
								uNode.unityObject.GetComponent<BetterWheelCollider>().wheelAngle = 45;
						}
					}
				}
            }
        }

		foreach (GameObject o in totes)
		{
			GameObject.Destroy(o);
		}

		totes.Clear ();
    }

    private void TryLoadRobot()
    {
        if (activeRobot != null)
        {
            skeleton = null;
            UnityEngine.Object.Destroy(activeRobot);
        }
        if (filePath != null && skeleton == null)
        {
			//Debug.Log (filePath);
            List<Collider> meshColliders = new List<Collider>();
            activeRobot = new GameObject("Robot");
            activeRobot.transform.parent = transform;

            List<RigidNode_Base> names = new List<RigidNode_Base>();
            RigidNode_Base.NODE_FACTORY = delegate()
            {
                return new UnityRigidNode();
            };

            skeleton = BXDJSkeleton.ReadSkeleton(filePath + "skeleton.bxdj");
			//Debug.Log(filePath + "skeleton.bxdj");
            skeleton.ListAllNodes(names);
            foreach (RigidNode_Base node in names)
            {
                UnityRigidNode uNode = (UnityRigidNode) node;

                uNode.CreateTransform(activeRobot.transform);
                uNode.CreateMesh(filePath + uNode.modelFileName);
                uNode.CreateJoint();

				Debug.Log("Joint");

                meshColliders.AddRange(uNode.unityObject.GetComponentsInChildren<Collider>());
            }

            {   // Add some mass to the base object
                UnityRigidNode uNode = (UnityRigidNode) skeleton;
                uNode.unityObject.transform.rigidbody.mass += 20f * PHYSICS_MASS_MULTIPLIER; // Battery'
                Vector3 vec = uNode.unityObject.rigidbody.centerOfMass;
                vec.y *= 0.9f;
                uNode.unityObject.rigidbody.centerOfMass = vec;
            }

            auxFunctions.IgnoreCollisionDetection(meshColliders);
            resetRobot();
        }
        else
        {
            Debug.Log("unityWheelData is null...");
        }
		HideGuiSidebar();
    }

	private void TryLoadField()
	{
		if (activeRobot != null)
		{
			skeleton = null;
			UnityEngine.Object.Destroy(activeRobot);
		}
		
		if (activeField != null)
		{
			UnityEngine.Object.Destroy(activeField);
		}
		
		if (filePath != null)
		{
			activeField = new GameObject("Field");
			
			FieldDefinition_Base.FIELDDEFINITION_FACTORY = delegate()
			{
				return new UnityFieldDefinition();
			};

			Debug.Log(filePath);
			field = (UnityFieldDefinition)BXDFProperties.ReadProperties(filePath + "definition.bxdf");
			field.CreateTransform(activeField.transform);
			field.CreateMesh(filePath + "mesh.bxda");
		}
		HideGuiSidebar();
	}

    void Start()
    {
		helpButtonContent = new GUIContent ("");
		helpButtonContent.image = Resources.Load ("Images/halp") as Texture2D;

        Physics.gravity = new Vector3(0, -9.8f, 0);
        Physics.solverIterationCount = 30;
		Physics.minPenetrationForPenalty = 0.001f;

		cameraObject = new GameObject ("Camera");
		cameraObject.AddComponent<Camera> ();
		cameraObject.GetComponent<Camera> ().backgroundColor = new Color (.3f, .3f, .3f);
		dynamicCamera = cameraObject.AddComponent<DynamicCamera> ();

		light = new GameObject ("Light");
		Light lightComponent = light.AddComponent<Light> ();
		lightComponent.type = LightType.Spot;
		lightComponent.intensity = 1.5f;
		lightComponent.range = 30f;
		lightComponent.spotAngle = 135;
		light.transform.position = new Vector3 (0f, 10f, 0f);
		light.transform.Rotate (90f, 0f, 0f);

		chuteMaterial = new PhysicMaterial("chuteMaterial");
		chuteMaterial.dynamicFriction = 0f;
		chuteMaterial.staticFriction = 0f;
		chuteMaterial.frictionCombine = PhysicMaterialCombine.Minimum;

		totes = new List<GameObject> ();

		filePath = Application.dataPath + "\\resources\\FieldOutput\\";

        reloadRobotInFrames = -1;
    }

    void OnEnable()
    {
        udp.Start();
    }

    void OnDisable()
    {
        udp.Stop();
    }

    void Update()
    {
		//Debug.Log(filePath);
        if (reloadRobotInFrames >= 0 && reloadRobotInFrames-- == 0)
        {
            reloadRobotInFrames = -1;
            TryLoadRobot();
        }

		// Orient Robot
		if (Input.GetKeyDown (KeyCode.R))
			gui.DoAction ("Reset Robot");

		// Show/Hide physics window
		if (Input.GetKeyDown (KeyCode.S))
			showStatWindow = !showStatWindow;
    }

    void FixedUpdate()
    {
		if (skeleton != null) 
		{
			List<RigidNode_Base> nodes = skeleton.ListAllNodes();

			unityPacket.OutputStatePacket packet = udp.GetLastPacket ();

			DriveJoints.UpdateAllMotors (skeleton, packet.dio);
			//TODO put this code in drivejoints, figure out nullreference problem with cDriver
			foreach(RigidNode_Base node in nodes)
			{
				UnityRigidNode uNode = (UnityRigidNode) node;
				if(uNode.GetSkeletalJoint() != null)
				{
					if(uNode.GetSkeletalJoint().GetJointType() == SkeletalJointType.LINEAR && uNode.GetSkeletalJoint().cDriver != null && uNode.GetSkeletalJoint().cDriver.GetDriveType() == JointDriverType.ELEVATOR)
					{
						ElevatorScript es = uNode.unityObject.GetComponent<ElevatorScript>();

						float[] pwm = packet.dio[0].pwmValues;
						if(Input.anyKey)
						{
							pwm[3] += (Input.GetKey(KeyCode.Alpha1) ? 1f : 0f);
							pwm[3] += (Input.GetKey(KeyCode.Alpha2) ? -1f : 0f);
						}
						if(es != null)
						{
							for(int i = 0; i < 8; i++)
							{
								if(uNode.GetSkeletalJoint().cDriver.portA == i)
									es.currentTorque = pwm[i]*2;
							}
						}
					}
				}
			} 
			DriveJoints.UpdateSolenoids (skeleton, packet.solenoid);

			#region HANDLE_SENSORS
			InputStatePacket sensorPacket = new InputStatePacket ();
			foreach (RigidNode_Base node in nodes) 
			{
				UnityRigidNode uNode = (UnityRigidNode)node;

				if (node.GetSkeletalJoint () == null)
					continue;

				foreach (RobotSensor sensor in node.GetSkeletalJoint().attachedSensors) 
				{
					int aiValue; //int between 0 and 1024, typically
					InputStatePacket.DigitalState dioValue;
					switch(sensor.type)
					{
					case RobotSensorType.POTENTIOMETER:
						if(node.GetSkeletalJoint() != null && node.GetSkeletalJoint() is RotationalJoint_Base)
						{
							float angle = DriveJoints.GetAngleBetweenChildAndParent (uNode) + ((RotationalJoint_Base)uNode.GetSkeletalJoint ()).currentAngularPosition;
							sensorPacket.ai [sensor.module - 1].analogValues [sensor.port - 1] = (int)sensor.equation.Evaluate (angle);
						}
						break;

					case RobotSensorType.ENCODER:
						throw new NotImplementedException();
						break;

					case RobotSensorType.LIMIT:
						throw new NotImplementedException();
						break;

					 case RobotSensorType.GYRO:
						throw new NotImplementedException();
						break;

					 case RobotSensorType.MAGNETOMETER:
						throw new NotImplementedException();
						break;

					 case RobotSensorType.ACCELEROMETER:
						throw new NotImplementedException();
						break;

					 case RobotSensorType.LIMIT_HALL:
						throw new NotImplementedException();
						break;
					}

				}
			}
			udp.WritePacket (sensorPacket);
			#endregion

			//finds main node of robot to use its rigidbody
			mainNode = GameObject.Find ("node_0.bxda");
			//calculates stats of robot
			if (mainNode != null) {
				speed = (float)Math.Abs (mainNode.rigidbody.velocity.magnitude);
				weight = (float)Math.Round (mainNode.rigidbody.mass * 2.20462 * 860, 1);
				angvelo = (float)Math.Abs (mainNode.rigidbody.angularVelocity.magnitude);
				acceleration = (float)(mainNode.rigidbody.velocity.magnitude - oldSpeed) / Time.deltaTime;
				oldSpeed = speed;
				if (!time_stop)
					time += Time.deltaTime;

				if(gui.guiVisible)
					mainNode.rigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationY;
				else
					mainNode.rigidbody.constraints = RigidbodyConstraints.None;
			}
		}
	}
}