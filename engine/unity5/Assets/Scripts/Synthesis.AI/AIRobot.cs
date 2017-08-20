using UnityEngine;
using System.Collections;
using BulletUnity;
using BulletSharp;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.FEA;
using Assets.Scripts.BUExtensions;

/// <summary>
/// To be attached to all AIRobot parent objects.
/// </summary>
/// 
/// Robot class was messy, so new keyword is to be used on inherited methods until Robot class is cleaned up and
/// made scalable. Using new keyword is not optimal, but must be used because of time constraints.
/// Possible fixes:
///  -- Make Robot List in MainState a GameObject List instead of a Robot list
///  -- Make Robot class extendable to different applications instead of bottlenecking 
///         methods with player-specific controls
///  -- Make this AIRobot class an entirely different component separate from Robot entirely
///         --> This would probably lead to AI Robots not acting like players, we inherited and tried to
///         replicate code from the Robot class as much as possible to make AI Robots as humanly as possible.
public class AIRobot : Robot, IControllable
{

    private bool isInitialized;

    private RigidNode_Base rootNode;
    private List<Transform> childNodeTransforms;

    private Vector3 robotStartPosition = new Vector3(01f, 1f, 0f);
    private BulletSharp.Math.Matrix robotStartOrientation = BulletSharp.Math.Matrix.Identity;

    private UnityPacket unityPacket;

    private Vector3 nodeToRobotOffset;

    private MainState mainState;


    private RobotCameraManager robotCameraManager;

    private GameObject manipulatorObject;
    private RigidNode_Base manipulatorNode;

    // Virtual Input
    // We use virtual input here so that we can recreate the process of a user pressing a button
    // on the simulation. We want our AI Robot to act realistically in the simulation.
    private Dictionary<string, float> virtualInput;

    // Why aren't Unity Packets members of different robots? We couldn't exactly follow why
    // the Unity Packets needed to be separate from the robots, so we put ours here. If this should be
    // moved, move it.
    private UnityPacket p = new UnityPacket();

    private void Awake()
    {
        // Make sure to define all virtual inputs here
        virtualInput = new Dictionary<string, float>();
        virtualInput.Add("Vertical", 0);
        virtualInput.Add("Horizontal", 0);
        p.Start();        
    }


    /// <summary>
    /// Called once every physics step (framerate independent) to drive motor joints as well as handle the resetting of the robot
    /// </summary>
    void FixedUpdate()
    {
        Packet = p.GetLastPacket();
        if (rootNode != null && ControlsEnabled)
        {
            if (Packet != null) DriveJoints.UpdateAIMotors(rootNode, Packet.dio, MixAndMatchMode.GetMecanum(), virtualInput);
            else DriveJoints.UpdateAIMotors(rootNode, new UnityPacket.OutputStatePacket.DIOModule[2], MixAndMatchMode.GetMecanum(), virtualInput);

            // TODO AFTER CODE SPRINT:
            //      -- Add support for MixAndMatch and Manipulators
            //          --> Current support is unknown -- Manipulators may be added as extension to IControllable, and used in a behavior
            //          --> Further testing required
            /*
            int isMixAndMatch = PlayerPrefs.GetInt("MixAndMatch", 0);

            int isManipulator = PlayerPrefs.GetInt("HasManipulator", 0); //0 is false, 1 is true
            //Debug.Log("Has Manipulator: " + isManipulator);
            if (isManipulator == 1 && isMixAndMatch == 1)
            {
                Debug.Log("should be moving manipulator!");

                UnityPacket.OutputStatePacket.DIOModule[] emptyDIO = new UnityPacket.OutputStatePacket.DIOModule[2];
                emptyDIO[0] = new UnityPacket.OutputStatePacket.DIOModule();
                emptyDIO[1] = new UnityPacket.OutputStatePacket.DIOModule();

                DriveJoints.UpdateManipulatorMotors(manipulatorNode, emptyDIO, controlIndex, MixAndMatchMode.GetMecanum());

            }*/
        }
    }

