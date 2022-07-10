using Synthesis.UI.Dynamic;
using System.Collections.Generic;
using UnityEngine;

public class ModeManager
{
    public static Vector3 GamepieceSpawnpoint = new Vector3(0, 10, 0);
    private static GameObject _gamepieceSpawnpointObject;

    private static PracticeMode practiceMode;
    public static MatchMode matchMode;
    
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
        if (CurrentMode == Mode.Practice)
        {
            practiceMode = new PracticeMode();
            practiceMode.Start();
        }
        else if (CurrentMode == Mode.Match)
        {
            matchMode = new MatchMode();
            matchMode.Start();
        }
    }
    
    public static void Update()
    {
        if (CurrentMode == Mode.Practice)
            practiceMode.Update();
        else if (CurrentMode == Mode.Match)
            matchMode.Update();
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

    public static void ModalClosed()
    {
        // used to tell practice mode that the modal has closed due to a button
        // so that the user doesn't have to press escape twice to open it again
        if (CurrentMode == Mode.Practice)
        {
            practiceMode.CloseMenu();
        } else if (CurrentMode == Mode.Match)
        {
            // match mode here if necessary
        }
    }

    public static void ConfigureGamepieceSpawnpoint()
    {
        _gamepieceSpawnpointObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _gamepieceSpawnpointObject.transform.localScale = new Vector3(1, 1, 1);
        _gamepieceSpawnpointObject.transform.position = GamepieceSpawnpoint;
        _gamepieceSpawnpointObject.transform.tag = "gamepiece";

        _gamepieceSpawnpointObject.GetComponent<Collider>().enabled = false;
        
        FieldSimObject currentField = FieldSimObject.CurrentField;
        if (currentField != null)
        {
            _gamepieceSpawnpointObject.transform.parent = currentField.FieldObject.transform;
        }
        
        // make it transparent
        Renderer renderer = _gamepieceSpawnpointObject.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Shader Graphs/DefaultSynthesisTransparentShader"));

        GizmoManager.SpawnGizmo(GizmoStore.GizmoPrefabStatic, _gamepieceSpawnpointObject.transform, _gamepieceSpawnpointObject.transform.position);
    }

    public static void EndConfigureGamepieceSpawnpoint()
    {
        GamepieceSpawnpoint = _gamepieceSpawnpointObject.transform.position;
        GameObject.Destroy(_gamepieceSpawnpointObject);
        _gamepieceSpawnpointObject = null;
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

    public static void SpawnGamepiece(Vector3 spawnPosition, float scale = 1.0f,
        PrimitiveType type = PrimitiveType.Sphere)
    {
        FieldSimObject currentField = FieldSimObject.CurrentField;
        GamepieceSimObject gamepiece = PracticeMode.ChosenGamepiece;
        if (currentField == null || gamepiece == null)
        {
            GameObject gameObject = GameObject.CreatePrimitive(type);
            gameObject.transform.position = spawnPosition;
            gameObject.AddComponent<Rigidbody>();
            gamepiece = new GamepieceSimObject(type + " Gamepiece", gameObject);
        }
        else
        {
            GameObject go = GameObject.Instantiate(gamepiece.GamepieceObject);
            go.transform.parent = gamepiece.GamepieceObject.transform.parent;
            go.transform.position = Vector3.zero;
            Transform childTransform = go.transform.GetChild(0);

            if (childTransform != null)
            {
                childTransform.position = spawnPosition;
            }
            
            gamepiece =
                new GamepieceSimObject(gamepiece.Name, go);
        }

        _gamepieces.Add(gamepiece);
    }

    public enum Mode
    {
        Practice,
        Match
    }
}