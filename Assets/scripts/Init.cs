using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class Init : MonoBehaviour
{
    public const float PHYSICS_MASS_MULTIPLIER = 0.001f;

    private GUIController gui;

    private RigidNode_Base skeleton;
    private GameObject activeRobot;

    private unityPacket udp;
	private string filePath;
	//main node of robot from which speed and other stats are derived
	private GameObject mainNode;
	//sizes and places window and repositions it based on screen size
	private Rect windowRect;

	private float acceleration;
	private float angvelo;
	private float speed;
	private float weight;
	private float time;
	private bool time_stop;
	private float oldSpeed;

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
		windowRect = new Rect (Screen.width - 320, 20, 300, 150);
		time_stop = false;
		reloadInFrames = -1;
    }

	//displays stats like speed and acceleration
	public void StatsWindow(int windowID) {
		
		GUI.Label (new Rect (10, 20, 300, 50), "Speed: " + Math.Round(speed, 1).ToString() + " m/s");
		GUI.Label (new Rect (150, 20, 300, 50),Math.Round(speed*3.28084, 1).ToString() + " ft/s");
		GUI.Label (new Rect (10, 40, 300, 50), "Acceleratiion: " + Math.Round(acceleration, 1).ToString() + " m/s^2");
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

	[STAThread]
    void OnGUI()
    {
		//draws stats window on to GUI
		windowRect = GUI.Window(0, windowRect, StatsWindow, "Stats");

        if (gui == null)
        {
            gui = new GUIController();
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
                    UserMessageManager.Dispatch("Invalid selection!");
                }
            });
            gui.AddAction("Orient Robot", () =>
            {
                OrientRobot();
            });
            if (!File.Exists(filePath + "\\skeleton.bxdj"))
            {
                gui.DoAction("Load Model");
            }

			gui.AddWindow ("Switch View", new DialogWindow("Switch View",
			    new string[] {"Driver Station [D]", "Orbit Robot [R]", "First Person [F]"}),
				(object o) =>
			    {
					GameObject cameraObject = GameObject.Find("Camera");
					Camera camera = cameraObject.GetComponent<Camera>();
					switch ((int) o) {
					case 0:
						camera.SwitchCameraState(new Camera.DriverStationState(camera));
						break;
					case 1:
						camera.SwitchCameraState(new Camera.OrbitState(camera));
						break;
					case 2:
						camera.SwitchCameraState(new Camera.FPVState(camera));
						break;
					default:
						Debug.Log("Camera state not found: " + (string) o);
						break;
					}
				});
        }
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
            activeRobot.transform.localPosition = Vector3.zero;
            activeRobot.transform.localRotation = Quaternion.identity;
            var nodes = skeleton.ListAllNodes();
            foreach (RigidNode_Base node in nodes)
            {
                UnityRigidNode uNode = (UnityRigidNode) node;
                uNode.unityObject.transform.localPosition = Vector3.zero;
                uNode.unityObject.transform.localRotation = Quaternion.identity;
                if (uNode.unityObject.rigidbody != null)
                {
                    uNode.unityObject.rigidbody.velocity = Vector3.zero;
                    uNode.unityObject.rigidbody.angularVelocity = Vector3.zero;
                }
                if (uNode.HasDriverMeta<WheelDriverMeta>() && uNode.wheelCollider != null)
                {
                    unityWheelData.Add(uNode.wheelCollider);
                }
            }
            if (unityWheelData.Count > 0)
            {
               auxFunctions.OrientRobot(unityWheelData, activeRobot.transform);
            }
        }
    }
	//loads a robot
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
        }
        else
        {
            Debug.Log("unityWheelData is null...");
        }
        gui.guiVisible = false;
		OrientRobot();
    }

    void Start()
    {
        Physics.gravity = new Vector3(0, -9.81f, 0);
        Physics.solverIterationCount = 15;
        Physics.minPenetrationForPenalty = 0.001f;

        // Load Field

        UnityRigidNode nodeThing = new UnityRigidNode();
        nodeThing.modelFileName = "field.bxda";
        nodeThing.CreateTransform(transform);
        nodeThing.CreateMesh(UnityEngine.Application.dataPath + "\\Resources\\field.bxda");
        nodeThing.unityObject.rigidbody.constraints = RigidbodyConstraints.FreezeAll;

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
    }

    void FixedUpdate()
    {
        if (skeleton != null)
        {
            unityPacket.OutputStatePacket packet = udp.GetLastPacket();
            DriveJoints.UpdateAllMotors(skeleton, packet.dio);
            DriveJoints.UpdateSolenoids(skeleton, packet.solenoid);
            List<RigidNode_Base> nodes = skeleton.ListAllNodes();
            InputStatePacket sensorPacket = new InputStatePacket();
            foreach (RigidNode_Base node in nodes)
            {
                if (node.GetSkeletalJoint() == null)
                    continue;
                foreach (RobotSensor sensor in node.GetSkeletalJoint().attachedSensors)
                {
                    if (sensor.type == RobotSensorType.POTENTIOMETER && node.GetSkeletalJoint() is RotationalJoint_Base)
                    {
                        UnityRigidNode uNode = (UnityRigidNode) node;
                        float angle = DriveJoints.GetAngleBetweenChildAndParent(uNode) + ((RotationalJoint_Base) uNode.GetSkeletalJoint()).currentAngularPosition;
                        sensorPacket.ai[sensor.module - 1].analogValues[sensor.port - 1] = (int) sensor.equation.Evaluate(angle);
                    }
                }
            }     
			udp.WritePacket(sensorPacket);
        }
			//finds main node of robot to use its rigidbody
			mainNode = GameObject.Find ("node_0.bxda");
			//calculates stats of robot
			if (mainNode != null) {
			speed = (float)Math.Abs (mainNode.rigidbody.velocity.magnitude);
			weight = (float)Math.Round (mainNode.rigidbody.mass * 2.20462 * 860, 1);
			angvelo = (float)Math.Abs (mainNode.rigidbody.angularVelocity.magnitude);
			acceleration = (float)(mainNode.rigidbody.velocity.magnitude - oldSpeed) / Time.deltaTime;
			oldSpeed = speed;
			if(!time_stop)
			time += Time.deltaTime;
		}
	}
}
