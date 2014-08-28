using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class Init : MonoBehaviour
{
    private FileBrowser fileBrowser = new FileBrowser();

    // We will need these
	public bool exitWindow = false;
    public const float PHYSICS_MASS_MULTIPLIER = 0.001f;

    RigidNode_Base skeleton;
    GameObject activeRobot;

    unityPacket udp = new unityPacket();
    List<GameObject> unityWheelData = new List<GameObject>();
    List<Collider> meshColliders = new List<Collider>();
    string filePath = null;//BXDSettings.Instance.LastSkeletonDirectory + "\\";

    public enum WheelPositions
    {
        FL = 1,
        FR = 2,
        BL = 3,
        BR = 4
    }
    [STAThread]
    void OnGUI()
    {
        UserMessageManager.Render();

        if (!fileBrowser.Active)
        {
            if (GUI.Button(new Rect(10, 10, 90, 30), "Load Model"))
            {
                fileBrowser.Active = true;
            }
        }
        if (fileBrowser.Active)
        {
            fileBrowser.Show();
        }
        if (fileBrowser.Submit)
        {
            fileBrowser.Active = false;
            fileBrowser.Submit = false;
            string fileLocation = fileBrowser.fileLocation;
            // If dir was selected...
            if (File.Exists(fileLocation + "\\skeleton.bxdj"))
                fileLocation += "\\skeleton.bxdj";
            DirectoryInfo parent =Directory.GetParent(fileLocation);
            if (parent != null && parent.Exists && File.Exists(parent.FullName + "\\skeleton.bxdj"))
            {
                filePath = parent.FullName + "\\";
                TryLoad();
			}
			else
			{
				UserMessageManager.Dispatch("Invalid selection!");
			}
        }

		if (exitWindow) 
		{
			Rect window = new Rect(Screen.width / 2 - 300, Screen.height / 2 - 100, 600, 200);
			window = GUI.Window(0, window, InitExitWindow, "Exit?");
		}
    }

	void InitExitWindow(int windowID) 
	{
		if (GUI.Button(new Rect(50, 50, 175, 100), "No")) 
		{
			exitWindow = false;
		}
		else if (GUI.Button(new Rect(350, 50, 175, 100), "Yes")) 
		{
			Application.Quit();
		}
	}

    void TryLoad()
    {
        if (activeRobot != null)
        {
            skeleton = null;
            UnityEngine.Object.Destroy(activeRobot);
        }

        if (filePath != null && skeleton == null)
        {
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
                if (uNode.modelFileName == "node_0.bxda")
                {
                    uNode.unityObject.transform.rigidbody.mass += 20f * PHYSICS_MASS_MULTIPLIER; // Battery'
                    Vector3 vec = uNode.unityObject.rigidbody.centerOfMass;
                    vec.y *= 0.9f;
                    uNode.unityObject.rigidbody.centerOfMass = vec;
                }
                if (uNode.IsWheel)
                {
                    unityWheelData.Add(uNode.wCollider);

                }
                //meshColliders.Add(uNode.meshCollider);                
                meshColliders.AddRange(uNode.unityObject.GetComponentsInChildren<Collider>());
            }
            if (unityWheelData.Count > 0)
            {
                auxFunctions.OrientRobot(unityWheelData, activeRobot.transform);

            }
            auxFunctions.IgnoreCollisionDetection(meshColliders);
        }
        else
        {
            Debug.Log("unityWheelData is null...");
        }

    }



    void Start()
    {
        Physics.gravity = new Vector3(0, -9.8f, 0);
        Physics.solverIterationCount = 15;
        Physics.minPenetrationForPenalty = 0.001f;

        // Load Field

        UnityRigidNode nodeThing = new UnityRigidNode();
        nodeThing.modelFileName = "field.bxda";
        nodeThing.CreateTransform(transform);
        nodeThing.CreateMesh(UnityEngine.Application.dataPath + "\\Resources\\field.bxda");
        nodeThing.unityObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

        TryLoad();
    }

    void OnEnable()
    {
        udp.Start();
    }

    void OnDisable()
    {
        udp.Stop();
    }


    void FixedUpdate()
    {
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (exitWindow) 
			{
				exitWindow = false;
			} else {
				exitWindow = true;
			}
		}

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
    }
}
