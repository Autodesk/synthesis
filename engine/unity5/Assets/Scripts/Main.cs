using UnityEngine;
using System.Collections;
using BulletUnity;
using BulletSharp;
using System;
using System.Collections.Generic;

public class Main : MonoBehaviour
{
    public const float COLLISION_MARGIN = 0.025f;

    private DynamicCamera dynamicCamera;

    private GameObject fieldObject;
    private UnityFieldDefinition fieldDefinition;

    private GameObject robotObject;
    private RigidNode_Base rootNode;

    private System.Random random;

    // Use this for initialization
    void Start ()
    {
        BPhysicsWorld world = gameObject.AddComponent<BPhysicsWorld>();
        world.maxSubsteps = 1000;
        world.DebugDrawMode = DebugDrawModes.DrawConstraintLimits | DebugDrawModes.DrawConstraints;
        world.DoDebugDraw = true;

        Debug.Log(LoadField() ? "Load field success!" : "Load field failed.");
        Debug.Log(LoadRobot() ? "Load robot success!" : "Load robot failed.");

        dynamicCamera = GameObject.Find("Main Camera").AddComponent<DynamicCamera>();

        random = new System.Random();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            Vector3 spawnPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 5f);
            GameObject newObject = (GameObject)Instantiate(GameObject.Find("YELLOW_TOTE:1"), Camera.main.ScreenToWorldPoint(spawnPoint), Quaternion.identity);
            Material newMaterial = new Material(newObject.GetComponentInChildren<MeshRenderer>().sharedMaterial);
            newMaterial.color = new Color((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
            newObject.GetComponentInChildren<MeshRenderer>().sharedMaterial = newMaterial;
        }
	}

    void FixedUpdate()
    {
        BRigidBody rigidBody = robotObject.GetComponentInChildren<BRigidBody>();
        GameObject g = rigidBody.gameObject;

        rigidBody.AddImpulse(new Vector3(
            Input.GetKey(KeyCode.RightArrow) ? 1f : Input.GetKey(KeyCode.LeftArrow) ? -1f : 0f,
            Input.GetKey(KeyCode.W) ? 5f : 0f,
            Input.GetKey(KeyCode.UpArrow) ? 1f : Input.GetKey(KeyCode.DownArrow) ? -1f : 0f));

        if (!rigidBody.GetCollisionObject().IsActive)
            rigidBody.GetCollisionObject().Activate();
    }

    bool LoadField()
    {
        fieldObject = new GameObject("Field");

        FieldDefinition.Factory = delegate (Guid guid, string name)
        {
            return new UnityFieldDefinition(guid, name);
        };

        string loadResult;
        fieldDefinition = (UnityFieldDefinition)BXDFProperties.ReadProperties("C:\\BXDFTest\\definition.bxdf", out loadResult);
        Debug.Log(loadResult);
        fieldDefinition.CreateTransform(fieldObject.transform);
        return fieldDefinition.CreateMesh("C:\\BXDFTest\\mesh.bxda");
    }

    bool LoadRobot()
    {
        robotObject = new GameObject("Robot");
        robotObject.transform.position = new Vector3(0f, 2f, 0f);

        RigidNode_Base.NODE_FACTORY = delegate (Guid guid)
        {
            return new RigidNode(guid);
        };

        List<RigidNode_Base> nodes = new List<RigidNode_Base>();
        rootNode = BXDJSkeleton.ReadSkeleton("C:\\BXDJTest\\skeleton.bxdj");
        rootNode.ListAllNodes(nodes);

        //for (int i = 0; i < 2; i++)
        //{
        //    RigidNode node = (RigidNode)nodes[i];
        foreach (RigidNode_Base n in nodes)
        {
            RigidNode node = (RigidNode)n;
            node.CreateTransform(robotObject.transform);

            if (!node.CreateMesh("C:\\BXDJTest\\" + node.ModelFileName))
            {
                Debug.Log("Robot not loaded!");
                Destroy(robotObject);
                return false;
            }

            node.CreateJoint(); // WIP.
        }

        return true; // Temp
    }
}
