using System.Collections.Generic;
using System.Linq;
using SynthesisAPI.Proto;
using UnityEngine;

public class ModeManager
{
    public static Vector3 GamepieceSpawnpoint = new Vector3(0, 1, 0);

    private static PracticeMode practiceMode;
    
    public static Mode CurrentMode
    {
        get;
        set;
    }

    private static Dictionary<GameObject, Vector3> _initialPositions = new Dictionary<GameObject, Vector3>();
    private static Dictionary<GameObject, Quaternion> _initialRotations = new Dictionary<GameObject, Quaternion>();

    // for resetting the robot in practice mode
    public static Dictionary<GameObject, Vector3> InitialPositions
    {
        get => _initialPositions;
        set => _initialPositions = value;
    }

    public static Dictionary<GameObject, Quaternion> InitialRotations
    {
        get => _initialRotations;
        set => _initialRotations = value;
    }

    private static List<GamepieceSimObject> _gamepieces = new List<GamepieceSimObject>();

    public static void Start()
    {
        practiceMode = new PracticeMode();
        practiceMode.Start();
    }
    
    public static void Update()
    {
        if (CurrentMode == Mode.Practice)
            practiceMode.Update();
    }

    public static void SetInitialState(GameObject robot)
    {
        InitialPositions.Clear();
        InitialRotations.Clear();
        robot.GetComponentsInChildren<Rigidbody>().ForEach(rb =>
        {
            GameObject go = rb.gameObject;
            InitialPositions.Add(go, go.transform.position);
            InitialRotations.Add(go, go.transform.rotation);
        });
    }

    public static void ResetRobot()
    {
        RobotSimObject robot = RobotSimObject.GetCurrentlyPossessedRobot();
        if (robot == null) return;
        robot.RobotNode.GetComponentsInChildren<Rigidbody>().ForEach(rb =>
        {
            GameObject go = rb.gameObject;
            go.transform.position = InitialPositions[go];
            go.transform.rotation = InitialRotations[go];
        });
    }

    public static void ResetField()
    {
        FieldSimObject field = FieldSimObject.CurrentField;
        if (field != null)
            FieldSimObject.CurrentField.ResetField();
        ResetGamepieces();
    }

    public static void ResetGamepieces()
    {
        _gamepieces.ForEach(gp => { GameObject.Destroy(gp.GamepieceObject); });
        _gamepieces.Clear();
        FieldSimObject currentField = FieldSimObject.CurrentField;
        if (currentField != null)
        {
            FieldSimObject.CurrentField.Gamepieces.ForEach(gp => gp.Reset());
        }
    }

    public static void ResetAll()
    {
        ResetField();
        ResetRobot();
    }

    public static void SpawnGamepiece(float scale = 1.0f, PrimitiveType type = PrimitiveType.Sphere)
    {
        SpawnGamepiece(GamepieceSpawnpoint, scale, type);
    }

    public static void SpawnGamepiece(int x, int y, int z, float scale = 1.0f,
        PrimitiveType type = PrimitiveType.Sphere)
    {
        SpawnGamepiece(new Vector3(x, y, z), scale, type);
    }

    // will change when we have gamepieces from mirabuf
    public static void SpawnGamepiece(Vector3 spawnPosition, float scale = 1.0f,
        PrimitiveType type = PrimitiveType.Sphere)
    {
        FieldSimObject currentField = FieldSimObject.CurrentField;
        // TODO this should be chosen by the user in a dropdown
        GamepieceSimObject gamepiece = PracticeMode.ChosenGamepiece;
        if (currentField == null || gamepiece == null)
        {
            GameObject gameObject = GameObject.CreatePrimitive(type);
            gameObject.AddComponent<Rigidbody>();
            gamepiece = new GamepieceSimObject(type + " Gamepiece", gameObject);
        }
        else
        {
            gamepiece =
                new GamepieceSimObject(gamepiece.Name, gamepiece.GamepieceObject);
            gamepiece.GamepieceObject.transform.position = spawnPosition;
            GameObject.Instantiate(gamepiece.GamepieceObject);
        }

        _gamepieces.Add(gamepiece);
    }

    public enum Mode
    {
        Practice,
        Match
    }
}