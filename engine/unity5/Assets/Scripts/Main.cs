using UnityEngine;
using System.Collections;
using BulletUnity;
using BulletSharp;
using System;
using System.Collections.Generic;
using BulletSharp.SoftBody;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
{
    const float RESET_VELOCITY = 0.05f;

    private UnityPacket unityPacket;

    private DynamicCamera dynamicCamera;

    private GameObject fieldObject;
    private UnityFieldDefinition fieldDefinition;

    private GameObject robotObject;
    private RigidNode_Base rootNode;
    
    private Vector3 robotStartPosition = new Vector3(0f, 1f, 0f);//new Vector3(-0.5f, 1f, -3f);

    List<GameObject> extraElements;

    private System.Random random;

    bool resetting;

    void Awake()
    {
        GImpactCollisionAlgorithm.RegisterAlgorithm((CollisionDispatcher)BPhysicsWorld.Get().world.Dispatcher);
        BPhysicsWorld.Get().DebugDrawMode = DebugDrawModes.DrawWireframe | DebugDrawModes.DrawConstraints | DebugDrawModes.DrawConstraintLimits;
        BPhysicsWorld.Get().DoDebugDraw = true;
    }

    // Use this for initialization
    void Start()
    {
        unityPacket = new UnityPacket();
        unityPacket.Start();

        Debug.Log(LoadField(PlayerPrefs.GetString("Field")) ? "Load field success!" : "Load field failed.");
        Debug.Log(LoadRobot(PlayerPrefs.GetString("Robot")) ? "Load robot success!" : "Load robot failed.");
        
        dynamicCamera = GameObject.Find("Main Camera").AddComponent<DynamicCamera>();

        extraElements = new List<GameObject>();
        
        random = new System.Random();

        resetting = false;
    }
	
	// Update is called once per frame
	void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            Vector3 spawnPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 5f);
            GameObject newObject = (GameObject)Instantiate(GameObject.Find("GE-16180_1279"), Camera.main.ScreenToWorldPoint(spawnPoint), Quaternion.identity);
            newObject.AddComponent<Rainbow>();
            extraElements.Add(newObject);
        }
	}

    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.M))
            SceneManager.LoadScene("MainMenu");

        if (rootNode != null)
        {
            UnityPacket.OutputStatePacket packet = unityPacket.GetLastPacket();

            DriveJoints.UpdateAllMotors(rootNode, packet.dio);
        }

        BRigidBody rigidBody = robotObject.GetComponentInChildren<BRigidBody>();

        if (Input.GetKey(KeyCode.R))
        {
            if (!resetting)
            {
                foreach (GameObject g in extraElements)
                    Destroy(g);

                BeginReset();

                resetting = true;
            }

            Vector3 transposition = new Vector3(
                Input.GetKey(KeyCode.RightArrow) ? RESET_VELOCITY : Input.GetKey(KeyCode.LeftArrow) ? -RESET_VELOCITY : 0f,
                0f,
                Input.GetKey(KeyCode.UpArrow) ? RESET_VELOCITY : Input.GetKey(KeyCode.DownArrow) ? -RESET_VELOCITY : 0f);

            if (!transposition.Equals(Vector3.zero))
                TransposeRobot(transposition);
        }
        else
        {
            EndReset();

            resetting = false;
        }

        if (!rigidBody.GetCollisionObject().IsActive)
            rigidBody.GetCollisionObject().Activate();
    }

    bool LoadField(string directory)
    {
        fieldObject = new GameObject("Field");

        FieldDefinition.Factory = delegate (Guid guid, string name)
        {
            return new UnityFieldDefinition(guid, name);
        };

        string loadResult;
        fieldDefinition = (UnityFieldDefinition)BXDFProperties.ReadProperties(directory + "\\definition.bxdf", out loadResult);
        Debug.Log(loadResult);
        fieldDefinition.CreateTransform(fieldObject.transform);
        return fieldDefinition.CreateMesh(directory + "\\mesh.bxda");
    }

    bool LoadRobot(string directory)
    {
        robotObject = new GameObject("Robot");
        robotObject.transform.position = robotStartPosition;

        RigidNode_Base.NODE_FACTORY = delegate (Guid guid)
        {
            return new RigidNode(guid);
        };

        List<RigidNode_Base> nodes = new List<RigidNode_Base>();
        rootNode = BXDJSkeleton.ReadSkeleton(directory + "\\skeleton.bxdj");
        rootNode.ListAllNodes(nodes);

        foreach (RigidNode_Base n in nodes)
        {
            RigidNode node = (RigidNode)n;
            node.CreateTransform(robotObject.transform);
            
            if (!node.CreateMesh(directory + "\\" + node.ModelFileName))
            {
                Debug.Log("Robot not loaded!");
                Destroy(robotObject);
                return false;
            }

            node.CreateJoint();
        }

        return true;
    }

    void BeginReset()
    {
        foreach (RigidNode n in rootNode.ListAllNodes())
        {
            RigidBody r = (RigidBody)n.MainObject.GetComponent<BRigidBody>().GetCollisionObject();
            r.LinearVelocity = r.AngularVelocity = BulletSharp.Math.Vector3.Zero;
            r.LinearFactor = r.AngularFactor = BulletSharp.Math.Vector3.Zero;
            
            BulletSharp.Math.Matrix newTransform = r.WorldTransform;
            newTransform.Origin = (robotStartPosition + n.ComOffset).ToBullet();
            newTransform.Basis = BulletSharp.Math.Matrix.Identity;
            r.WorldTransform = newTransform;
        }
    }

    void EndReset()
    {
        foreach (RigidNode n in rootNode.ListAllNodes())
        {
            RigidBody r = (RigidBody)n.MainObject.GetComponent<BRigidBody>().GetCollisionObject();
            r.LinearFactor = r.AngularFactor = BulletSharp.Math.Vector3.One;
        }
    }

    void TransposeRobot(Vector3 transposition)
    {
        foreach (RigidNode n in rootNode.ListAllNodes())
        {
            RigidBody r = (RigidBody)n.MainObject.GetComponent<BRigidBody>().GetCollisionObject();

            BulletSharp.Math.Matrix newTransform = r.WorldTransform;
            newTransform.Origin += transposition.ToBullet();
            r.WorldTransform = newTransform;
        }
    }
}
