using UnityEngine;
using System.Collections;
using BulletUnity;
using BulletSharp;
using System;
using System.Collections.Generic;

public class Main : MonoBehaviour
{
    private const bool ROBOT_BRAKING_ENABLED = true;
    private const float ROBOT_TOP_SPEED = 150f;
    private const float ROBOT_TURNING_SCALE = 0.25f;

    private UnityPacket unityPacket;

    private DynamicCamera dynamicCamera;

    private GameObject fieldObject;
    private UnityFieldDefinition fieldDefinition;

    private GameObject robotObject;
    private RigidNode_Base rootNode;

    List<GameObject> extraSpheres;

    private System.Random random;

    // Use this for initialization
    void Start ()
    {
        unityPacket = new UnityPacket();
        unityPacket.Start();

        BPhysicsWorld world = gameObject.AddComponent<BPhysicsWorld>();
        world.maxSubsteps = 1000;
        world.DebugDrawMode = DebugDrawModes.DrawConstraintLimits | DebugDrawModes.DrawConstraints | DebugDrawModes.DrawWireframe;
        world.DoDebugDraw = true;

        Debug.Log(LoadField(PlayerPrefs.GetString("Field")) ? "Load field success!" : "Load field failed.");
        Debug.Log(LoadRobot(PlayerPrefs.GetString("Robot")) ? "Load robot success!" : "Load robot failed.");

        dynamicCamera = GameObject.Find("Main Camera").AddComponent<DynamicCamera>();

        extraSpheres = new List<GameObject>();

        random = new System.Random();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            Vector3 spawnPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 5f);
            GameObject newObject = (GameObject)Instantiate(GameObject.Find("GE-16180_1279"), Camera.main.ScreenToWorldPoint(spawnPoint), Quaternion.identity);
            newObject.AddComponent<Rainbow>();
            extraSpheres.Add(newObject);
        }
	}

    void FixedUpdate()
    {
        if (rootNode != null)
        {
            UnityPacket.OutputStatePacket packet = unityPacket.GetLastPacket();

            DriveJoints.UpdateAllMotors(rootNode, packet.dio);
            
        }

        BRigidBody rigidBody = robotObject.GetComponentInChildren<BRigidBody>();

        if (Input.GetKey(KeyCode.R))
        {
            foreach (GameObject g in extraSpheres)
                Destroy(g);

            ResetRobot();
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
        robotObject.transform.position = new Vector3(0f, 1f, 0f);

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

    void ResetRobot()
    {
        foreach (BRigidBody rb in robotObject.GetComponentsInChildren<BRigidBody>())
        {
            RigidBody r = (RigidBody)rb.GetCollisionObject();
            r.LinearVelocity = r.AngularVelocity = BulletSharp.Math.Vector3.Zero;

            BulletSharp.Math.Matrix newTransform = r.WorldTransform;
            newTransform.Origin = (rb.transform.GetChild(0).GetComponent<MeshFilter>().mesh.bounds.center + new Vector3(0f, 1f, 0f)).ToBullet();
            newTransform.Basis = BulletSharp.Math.Matrix.Identity;
            r.WorldTransform = newTransform;
        }
    }
}