    /// <summary>
    /// Initializes AI robot based off of robot directory.
    /// </summary>
    /// <param name="directory">folder directory of robot</param>
    /// <returns>Whether or not initialization was successful</returns>
    new public bool InitializeRobot(string directory, MainState source)
    {
        //Deletes all nodes if any exist, take the old node transforms out from the robot object
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);

            //If not do this the game object is destroyed but the parent-child transform relationship remains!
            child.parent = null;
            Destroy(child.gameObject);
        }

        SensorManager sensorManager = new GameObject("AI Sensor Manager").AddComponent<SensorManager>();        
        sensorManager.ResetSensorLists();

        mainState = source;
        // Set position to Spawn Point
        transform.position = SynthAIManager.Instance.SpawnPoint;

        RigidNode_Base.NODE_FACTORY = delegate (Guid guid)
        {
            return new RigidNode(guid);
        };

        List<RigidNode_Base> nodes = new List<RigidNode_Base>();
        //Read .robot instead. Maybe need a RobotSkeleton class
        rootNode = BXDJSkeleton.ReadSkeleton(directory + "\\skeleton.bxdj");
        rootNode.ListAllNodes(nodes);

        int numWheels = nodes.Count(x => x.HasDriverMeta<WheelDriverMeta>() && x.GetDriverMeta<WheelDriverMeta>().type != WheelType.NOT_A_WHEEL);
        float collectiveMass = 0f;

        foreach (RigidNode_Base n in nodes)
        {
            RigidNode node = (RigidNode)n;
            node.CreateTransform(transform);

            if (!node.CreateMesh(directory + "\\" + node.ModelFileName))
            {
                Debug.Log("AI Robot not loaded!");
                return false;
            }

            node.CreateJoint(numWheels);

            if (node.PhysicalProperties != null)
                collectiveMass += node.PhysicalProperties.mass;

            if (node.MainObject.GetComponent<BRigidBody>() != null)
                node.MainObject.AddComponent<Tracker>().Trace = true;
        }

        foreach (BRaycastRobot r in FindObjectsOfType<BRaycastRobot>())
            r.RaycastRobot.EffectiveMass = collectiveMass;

        RobotName = new DirectoryInfo(directory).Name;

        isInitialized = true;

        BRigidBody rigidBody = GetComponentInChildren<BRigidBody>();

        if (!rigidBody.GetCollisionObject().IsActive)
            rigidBody.GetCollisionObject().Activate();

        // Exclude the SensorManager
        childNodeTransforms = new List<Transform>();
        foreach(Transform t in this.transform)
        {
            if (!t.Equals(sensorManager.transform) && !t.GetComponent<UnityEngine.AI.NavMeshAgent>())
            {
                childNodeTransforms.Add(t);
            }
        }
        sensorManager.transform.parent = this.transform;
        //sensorManager.AddBeamBreaker(transform.GetChild(0).gameObject, new Vector3(0, 0, 1), new Vector3(0, 90, 0), 1);
        //sensorManager.AddUltrasonicSensor(transform.GetChild(0).gameObject, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
        //sensorManager.AddGyro(transform.GetChild(0).gameObject, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
        return true;
    }

    /// <summary>
    /// Destroys this AI Robot
    /// </summary>
    new public void BeginReset()
    {
        BaseSynthBehaviour b = this.gameObject.GetComponent<BaseSynthBehaviour>();
        if (b != null) {
            b.enabled = false; // Disable behaviour so that it does not attempt to run next frame
                               // (Destroyed game objects are not destroyed until next frame).
                               // This prevents error on NavMeshAgent.
        }
        Destroy(this.gameObject);
    }

    new public bool LoadManipulator(string directory)
    {
        manipulatorObject = new GameObject("Manipulator");

        //Set the manipulator transform to match with the position of node_0 of the robot. THIS ONE ACTUALLY DOES SOMETHING:
        manipulatorObject.transform.position = this.transform.GetChild(0).transform.position;
        //manipulatorObject.transform.position = robotStartPosition;

        RigidNode_Base.NODE_FACTORY = delegate (Guid guid)
        {
            return new RigidNode(guid);
        };

        List<RigidNode_Base> nodes = new List<RigidNode_Base>();
        //TO-DO: Read .robot instead (from the new exporters if they are implemented). Maybe need a RobotSkeleton class
        manipulatorNode = BXDJSkeleton.ReadSkeleton(directory + "\\skeleton.bxdj");
        manipulatorNode.ListAllNodes(nodes);

        int numWheels = nodes.Count(x => x.HasDriverMeta<WheelDriverMeta>() && x.GetDriverMeta<WheelDriverMeta>().type != WheelType.NOT_A_WHEEL);
        float collectiveMass = 0f;

        //Load node_0 for attaching manipulator to robot
        RigidNode node = (RigidNode)nodes[0];
        node.CreateTransform(manipulatorObject.transform);
        if (!node.CreateMesh(directory + "\\" + node.ModelFileName))
        {
            Debug.Log("Robot not loaded!");
            UnityEngine.Object.Destroy(manipulatorObject);
            return false;
        }
        node.CreateManipulatorJoint();
        node.MainObject.AddComponent<Tracker>().Trace = true;
        Tracker t = node.MainObject.GetComponent<Tracker>();
        Debug.Log(t);

        //Load other nodes associated with the manipulator
        for (int i = 1; i < nodes.Count; i++)
        {
            RigidNode otherNode = (RigidNode)nodes[i];
            otherNode.CreateTransform(manipulatorObject.transform);
            if (!otherNode.CreateMesh(directory + "\\" + otherNode.ModelFileName))
            {
                Debug.Log("Robot not loaded!");
                UnityEngine.Object.Destroy(manipulatorObject);
                return false;
            }
            otherNode.CreateJoint(numWheels);
            otherNode.MainObject.AddComponent<Tracker>().Trace = true;
            t = otherNode.MainObject.GetComponent<Tracker>();
            Debug.Log(t);
        }

        foreach (BRaycastRobot r in GetComponentsInChildren<BRaycastRobot>())
            r.RaycastRobot.EffectiveMass = collectiveMass;

        RotateRobot(robotStartOrientation);
        return true;
    }

    /// <summary>
    /// Retrieves forward vector of the root node. For some robots, this may be incorrectly defined.
    /// </summary>
    /// <returns>Returns the forward vector of the robot</returns>
    public Vector3 GetForward()
    {
        return transform.GetChild(0).transform.forward.normalized;
    }

    /// <summary>
    /// Moves the robot's wheels to accelerate either forwards or backwards
    /// NOTE: Robot will not automatically stop accelerating. Acceleration must be set back to 0
    /// </summary>
    /// <param name="acceleration">A number from -1 to 1, to accelerate by</param>
    public void Accelerate(float acceleration)
    {
        virtualInput["Vertical"] = Mathf.Clamp(acceleration, -1f, 1f);
    }

    /// <summary>
    /// Moves the robot's wheels to turn left or right
    /// </summary>
    /// <param name="direction">A number from -1 to 1, to turn by. -1 is Left, 1 is Right</param>
    public void Turn(float direction)
    {
        virtualInput["Horizontal"] = Mathf.Clamp(direction, -1f, 1f);
    }

    /// <summary>
    /// Calculates average center of robot based on node positions.
    /// </summary>
    /// <returns>Average center of robot</returns>
    public Vector3 GetPosition()
    {
        Vector3 position = Vector3.zero;
        foreach(Transform t in this.childNodeTransforms)
        {
            position += t.position;
        }
        position /= this.childNodeTransforms.Count;
        return position;
    }
}