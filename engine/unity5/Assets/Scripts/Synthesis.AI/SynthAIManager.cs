using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using NavMeshBuilder = UnityEngine.AI.NavMeshBuilder;
using Assets.Scripts.FSM;

// Place this component on an empty Game Object to turn on AI Capabilites
// Creates NavMesh from objects tagged by SynthAITag
// Tagging the field Game Object with the field tag and calling InitiateAI() will automatically tag all field elements
public class SynthAIManager : MonoBehaviour
{
    private NavMeshData NavMesh;
    private AsyncOperation Operation;
    private NavMeshDataInstance NavMeshInstance;
    private List<NavMeshBuildSource> Sources = new List<NavMeshBuildSource>();

    private bool isInitialized = false;
    private static List<AIRobot> robots;

    // Singleton Instance
    public static SynthAIManager Instance { get; private set; }

    // Reference to tag on Field Game Object.
    public string FieldTag { get { return fieldTag; } private set { fieldTag = value; } }

    [SerializeField] public float AIMaxSpeed = 0.75f; // Max Speed of an AI controlled robot
    [SerializeField] public float AILookAhead = 1.25f; // How far ahead AIs should calculate steering

    [SerializeField] private string fieldTag = "Field"; // Serialization with private setter

    [SerializeField] private Vector3 spawnPoint;
    public Vector3 SpawnPoint { get { return spawnPoint; } set { spawnPoint = new Vector3(value.x, value.y + 0.5f, value.z); } }

    [Header("Do Not Edit -- NavMesh properties")]
    // The center of the build -- Serialized to be visible in Editor, not to be edited in Editor.
    [SerializeField]
    private Transform center;

    // How large the NavMesh should be (Larger sizes lead to decreased performance, 
    // so this should be as small as possible).
    [SerializeField] private Vector3 m_Size;

    private void Awake()
    {
        // Set Singleton Instance. Only one SynthAIManager should be present
        if (SynthAIManager.Instance == null)
        {
            SynthAIManager.Instance = this;
        }
        else
        {
            Debug.Log("ERROR, MORE THAN ONE SYNTHESIS AI MANAGER IS PRESENT");
        }

        // Initialize Robot arraylist
        robots = new List<AIRobot>();
    }

    /// <summary>
    /// Initiates NavMesh generator based on a field tag. Currently, the field tag
    /// is set in this class, but that can be changed at any time.
    /// Also places SynthAITag component on all children Game Objects parented to
    /// the Game Object with the field tag (any Game Object with a SynthAITag component
    /// will be calculated in the making of the field NavMesh).
    /// </summary>
    public static void InitiateNavMesh(string fieldTag)
    {
        // Keep track of start and end times
        float start = Time.realtimeSinceStartup;

        // Get the field object with field tag
        GameObject field = GameObject.FindGameObjectWithTag(fieldTag);

        // Deep search of field returns all children and children's children
        Transform[] children = field.GetComponentsInChildren<Transform>();

        // Get all mesh filters for efficient tracking of child bounds
        Renderer[] renderers = new Renderer[children.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i] = children[i].GetComponent<Renderer>();
        }

        // Calculate center of NavMesh generation
        Vector3 center = Vector3.zero;
        for (int i = 0; i < children.Length; i++)
        {
            Transform t = children[i];

            // Add SynthAITag to all field objects for NavMesh tracking
            t.gameObject.AddComponent<SynthAITag>();


            if (renderers[i] != null)
            {
                center += renderers[i].bounds.center;
            }
        }
        center /= children.Length; //center is average center of children


        // Calculate the size of NavMesh generation
        Bounds bounds = new Bounds(center, Vector3.zero);
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        Instance.center.position = center; // Set center of NavMesh generation
        Instance.m_Size = bounds.size; // Set size of NavMesh generation
        Instance.SpawnPoint = center; // Set Spawn Point to center of field.
        Instance.isInitialized = true;

        Debug.Log("AI NavMesh initiated in " + (Time.realtimeSinceStartup - start) + "seconds!");
    }

    // Update NavMesh as terrain changes
    IEnumerator Start()
    {
        while (true)
        {
            if (isInitialized) // Only update NavMesh if the AI Manager has been initialized
            {
                UpdateNavMesh(true);
            }
            yield return Operation;
        }
    }

    void OnEnable()
    {
        // Construct and add navmesh
        NavMesh = new NavMeshData();
        NavMeshInstance = UnityEngine.AI.NavMesh.AddNavMeshData(NavMesh);
        if (center == null)
            center = transform;
    }

    void OnDisable()
    {
        // Unload navmesh and clear handle
        NavMeshInstance.Remove();
    }

    private void UpdateNavMesh(bool asyncUpdate = false)
    {
        SynthAITag.Collect(ref Sources);
        var defaultBuildSettings = UnityEngine.AI.NavMesh.GetSettingsByID(0);
        var bounds = QuantizedBounds();

        if (asyncUpdate)
            Operation = NavMeshBuilder.UpdateNavMeshDataAsync(NavMesh, defaultBuildSettings, Sources, bounds);
        else
            NavMeshBuilder.UpdateNavMeshData(NavMesh, defaultBuildSettings, Sources, bounds);
    }

    public static AIRobot SpawnRobot(string directory, BaseSynthBehaviour chosenBehaviour)
    {
        MainState main = Instance.transform.GetComponent<StateMachine>().CurrentState as MainState;

        GameObject obj = new GameObject("AI Robot " + (SynthAIManager.robots.Count + 1));

        AIRobot ai = obj.AddComponent<AIRobot>();
        SynthAIManager.robots.Add(ai);
        if (ai.InitializeRobot(directory, main))
        {
            BaseSynthBehaviour behaviour = ai.gameObject.AddComponent(chosenBehaviour.GetType()) as BaseSynthBehaviour;
            behaviour.Initialize(main);
            return ai;
        }
        else
        {
            return null;
        }
    }

    public static void ClearRobots()
    {
        MainState main = Instance.transform.GetComponent<StateMachine>().CurrentState as MainState;

        // Going through list backwards allows for simultaneous iteration and destruction of collection
        for (int i = robots.Count - 1; i >= 0; i--)
        {
            AIRobot robot = robots[i];
            main.SpawnedRobots.Remove(robot);
            SynthAIManager.robots.Remove(robot);
            robot.BeginReset();
        }
    }


    // Helper methods
    private Bounds QuantizedBounds()
    {
        // Quantize the bounds to update only when there's a 10% change in size
        var center = this.center ? this.center.position : transform.position;
        return new Bounds(Quantize(center, 0.1f * m_Size), m_Size);
    }

    private static Vector3 Quantize(Vector3 v, Vector3 quant)
    {
        float x = quant.x * Mathf.Floor(v.x / quant.x);
        float y = quant.y * Mathf.Floor(v.y / quant.y);
        float z = quant.z * Mathf.Floor(v.z / quant.z);
        return new Vector3(x, y, z);
    }

    // See AIManager bounds in editor for debugging.
    private void OnDrawGizmosSelected()
    {
        if (NavMesh)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(NavMesh.sourceBounds.center, NavMesh.sourceBounds.size);
        }

        Gizmos.color = Color.yellow;
        var bounds = QuantizedBounds();
        Gizmos.DrawWireCube(bounds.center, bounds.size);

        Gizmos.color = Color.green;
        var center = this.center ? this.center.position : transform.position;
        Gizmos.DrawWireCube(center, m_Size);
    }
}
