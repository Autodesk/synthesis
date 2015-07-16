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

	enum FieldType
	{
		None,
		FRC_2014,
		FRC_2015
	};

	FieldType currentFieldType;

    private GUIController gui;

	private PhysicMaterial chuteMaterial;

    private RigidNode_Base skeleton;
    private GameObject activeRobot;
	private GameObject cameraObject;
	private DynamicCamera dynamicCamera;
	private GameObject light;
	private Field field;
	private List<GameObject> totes;

    private unityPacket udp;
	private string filePath;
	//main node of robot from which speed and other stats are derived
	private GameObject mainNode;
	//sizes and places window and repositions it based on screen size
	private Rect statsWindowRect;

	// Hotkeys window constants
	private Rect hotkeysWindowRect;
	private int hotkeysWindowWidth = 400;
	private int hotkeysWindowHeight = 200;

	private float acceleration;
	private float angvelo;
	private float speed;
	private float weight;
	private float time;
	private bool time_stop;
	private float oldSpeed;
	private bool showStatWindow;
	Quaternion rotation;

    /// <summary>
    /// Frames before the robot gets reloaded, or -1 if no reload is queued.
    /// </summary>
    /// <remarks>
    /// This allows reloading the robot to be delayed until a "Loading" dialog can be drawn.
    /// </remarks>
    private volatile int reloadInFrames;

    public Init()
    {
		udp = new unityPacket ();
		filePath = BXDSettings.Instance.LastSkeletonDirectory + "\\";
		statsWindowRect = new Rect (Screen.width - 320, 20, 300, 150);
		hotkeysWindowRect = new Rect(
			(Screen.width / 2) - (hotkeysWindowWidth / 2), 
     		(Screen.height / 2) - (hotkeysWindowHeight / 2), 
         	hotkeysWindowWidth, 
         	hotkeysWindowHeight
		);
		time_stop = false;
		reloadInFrames = -1;
		showStatWindow = true;
		StreamReader orientation = new StreamReader ("orientation.txt");
		rotation = new Quaternion (float.Parse (orientation.ReadLine ()), float.Parse (orientation.ReadLine ()), float.Parse (orientation.ReadLine ()), float.Parse (orientation.ReadLine ()));
		orientation.Close ();
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

		TextWindow oWindow = new TextWindow ("Orient Robot", new Rect ((Screen.width / 2) - 150, (Screen.height / 2) - 75, 300, 250),
		                                    new string[0], new Rect[0], titles.ToArray (), rects.ToArray ());

		gui.AddWindow("Orient Robot", oWindow, (object o)=>{
			switch((int)o)
			{
			case 0:
				activeRobot.transform.Rotate(new Vector3(activeRobot.transform.localRotation.x, activeRobot.transform.localRotation.y,activeRobot.transform.localRotation.z + 90));
                break;
			case 1:	
				activeRobot.transform.Rotate (new Vector3(activeRobot.transform.localRotation.x, activeRobot.transform.localRotation.y,activeRobot.transform.localRotation.z - 90));
				break;
			case 2:
				activeRobot.transform.Rotate(new Vector3(activeRobot.transform.localRotation.x + 90, activeRobot.transform.localRotation.y,activeRobot.transform.localRotation.z));
				break;
			case 3:
				activeRobot.transform.Rotate(new Vector3(activeRobot.transform.localRotation.x - 90, activeRobot.transform.localRotation.y,activeRobot.transform.localRotation.z));
				break;
			case 4:
			{
				rotation = activeRobot.transform.rotation;
				StreamWriter orientation = new StreamWriter("orientation.txt");
				orientation.WriteLine(rotation.x);
				orientation.WriteLine(rotation.y);
				orientation.WriteLine(rotation.z);
				orientation.WriteLine(rotation.w);
				orientation.Close ();

				Debug.Log (rotation);
				break;
			}
			case 5:
				oWindow.Active = false;
				break;
			case 6:
				rotation = Quaternion.identity;
				activeRobot.transform.rotation = rotation;
				break;

			}			
		});
	}

	public void HotkeysWindow()
	{
		int leftX = 75;
		int leftXOffset = 275;
		int heightGap = 25;
		
		List<string> labelTitles = new List<string>();
		labelTitles.Add ("Reset Robot:"); 
		labelTitles.Add ("Driverstation:");
		labelTitles.Add ("Orbit Robot:O");
		labelTitles.Add ("First Person:F"); 
		labelTitles.Add ("Stats window toggle:");
		labelTitles.Add ("Menu:");
		labelTitles.Add ("[R]"); 
		labelTitles.Add ("[D]");
		labelTitles.Add ("[O]");
		labelTitles.Add ("[F]"); 
		labelTitles.Add ("[H]");
		labelTitles.Add ("[Esc]");

		List<Rect> labelRects = new List<Rect>();
		labelRects.Add (new Rect (leftX, 1 * heightGap, 300, 50));
		labelRects.Add (new Rect (leftX, 2 * heightGap, 300, 50));
		labelRects.Add (new Rect (leftX, 3 * heightGap, 300, 50));
		labelRects.Add (new Rect (leftX, 4 * heightGap, 300, 50)); 
		labelRects.Add (new Rect (leftX, 5 * heightGap, 300, 50));
		labelRects.Add (new Rect (leftX, 6 * heightGap, 300, 50));
		labelRects.Add (new Rect (leftXOffset, 1 * heightGap, 300, 50)); 
		labelRects.Add (new Rect (leftXOffset, 2 * heightGap, 300, 50));
		labelRects.Add (new Rect (leftXOffset, 3 * heightGap, 300, 50));
		labelRects.Add (new Rect (leftXOffset, 4 * heightGap, 300, 50)); 
		labelRects.Add (new Rect (leftXOffset, 5 * heightGap, 300, 50));
		labelRects.Add (new Rect (leftXOffset, 6 * heightGap, 300, 50));

		string windowTitle = "Hotkeys";
		Rect windowRect = hotkeysWindowRect;
		gui.AddWindow (windowTitle, new TextWindow (windowTitle, windowRect, labelTitles.ToArray(), labelRects.ToArray(), new string[0], new Rect[0]), (object o)=>{});
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
		// Draws stats window on to GUI
		if(showStatWindow)
			statsWindowRect = GUI.Window(0, statsWindowRect, StatsWindow, "Stats");

        if (gui == null)
        {
            gui = new GUIController();
			gui.hideGuiCallback = HideGuiSidebar;
			gui.showGuiCallback = ShowGuiSidebar;

            gui.AddWindow("Load Model", new FileBrowser(), (object o) =>
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
                    reloadInFrames = 2;
                }
                else
                {
                    UserMessageManager.Dispatch("Invalid selection!", 10f);
                }
			});

            gui.AddAction("Reset Robot", () =>
            {
				Debug.Log (rotation + " reset");
                OrientRobot();
            });
			//button to manually orient the robot
			ShowOrient();

            if (!File.Exists(filePath + "\\skeleton.bxdj"))
            {
                gui.DoAction("Load Model");
            }

			gui.AddWindow ("Switch View", new DialogWindow("Switch View",
			    "Driver Station [D]", "Orbit Robot [R]", "First Person [F]"), (object o) =>
			    {
					gui.guiVisible = false;

					switch ((int) o) {
					case 0:
						dynamicCamera.SwitchCameraState(new DynamicCamera.DriverStationState(dynamicCamera));
						break;
					case 1:
						dynamicCamera.SwitchCameraState(new DynamicCamera.OrbitState(dynamicCamera));
						break;
					case 2:
						dynamicCamera.SwitchCameraState(new DynamicCamera.FPVState(dynamicCamera));
						break;
					default:
						Debug.Log("Camera state not found: " + (string) o);
						break;
					}
				});

			gui.AddWindow ("Switch Field", new DialogWindow("Switch Field",
				"Aerial Asssist (2014)", "Recycle Rush (2015)"), (object o) =>
			    {
					gui.guiVisible = false;

					switch ((int) o)
					{
					case 0:
						SetField(FieldType.FRC_2014);
						break;
					case 1:
						SetField(FieldType.FRC_2015);
						break;
					}
				});

			HotkeysWindow();

			gui.AddWindow ("Exit", new DialogWindow ("Exit?", "Yes", "No"), (object o) =>
			               {
				if ((int) o == 0) {
					Application.Quit();
				}
			});
        }

		// The Menu bottom on the bottom left corner
		GUI.Window (1, new Rect (0, Screen.height - 25, 100, 25), 
        	(int windowID) =>
        	{
				if (GUI.Button (new Rect (0, 0, 100, 25), "Menu"))
					gui.EscPressed();
			},
			""
		);

        gui.Render();

        if (reloadInFrames >= 0)
        {
            GUI.backgroundColor = new Color(1, 1, 1, 0.5f);
            GUI.Box(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 25, 200, 50), "Loading... Please Wait", gui.BlackBoxStyle);
        }
    }

    /// <summary>
    /// Repositions the robot so it is aligned at the center of the field, and resets all the
    /// joints, velocities, etc..
    /// </summary>
    private void OrientRobot()
    {	
        if (activeRobot != null && skeleton != null)
        {
            var unityWheelData = new List<GameObject>();
            // Invert the position of the root object
            activeRobot.transform.localPosition = new Vector3(2.5f, 1f, -2.25f);
            activeRobot.transform.localRotation = Quaternion.identity;
            var nodes = skeleton.ListAllNodes();
            foreach (RigidNode_Base node in nodes)
            {
                UnityRigidNode uNode = (UnityRigidNode) node;
				uNode.unityObject.transform.localPosition = Vector3.zero;
                uNode.unityObject.transform.localRotation = rotation;
                if (uNode.unityObject.rigidbody != null)
                {
                    uNode.unityObject.rigidbody.velocity = Vector3.zero;
                    uNode.unityObject.rigidbody.angularVelocity = Vector3.zero;
                }
                if (uNode.HasDriverMeta<WheelDriverMeta>() && uNode.GetDriverMeta<WheelDriverMeta>().isDriveWheel && uNode.wheelCollider != null)
                {
                    unityWheelData.Add(uNode.wheelCollider);
                }
            }
            if (unityWheelData.Count > 0)
            {
               auxFunctions.OrientRobot(unityWheelData, activeRobot.transform);
            }
        }

		dynamicCamera.SwitchCameraState (new DynamicCamera.DriverStationState(dynamicCamera));

		foreach (GameObject o in totes)
		{
			GameObject.Destroy(o);
		}

		totes.Clear ();
    }

    private void TryLoad()
    {
        if (activeRobot != null)
        {
            skeleton = null;
            UnityEngine.Object.Destroy(activeRobot);
        }
        if (filePath != null && skeleton == null)
        {
            List<Collider> meshColliders = new List<Collider>();
            activeRobot = new GameObject("Robot");
            activeRobot.transform.parent = transform;

            List<RigidNode_Base> names = new List<RigidNode_Base>();
            RigidNode_Base.NODE_FACTORY = delegate()
            {
                return new UnityRigidNode();
            };

            skeleton = BXDJSkeleton.ReadSkeleton(filePath + "skeleton.bxdj");
			Debug.Log(filePath + "skeleton.bxdj");
            skeleton.ListAllNodes(names);
            foreach (RigidNode_Base node in names)
            {
                UnityRigidNode uNode = (UnityRigidNode) node;

                uNode.CreateTransform(activeRobot.transform);
                uNode.CreateMesh(filePath + uNode.modelFileName);
                uNode.CreateJoint();

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
            OrientRobot();
        }
        else
        {
            Debug.Log("unityWheelData is null...");
        }
        gui.guiVisible = false;
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

		SetField (FieldType.FRC_2015);

        reloadInFrames = 2;
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
        if (reloadInFrames >= 0 && reloadInFrames-- == 0)
        {
            reloadInFrames = -1;
            TryLoad();
        }

		// Only allow camera moving if gui is not showing
		if (gui != null && !gui.guiVisible) 
		{
			if (Input.GetKeyDown (KeyCode.Z)) {
				totes.Add (Tote.Create (new Vector3 (-3.619f, 0.742f, -8.183f), new Vector3 (0f, 323.3176f, 247.9989f), new Vector3 (FORMAT_3DS_SCALE, FORMAT_3DS_SCALE, FORMAT_3DS_SCALE)));
			}
			if (Input.GetKeyDown (KeyCode.X)) {
				totes.Add (Tote.Create (new Vector3 (3.619f, 0.742f, -8.183f), new Vector3 (0f, 216.2776f, 247.9989f), new Vector3 (FORMAT_3DS_SCALE, FORMAT_3DS_SCALE, FORMAT_3DS_SCALE)));
			}
			if (Input.GetKeyDown (KeyCode.C)) {
				totes.Add (Tote.Create (new Vector3 (-3.619f, 0.742f, 8.183f), new Vector3 (0f, 36.2776f, 247.9989f), new Vector3 (FORMAT_3DS_SCALE, FORMAT_3DS_SCALE, FORMAT_3DS_SCALE)));
			}
			if (Input.GetKeyDown (KeyCode.V)) {
				totes.Add (Tote.Create (new Vector3 (3.619f, 0.742f, 8.183f), new Vector3 (0f, 143.3176f, 247.9989f), new Vector3 (FORMAT_3DS_SCALE, FORMAT_3DS_SCALE, FORMAT_3DS_SCALE)));
			}
		}

		// Orient Robot
		if (Input.GetKeyDown (KeyCode.R))
			gui.DoAction ("Reset Robot");

		// Show/Hide physics window
		if (Input.GetKeyDown (KeyCode.H))
			showStatWindow = !showStatWindow;
    }

    void FixedUpdate()
    {
		if (skeleton != null) {
			unityPacket.OutputStatePacket packet = udp.GetLastPacket ();
			DriveJoints.UpdateAllMotors (skeleton, packet.dio);
			DriveJoints.UpdateSolenoids (skeleton, packet.solenoid);
			List<RigidNode_Base> nodes = skeleton.ListAllNodes ();
			InputStatePacket sensorPacket = new InputStatePacket ();
			foreach (RigidNode_Base node in nodes) {
				if (node.GetSkeletalJoint () == null)
					continue;

				foreach (RobotSensor sensor in node.GetSkeletalJoint().attachedSensors) {
					if (sensor.type == RobotSensorType.POTENTIOMETER && node.GetSkeletalJoint () is RotationalJoint_Base) {
						UnityRigidNode uNode = (UnityRigidNode)node;
						float angle = DriveJoints.GetAngleBetweenChildAndParent (uNode) + ((RotationalJoint_Base)uNode.GetSkeletalJoint ()).currentAngularPosition;
						sensorPacket.ai [sensor.module - 1].analogValues [sensor.port - 1] = (int)sensor.equation.Evaluate (angle);
					}
				}
			}
			udp.WritePacket (sensorPacket);
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

	void SetField(FieldType type)
	{
		if (!currentFieldType.Equals (type))
		{
			currentFieldType = type;

			switch (type)
			{
			case FieldType.FRC_2014:
				if (field != null) field.Destroy();

				UnityRigidNode nodeThing = new UnityRigidNode ();

				nodeThing.modelFileName = "field.bxda";
				nodeThing.CreateTransform (transform);
				nodeThing.CreateMesh (UnityEngine.Application.dataPath + "\\Resources\\field.bxda");
				nodeThing.unityObject.GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezeAll;
				break;
			case FieldType.FRC_2015:
				GameObject.Destroy(GameObject.Find("field.bxda"));

				field = new Field ("field2015", new Vector3(0f, 0.58861f, 0f), new Vector3(FORMAT_3DS_SCALE, FORMAT_3DS_SCALE, FORMAT_3DS_SCALE));

				field.AddCollisionObjects (
					"GE-15025_0", "GE-15025_1", "GE-15025_2", "GE-15025_A",
					"GE-15003_0", "GE-15002_2", "GE-15017_0", "GE-15009_0",
					"GE-15017_0", "GE-15017_3", "GE-15018_0", "GE-15018_3",
					"GE-15002_1", "GE-15003_1", "GE-15009_1", "GE-15017_3",
					"GE-15002_0", "GE-15003_2", "GE-15018_0", "GE-15009_2",
					"GE-15003_3", "GE-15002_3", "GE-15018_3", "GE-15009_3"
					);
				
				BoxCollider floor = field.AddComponent<BoxCollider> ("floor");
				floor.center = new Vector3 (0f, -6.5f, 0f);
				floor.size = new Vector3 (36.00475f, 8.343518f, 86.43035f);
				
				BoxCollider blueDS = field.AddComponent<BoxCollider> ("blueDS");
				blueDS.center = new Vector3 (0f, 1.529588f, 33.50338f);
				blueDS.size = new Vector3 (21.74336f, 7.846722f, 2.357369f);
				
				BoxCollider redDS = field.AddComponent<BoxCollider> ("redDS");
				redDS.center = new Vector3 (0f, 1.529588f, -33.50338f);
				redDS.size = new Vector3 (21.74336f, 7.846722f, 2.357369f);
				
				BoxCollider step = field.AddComponent<BoxCollider> ("step");
				step.center = new Vector3 (0f, -2.022223f, 0f);
				step.size = new Vector3 (32.41857f, 0.7251982f, 2.498229f);
				
				BoxCollider leftSidePanels = field.AddComponent<BoxCollider> ("leftSidePanels");
				leftSidePanels.center = new Vector3 (-17.08714f, -1.359237f, 0f);
				leftSidePanels.size = new Vector3 (1.80665f, 2.039491f, 50.91413f);
				
				BoxCollider rightSidePanels = field.AddComponent<BoxCollider> ("rightSidePanels");
				rightSidePanels.center = new Vector3 (17.08714f, -1.359237f, 0f);
				rightSidePanels.size = new Vector3 (1.80665f, 2.039491f, 50.91413f);
				
				field.getCollisionObjects ("GE-15017_0").GetComponent<MeshCollider>().material = chuteMaterial;
				field.getCollisionObjects ("GE-15009_0").GetComponent<MeshCollider>().material = chuteMaterial;
				field.getCollisionObjects ("GE-15017_3").GetComponent<MeshCollider>().material = chuteMaterial;
				field.getCollisionObjects ("GE-15009_1").GetComponent<MeshCollider>().material = chuteMaterial;
				field.getCollisionObjects ("GE-15018_0").GetComponent<MeshCollider> ().material = chuteMaterial;
				field.getCollisionObjects ("GE-15009_2").GetComponent<MeshCollider> ().material = chuteMaterial;
				field.getCollisionObjects ("GE-15018_3").GetComponent<MeshCollider> ().material = chuteMaterial;
				field.getCollisionObjects ("GE-15009_3").GetComponent<MeshCollider> ().material = chuteMaterial;

				break;
			}
		}
	}
}