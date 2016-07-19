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
	// robot stats for stats window
	private float acceleration;
	private float angvelo;
	private float speed;
	private float weight;
	private float time;
	private bool time_stop;
	private float oldSpeed;
	//Display windows or not
	private bool showStatWindow;
	//for orienting the robot
	private Quaternion rotation;
	//start/stop button on the stats window timer
	private string label;

    /// <summary>
    /// Frames before the robot gets reloaded, or -1 if no reload is queued.
    /// </summary>
    /// <remarks>
    /// This allows reloading the robot to be delayed until a "Loading" dialog can be drawn.
    /// </remarks>
    private volatile int reloadRobotInFrames;
	private volatile int reloadFieldInFrames;

	private FileBrowser fieldBrowser = null;
	private bool fieldLoaded = false;

    /// <summary>
	/// Default textures.
	/// </summary>
	private Texture2D buttonTexture;
    private Texture2D greyWindowTexture;
    private Texture2D darkGreyWindowTexture;
    private Texture2D lightGreyWindowTexture;
    private Texture2D transparentWindowTexture;
    /// <summary>
	/// Selected button texture.
	/// </summary>
	private Texture2D buttonSelected;
    /// <summary>
	/// Gravity-Regular font.
	/// </summary>
	private Font gravityRegular;
    private Font russoOne;
    /// <summary>
	/// Custom GUIStyle for windows.
	/// </summary>
	private GUIStyle statsWindow;
    private GUIStyle menuWindow;
    /// <summary>
	/// Custom GUIStyle for buttons.
	/// </summary>
	private GUIStyle statsButton;
    private GUIStyle menuButton;
    /// <summary>
	/// Custom GUIStyle for labels.
	/// </summary>s
	private GUIStyle statsLabel;

    /// <summary>
    /// Orient Robot Window
    /// </summary>
    private OverlayWindow oWindow;

    public Init()
    {
		udp = new unityPacket ();
		Debug.Log (filePath);

		statsWindowRect = new Rect (Screen.width - 320, 20, 300, 150);
		time_stop = false;

		reloadRobotInFrames = -1;
		reloadFieldInFrames = -1;
		showStatWindow = false;
		rotation = Quaternion.identity;
		label = "Stop";
    }

	//displays stats like speed and acceleration
	public void StatsWindow(int windowID) {
        //Custom style for windows
        statsWindow = new GUIStyle(GUI.skin.window);
        statsWindow.normal.background = greyWindowTexture;
        statsWindow.onNormal.background = greyWindowTexture;
        statsWindow.font = russoOne;
        statsWindow.fontSize = 15;

        //Custom style for buttons
        statsButton = new GUIStyle(GUI.skin.button);
        statsButton.font = gravityRegular;
        statsButton.normal.background = buttonTexture;
        statsButton.hover.background = buttonSelected;
        statsButton.active.background = buttonSelected;
        statsButton.onNormal.background = buttonSelected;
        statsButton.onHover.background = buttonSelected;
        statsButton.onActive.background = buttonSelected;
        statsButton.fontSize = 13;

        //Custom style for labels
        statsLabel = new GUIStyle(GUI.skin.label);
        statsLabel.font = gravityRegular;
        statsLabel.fontSize = 13;

        GUI.Label (new Rect (10, 20, 300, 50), "Speed: " + Math.Round(speed, 1).ToString() + " m/s - " + Math.Round(speed * 3.28084, 1).ToString() + " ft/s", statsLabel);
		GUI.Label (new Rect (10, 40, 300, 50), "Acceleration: " + Math.Round(acceleration, 1).ToString() + " m/s^2 - " + Math.Round(acceleration * 3.28084, 1).ToString() + " ft/s^2", statsLabel);
		GUI.Label (new Rect (10, 60, 300, 50), "Angular Velocity: " + Math.Round(angvelo, 1).ToString() + " rad/s", statsLabel);
		GUI.Label (new Rect (10, 80, 300, 50), "Weight: " + weight.ToString() + " lbs", statsLabel);
		GUI.Label (new Rect (10, 120, 300, 50), "Timer: " + Math.Round (time, 1).ToString() + " sec", statsLabel);
		
		if (GUI.Button (new Rect (210, 120, 80, 25), "Reset", statsButton)) 
		{
			time = 0;
		}

		if(GUI.Button (new Rect (120, 120, 80, 25), label, statsButton))
		{
			time_stop = !time_stop;

			if (time_stop)
				label = "Start";
			else
				label = "Stop";
		}
	}

	/// <summary>
	/// Opens windows to orient the robot
	/// </summary>
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
		rects.Add (new Rect(40, 200, 105, 35));
		rects.Add (new Rect(245, 200, 105, 35));
		rects.Add (new Rect(147, 155, 105, 35));
		rects.Add (new Rect(147, 245, 105, 35));
		rects.Add (new Rect (110, 95, 190, 35));
		rects.Add (new Rect (270, 50, 90, 35));
		rects.Add (new Rect (50, 50, 90, 35));

		oWindow = new TextWindow ("Orient Robot", new Rect ((Screen.width / 2) - 150, (Screen.height / 2) - 125, 400, 300),
		                                     new string[0], new Rect[0], titles.ToArray (), rects.ToArray ());
		//The directional buttons lift the robot to avoid collison with objects, rotates it, and saves the applied rotation to a vector3
		gui.AddWindow("Orient Robot", oWindow, (object o)=>{
			mainNode.transform.position = new Vector3(mainNode.transform.position.x, 1, mainNode.transform.position.z);
			switch((int)o)
			{
			case 0:
				mainNode.transform.position = new Vector3(mainNode.transform.position.x, 1, mainNode.transform.position.z);
				mainNode.transform.Rotate(new Vector3(0, 0, 45));
                break;
			case 1:
				mainNode.transform.position = new Vector3(mainNode.transform.position.x, 1, mainNode.transform.position.z);
				mainNode.transform.Rotate(new Vector3(0, 0, -45));
				break;
			case 2:
				mainNode.transform.position = new Vector3(mainNode.transform.position.x, 1, mainNode.transform.position.z);
				mainNode.transform.Rotate(new Vector3(45, 0, 0));
				break;
			case 3:
				mainNode.transform.position = new Vector3(mainNode.transform.position.x, 1, mainNode.transform.position.z);
				mainNode.transform.Rotate(new Vector3(-45, 0, 0));
				break;
			case 4:
				rotation = mainNode.transform.rotation;
				Debug.Log(rotation);
				break;
			case 5:
				oWindow.Active = false;
				break;
			case 6:
				rotation = Quaternion.identity;
				mainNode.transform.rotation = rotation;
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
	}

	[STAThread]
    void OnGUI()
    {
        //Custom style for windows
        menuWindow = new GUIStyle(GUI.skin.window);
        menuWindow.normal.background = transparentWindowTexture;
        menuWindow.onNormal.background = transparentWindowTexture;
        menuWindow.font = russoOne;

        //Custom style for buttons
        menuButton = new GUIStyle(GUI.skin.button);
        menuButton.font = russoOne;
        menuButton.normal.background = buttonTexture;
        menuButton.hover.background = buttonSelected;
        menuButton.active.background = buttonSelected;
        menuButton.onNormal.background = buttonSelected;
        menuButton.onHover.background = buttonSelected;
        menuButton.onActive.background = buttonSelected;

        // Draws stats window on to GUI
        if (showStatWindow)
			GUI.Window(0, statsWindowRect, StatsWindow, "Stats", statsWindow);

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

            gui.AddWindow("Quit to Main Menu", new DialogWindow("Quit to Main Menu?", "Yes", "No"), (object o) =>
            {
                if ((int)o == 0)
                {
                    Application.LoadLevel("MainMenu");
                }
            });

            gui.AddWindow ("Quit to Desktop", new DialogWindow ("Quit to Desktop?", "Yes", "No"), (object o) =>
			               {
				if ((int) o == 0) {
					Application.Quit();
				}
			});
        }
		
		if (Input.GetMouseButtonUp (0) && !gui.ClickedInsideWindow ())
		{
			HideGuiSidebar();
			gui.HideAllWindows();
		}

		if (fieldBrowser == null) {
			fieldBrowser = new FileBrowser ("Load Field", false);
			fieldBrowser.Active = false;
			fieldBrowser.OnComplete += (object obj) => 
			{
				fieldBrowser.Active = false;
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
					fieldBrowser.Active = false;
					reloadFieldInFrames = 2;
				}
				else
				{
					UserMessageManager.Dispatch("Invalid selection!", 10f);
				}
			};
		}

		if (!fieldLoaded)
			fieldBrowser.Render ();

		else 
		{
			// The Menu button on the top left corner
			GUI.Window (1, new Rect (0, 0, gui.GetSidebarWidth (), 25), 
            	(int windowID) =>
				{
					if (GUI.Button (new Rect (0, 0, gui.GetSidebarWidth (), 25), "Menu", menuButton))
						gui.EscPressed();
				},
				"",
                menuWindow
            );

			gui.Render ();
		}

		UserMessageManager.Render();

		if (reloadRobotInFrames >= 0 || reloadFieldInFrames >= 0)
        {
            GUI.backgroundColor = new Color(1, 1, 1, 0.5f);
            GUI.Box(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 25, 200, 25), "Loading... Please Wait", gui.BlackBoxStyle);
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
			unityPacket.OutputStatePacket packet = null;
            var unityWheelData = new List<GameObject>();
			var unityWheels = new List<UnityRigidNode>();
            // Invert the position of the root object
			/**/
			activeRobot.transform.localPosition = new Vector3(1f, 1f, -0.5f);
			activeRobot.transform.rotation = Quaternion.identity;
			activeRobot.transform.localRotation = Quaternion.identity;
			mainNode.transform.rotation = Quaternion.identity;
			/**/
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
			mainNode.transform.rotation = rotation;
			mainNode.rigidbody.inertiaTensorRotation = Quaternion.identity;
			
			//makes sure robot spawns in the correct place
			mainNode.transform.position = new Vector3(-2f, 1f, -3f);

        }

		foreach (GameObject o in totes)
		{
			GameObject.Destroy(o);
		}

		totes.Clear ();
    }

	/// <summary>
	/// Loads a robot from file into the simulator.
	/// </summary>
    private void TryLoadRobot()
    {
		//resets rotation for new robot
		rotation = Quaternion.identity;
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
            RigidNode_Base.NODE_FACTORY = delegate(Guid guid)
            {
                return new UnityRigidNode(guid);
            };
																																																																																																																																																																																																																																																																																																																					
            skeleton = BXDJSkeleton.ReadSkeleton(filePath + "skeleton.bxdj");
			//Debug.Log(filePath + "skeleton.bxdj");
            skeleton.ListAllNodes(names);
            foreach (RigidNode_Base node in names)
            {
                UnityRigidNode uNode = (UnityRigidNode) node;

                uNode.CreateTransform(activeRobot.transform);

                if (!uNode.CreateMesh(filePath + uNode.ModelFileName))
				{
					UserMessageManager.Dispatch(node.ModelFileName + " has been modified and cannot be loaded.", 6f);
					skeleton = null;
					UnityEngine.Object.Destroy(activeRobot);
					return;
				}

                uNode.CreateJoint();

				Debug.Log("Joint");

                meshColliders.AddRange(uNode.unityObject.GetComponentsInChildren<Collider>());
            }

            {   // Add some mass to the base object
                UnityRigidNode uNode = (UnityRigidNode) skeleton;
                uNode.unityObject.transform.rigidbody.mass += 20f * PHYSICS_MASS_MULTIPLIER; // Battery'
            }

			//finds main node of robot to use its rigidbody
			mainNode = GameObject.Find ("node_0.bxda");

			//Debug.Log ("HELLO AMIREKA: " + mainNode);
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
		activeField = new GameObject("Field");

		FieldDefinition.Factory = delegate(Guid guid, string name)
		{
			return new UnityFieldDefinition(guid, name);
		};

		Debug.Log (filePath);
		string loadResult;
		field = (UnityFieldDefinition)BXDFProperties.ReadProperties(filePath + "definition.bxdf", out loadResult);
		Debug.Log(loadResult);
		field.CreateTransform(activeField.transform);
		fieldLoaded = field.CreateMesh(filePath + "mesh.bxda");
	}

    void Start()
    {
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

        filePath = PlayerPrefs.GetString("Field");
        Debug.Log(filePath);
        reloadFieldInFrames = 2;

        reloadRobotInFrames = -1;

        //Loads textures and fonts
        buttonTexture = Resources.Load("Images/greyButton") as Texture2D;
        buttonSelected = Resources.Load("Images/selectedbuttontexture") as Texture2D;
        gravityRegular = Resources.Load("Fonts/Gravity-Regular") as Font;
        russoOne = Resources.Load("Fonts/Russo_One") as Font;
        greyWindowTexture = Resources.Load("Images/greyBackground") as Texture2D;
        darkGreyWindowTexture = Resources.Load("Images/darkGreyBackground") as Texture2D;
        lightGreyWindowTexture = Resources.Load("Images/lightGreyBackground") as Texture2D;
        transparentWindowTexture = Resources.Load("Images/transparentBackground") as Texture2D;
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
		if (mainNode == null) {
			mainNode = GameObject.Find ("node_0.bxda");
			resetRobot();
		}

		//Debug.Log(filePath);
        if (reloadRobotInFrames >= 0 && reloadRobotInFrames-- == 0)
        {
            reloadRobotInFrames = -1;
            TryLoadRobot();
        }

		if(reloadFieldInFrames >= 0 && reloadFieldInFrames-- == 0)
		{
			reloadFieldInFrames = -1;

			TryLoadField();

			if (fieldLoaded)
			{
                filePath = PlayerPrefs.GetString("Robot");
                TryLoadRobot();
				showStatWindow = true;
			}
			else
			{
                UserMessageManager.Dispatch("Incompatible Mesh!", 10f);
                Application.LoadLevel(0);
			}
		}

		// Reset Robot
		if (Input.GetKeyDown (Controls.ControlKey[(int)Controls.Control.ResetRobot]))
			gui.DoAction ("Reset Robot");

        if (Input.GetKeyDown(Controls.ControlKey[(int)Controls.Control.RobotOrient]))
            oWindow.Active = true;

		// Show/Hide physics window
		if (Input.GetKeyDown (Controls.ControlKey[(int)Controls.Control.Stats]))
			showStatWindow = !showStatWindow;
    }

    void FixedUpdate()
    {
		if (skeleton != null) 
		{
			List<RigidNode_Base> nodes = skeleton.ListAllNodes();

			unityPacket.OutputStatePacket packet = udp.GetLastPacket ();

			//stops robot while menu is open
			if(gui.guiVisible)
			{
				mainNode.rigidbody.isKinematic = true;
			}
			else
			{
				mainNode.rigidbody.isKinematic = false;
			}

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

			//calculates stats of robot
			if (mainNode != null) {
				speed = (float)Math.Abs (mainNode.rigidbody.velocity.magnitude);
				weight = (float)Math.Round (mainNode.rigidbody.mass * 2.20462 * 860, 1);
				angvelo = (float)Math.Abs (mainNode.rigidbody.angularVelocity.magnitude);
				acceleration = (float)(mainNode.rigidbody.velocity.magnitude - oldSpeed) / Time.deltaTime;
				oldSpeed = speed;
				if (!time_stop)
					time += Time.deltaTime;
			}
		}
	}
}